using VXMonster.Core.Bossfight;
using VXMonster.Core.Extensions;
using VXMonster.Core.Pool;
using VXMonster.Core.Timeline;
using VXMonster.Core.UI;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using VXMonster.Gameplay;

namespace VXMonster.Core
{
    [DefaultExecutionOrder(-10)]
    public class EnemiesSpawner : MonoBehaviour
    {
        protected List<EnemyBehavior> enemies = new List<EnemyBehavior>();

        [SerializeField] protected EnemiesDatabase database;
        [SerializeField] protected BossfightDatabase bossfightDatabase;

        [Space]
        [SerializeField] protected ScalingLabelBehavior enemiesDiedLabel;

        [Space]
        [Tooltip("Maximum amount of alive enemies at a time. No more enemies will be spawned until some of existing aren't defeated")]
        [SerializeField] protected int enemiesCap = 3000;

        [Header("Offscreen Teleport")]
        [Tooltip("When enabled, enemies that are behaind of the player will teleport to the front")]
        [SerializeField] protected bool isOffscreenTeleportEnabled = true;
        [Tooltip("Distance from the player to enemy, where 1 is a camera diagonal length")]
        [SerializeField] protected float diagonalDistanceMultiplier = 1.3f;
        [SerializeField, Range(0, 1f)] protected float teleportConeSize = 0.8f;

        [Tooltip("Enemy will stop teleporting if there are more than this amount of enemies")]
        [SerializeField] protected int enemiesTeleportCap = 200;

        protected int enemiesDiedCounter;

        protected Dictionary<EnemyType, PoolComponent<EnemyBehavior>> enemyPools;
        protected Dictionary<EnemyType, EnemyData> enemyDataDictionary;

        protected StageSave stageSave;

        public bool IsBossfightActive { get; set; }

        protected Camera mainCamera;

        protected virtual void Awake()
        {
            mainCamera = Camera.main;
        }

        // Were creating pools only for the enemies that are present in the Stage Timeline
        public virtual void Init(PlayableDirector director)
        {
            enemyDataDictionary = database.GetEnemyDataDictionary();

            stageSave = GameController.SaveManager.GetSave<StageSave>("Stage");

            Dictionary<EnemyType, int> enemiesOnLevel = new Dictionary<EnemyType, int>();

            var waves = director.GetAssets<WaveTrack, WaveAsset>();

            for(int i = 0; i < waves.Count; i++)
            {
                var wave = waves[i];
                var enemyType = wave.EnemyType;
                var enemiesCount = wave.EnemiesCount;

                if (enemiesOnLevel.ContainsKey(enemyType))
                {
                    if (enemiesOnLevel[enemyType] < enemiesCount)
                    {
                        enemiesOnLevel[enemyType] = enemiesCount;
                    }
                }
                else
                {
                    enemiesOnLevel.Add(enemyType, enemiesCount);
                }
            }

            var trackEnemies = new List<EnemyType>();
            foreach (var output in director.playableAsset.outputs)
            {
                if(output.sourceObject is WaveTrack waveTrack)
                {
                    if (!trackEnemies.Contains(waveTrack.EnemyType))
                    {
                        trackEnemies.Add(waveTrack.EnemyType);
                    }
                }
            }

            enemyPools = new Dictionary<EnemyType, PoolComponent<EnemyBehavior>>();

            foreach(var enemyType in enemiesOnLevel.Keys)
            {
                var data = database.GetEnemyData(enemyType);

                var amount = enemiesOnLevel[enemyType];
                if (amount > 100) amount = 100;
                if (amount < 0) amount = 1;

                var pool = new PoolComponent<EnemyBehavior>($"Enemy {enemyType}", data.Prefab, amount);

                enemyPools.Add(data.Type, pool);
            }

            foreach(var enemyType in trackEnemies)
            {
                if (!enemyPools.ContainsKey(enemyType))
                {
                    var data = database.GetEnemyData(enemyType);
                    var pool = new PoolComponent<EnemyBehavior>($"Enemy {enemyType}", data.Prefab, 1);

                    enemyPools.Add(data.Type, pool);
                }
            }

            enemiesDiedCounter = 0;
            if (!stageSave.ResetStageData)
            {
                enemiesDiedCounter = stageSave.EnemiesKilled;
            }
            
            enemiesDiedLabel.SetAmount(enemiesDiedCounter);
        }

        protected virtual void Update()
        {
            if (!isOffscreenTeleportEnabled || IsBossfightActive || enemies.Count > enemiesTeleportCap) return;

            var diagonalSqr = (CameraManager.HalfWidth * CameraManager.HalfWidth + CameraManager.HalfHeight * CameraManager.HalfHeight) * diagonalDistanceMultiplier;
            var diagonal = Mathf.Sqrt(diagonalSqr);

            var dotValue = teleportConeSize - 1;
            var modValue = Mathf.Clamp(enemies.Count / 20, 1, 100);
            int frame = Time.frameCount % modValue;
            for(int i = frame; i < enemies.Count; i += modValue)
            {
                var enemy = enemies[i];

                if (enemy.WaveOverride != null && enemy.WaveOverride.DisableOffscreenTeleport || enemy.IsRushing) continue;

                var enemyToPlayer = enemy.transform.position - PlayerBehavior.Player.transform.position;
                var direction = enemyToPlayer.normalized;
                var dot = Vector2.Dot(direction, PlayerBehavior.Player.LookDirection);

                if(diagonalSqr < enemyToPlayer.sqrMagnitude && dot < dotValue)
                {
                    var teleportPosition = PlayerBehavior.Player.transform.position + Quaternion.Euler(0, 0, Random.Range(-45, 45)) * PlayerBehavior.Player.LookDirection * diagonal;
                    if (StageController.FieldManager.ValidatePosition(teleportPosition, Vector2.zero, false))
                    {
                        enemy.transform.position = teleportPosition;
                    }
                }
            }
        }

        public virtual EnemyBehavior GetClosestEnemy(Vector2 point)
        {
            EnemyBehavior closestEnemy = null;
            float closestDistance = float.MaxValue;

            for(int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == null)
                {
                    enemies.RemoveAt(i);
                    i--;
                    continue;
                }

                float distance = (point - enemies[i].transform.position.XY()).sqrMagnitude;

                if(distance < closestDistance)
                {
                    closestEnemy = enemies[i];
                    closestDistance = distance;
                }
            }

            return closestEnemy;
        }

        public virtual EnemyBehavior Spawn(EnemyType enemyType, Vector2 position, UnityAction<EnemyBehavior> onEnemyDiedCallback = null)
        {
            if (enemies.Count >= enemiesCap) return null;

            var enemyData = enemyDataDictionary[enemyType];

            if (!enemyPools.ContainsKey(enemyType))
            {
                var pool = new PoolComponent<EnemyBehavior>($"Enemy {enemyType}", enemyData.Prefab, 10);

                enemyPools.Add(enemyData.Type, pool);
            }

            var enemy = enemyPools[enemyType].GetEntity();

            enemy.SetData(enemyData);

            enemy.transform.position = position;

            enemy.onEnemyDied += OnEnemyDied;
            if (onEnemyDiedCallback != null) enemy.onEnemyDied += onEnemyDiedCallback;

            enemy.Play();

            enemies.Add(enemy);

            return enemy;
        }

        public virtual void Spawn(EnemyType type, WaveOverride waveOverride, bool circularSpawn = false, int amount = 1, UnityAction<EnemyBehavior> onEnemyDiedCallback = null)
        {
            var cameraHeight = mainCamera.orthographicSize;
            var cameraWidth = cameraHeight * mainCamera.aspect;
            var cameraDiagonal = Mathf.Sqrt(cameraWidth * cameraWidth + cameraHeight * cameraHeight);

            for (int i = 0; i < amount; i++)
            {
                if (enemies.Count >= enemiesCap) return;

                var enemy = enemyPools[type].GetEntity();

                enemy.SetData(enemyDataDictionary[type]);
                enemy.SetWaveOverride(waveOverride);

                var triesCount = 0;
                var maxTriesCount = 10;

                var position = Vector3.zero;
                var foundPosition = false;

                while(triesCount < maxTriesCount)
                {
                    triesCount++;

                    // when spawning a lot of enemies at once we want to offset them to not overload physics computations
                    var additionalDistance = amount > 100 ? Mathf.Sqrt(enemies.Count) * 0.1f : 0;
                    if (circularSpawn)
                    {
                        position = PlayerBehavior.Player.transform.position + Random.onUnitSphere.SetZ(0).normalized * (cameraDiagonal * 1.05f + Random.value * 0.2f + additionalDistance);
                    } else
                    {
                        position = CameraManager.GetRandomPointOutsideCamera(0.5f + Random.value * 0.2f + additionalDistance);
                    }
                    
                    if(StageController.FieldManager.ValidatePosition(position, Vector2.zero))
                    {
                        foundPosition = true;
                        break;
                    }
                }

                if (!foundPosition)
                {
                    for (int j = 1; j < 10; j++)
                    {
                        var middlePosition = Vector3.Lerp(position, PlayerBehavior.Player.transform.position, 1 - j / 10f);

                        if (StageController.FieldManager.ValidatePosition(middlePosition, Vector2.zero))
                        {
                            foundPosition = true;
                            position = middlePosition;
                            break;
                        }
                    }
                }

                if (!foundPosition)
                {
                    position = StageController.FieldManager.GetRandomPositionOnBorder();
                }

                enemy.transform.position = position;

                enemy.onEnemyDied += OnEnemyDied;
                if (onEnemyDiedCallback != null) enemy.onEnemyDied += onEnemyDiedCallback;

                enemy.Play();

                enemies.Add(enemy);
            }
        }

        public virtual void SpawnRush(EnemyType type, RushSpawn rushData, WaveOverride waveOverride, bool circularSpawn = false, int amount = 1, UnityAction<EnemyBehavior> onEnemyDiedCallback = null, UnityAction<EnemyBehavior> onEnemyHiddenCallback = null)
        {
            for(int i = 0; i < amount; i++)
            {
                if (enemies.Count >= enemiesCap) return;

                rushData.GetSpawnData(out var direction, out var offset);

                var perpendicular = new Vector2(-direction.y, direction.x);

                var corner = GetRushCorner(direction);
                var spawnLineCenter = GetSpawnLineCenter(corner, direction);

                var position = spawnLineCenter - perpendicular * CameraManager.BiggerSide * offset - Random.value * direction;

                var enemy = enemyPools[type].GetEntity();

                enemy.SetData(enemyDataDictionary[type]);
                enemy.SetWaveOverride(waveOverride);
                enemy.SetRushDirection(direction);

                enemy.transform.position = position;

                enemy.onEnemyDied += OnEnemyDied;
                enemy.onEnemyHidden += OnEnemyHidden;

                if (onEnemyDiedCallback != null) enemy.onEnemyDied += onEnemyDiedCallback;
                if (onEnemyHiddenCallback != null) enemy.onEnemyHidden += onEnemyHiddenCallback;

                enemy.Play();

                enemies.Add(enemy);
            }
        }

        protected virtual Vector2 GetRushCorner(Vector2 direction)
        {
            var x = direction.x > 0 ? CameraManager.LeftBound : CameraManager.RightBound;
            var y = direction.y > 0 ? CameraManager.BottomBound : CameraManager.TopBound;
            
            return new Vector2(x, y);
        }

        public static Vector2 GetSpawnLineCenter(Vector2 corner, Vector2 direction)
        {
            var t = Vector2.Dot(corner - CameraManager.Position, direction);

            return CameraManager.Position + (direction * t);
        }

        public virtual EnemyBehavior GetRandomVisibleEnemy()
        {
            if (enemies.Count == 0) return null;

            // Trying to find random visible enemy 10 times
            for(int i = 0; i < 10; i++)
            {
                var randomIndex = Random.Range(0, enemies.Count);

                var enemy = enemies[randomIndex];

                if (enemy.IsVisible) return enemy;
            }

            for(int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];

                if (enemy.IsVisible) return enemy;
            }

            return null;
        }

        public virtual List<EnemyBehavior> GetEnemiesInRadius(Vector2 position, float radius)
        {
            var result = new List<EnemyBehavior>();

            float radiusSqr = radius * radius;

            for(int i = 0; i < enemies.Count; i++)
            {
                if ((enemies[i].transform.position.XY() - position).sqrMagnitude <= radiusSqr)
                {
                    result.Add(enemies[i]);
                }
            }

            return result;
        }

        public virtual void KillEveryEnemy()
        {
            foreach(var enemy in enemies)
            {
                enemy.onEnemyDied -= OnEnemyDied;
                enemy.onEnemyHidden -= OnEnemyHidden;
                enemy.Kill();
            }

            enemiesDiedCounter += enemies.Count;
            stageSave.EnemiesKilled = enemiesDiedCounter;

            enemiesDiedLabel.SetAmount(enemiesDiedCounter);

            enemies.Clear();
        }

        public virtual void DealDamageToAllEnemies(float damage)
        {
            var aliveEnemies = new List<EnemyBehavior>();

            foreach(var enemy in enemies)
            {
                if(enemy.HP <= damage)
                {
                    // if enemy is not a boss
                    if(enemy.Data != null)
                    {
                        enemy.onEnemyDied -= OnEnemyDied;
                        enemy.onEnemyHidden -= OnEnemyHidden;
                        enemy.Kill();

                        foreach (var dropData in enemy.GetDropData())
                            TrySpawnEnemyDrop(enemy, dropData);
                    } else
                    {
                        aliveEnemies.Add(enemy);
                    }
                }
                else
                {
                    // if enemy is not a boss
                    if (enemy.Data != null)
                    {
                        enemy.TakeDamage(damage);
                    }
                    aliveEnemies.Add(enemy);
                }
            }

            enemiesDiedCounter += enemies.Count - aliveEnemies.Count;

            stageSave.EnemiesKilled = enemiesDiedCounter;
            enemiesDiedLabel.SetAmount(enemiesDiedCounter);

            enemies.Clear();
            enemies.AddRange(aliveEnemies);
        }

        protected virtual void OnEnemyDied(EnemyBehavior enemy)
        {
            enemies.RemoveSwapBack(enemy);
            enemy.onEnemyDied -= OnEnemyDied;
            enemy.onEnemyHidden -= OnEnemyHidden;

            foreach(var dropData in enemy.GetDropData())
                TrySpawnEnemyDrop(enemy, dropData);

            enemiesDiedCounter++;
            stageSave.EnemiesKilled = enemiesDiedCounter;
            enemiesDiedLabel.SetAmount(enemiesDiedCounter);
        }

        protected virtual void OnEnemyHidden(EnemyBehavior enemy)
        {
            enemies.RemoveSwapBack(enemy);
            enemy.onEnemyDied -= OnEnemyDied;
            enemy.onEnemyHidden -= OnEnemyHidden;
        }

        protected virtual void TrySpawnEnemyDrop(EnemyBehavior enemy, EnemyDropData dropData)
        {
            if (enemy == null || dropData == null || dropData.Chance == 0) return;
            if (!StageController.IsLoaded || StageController.DropManager == null) return;

            var tierBias = RunXpScaling.GetCurrentGemTierBias();
            var rollChance = RunXpScaling.GetBiasedGemDropChance(dropData.DropType, dropData.Chance, tierBias);
            if (Random.value * 100f > rollChance) return;

            var dropType = RunXpScaling.ResolveGemDrop(dropData.DropType, tierBias);
            if (!StageController.DropManager.CheckDropCooldown(dropType)) return;

            StageController.DropManager.Drop(
                dropType,
                enemy.transform.position.XY() + Random.insideUnitCircle * 0.2f);
        }

        protected virtual void OnBossDied(EnemyBehavior boss)
        {
            enemies.RemoveSwapBack(boss);
            boss.onEnemyDied -= OnBossDied;

            TalentPointUtility.AwardBossKillPoints();

            if (boss.ShouldSpawnChestOnDeath && StageController.AbilityManager.HasAvailableAbilities()) StageController.DropManager.Drop(DropType.Chest, boss.transform.position.XY() + Random.insideUnitCircle);
            StageController.DropManager.Drop(DropType.Magnet, boss.transform.position.XY() + Random.insideUnitCircle);
            StageController.DropManager.Drop(DropType.Food, boss.transform.position.XY() + Random.insideUnitCircle);

            enemiesDiedCounter++;
            stageSave.EnemiesKilled = enemiesDiedCounter;
            enemiesDiedLabel.SetAmount(enemiesDiedCounter);
        }

        public virtual EnemyBehavior SpawnBoss(BossType bossType, Vector2 spawnPosition, UnityAction<EnemyBehavior> onBossDied = null)
        {
            var bossData = bossfightDatabase.GetBossfight(bossType);

            var boss = Instantiate(bossData.BossPrefab).GetComponent<EnemyBehavior>();
            boss.transform.position = spawnPosition;

            boss.IsBoss = true;
            boss.Play();

            boss.onEnemyDied += OnBossDied;
            boss.onEnemyDied += onBossDied;

            enemies.Add(boss);

            return boss;
        }

        public virtual BossfightData GetBossData(BossType bossType)
        {
            return bossfightDatabase.GetBossfight(bossType);
        }
    }
}