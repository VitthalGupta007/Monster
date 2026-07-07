using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class EnemyMegaSlimeBehavior : EnemyBehavior
    {
        private static readonly int SPAWN_TRIGGER = Animator.StringToHash("Spawn");
        private static readonly int CHARGE_TRIGGER = Animator.StringToHash("Charge");

        [SerializeField] Animator animator;

        [Header("Swords")]
        [Tooltip("The prefab of the sword")]
        [SerializeField] GameObject swordProjectilePrefab;
        [Tooltip("The amount of waves of the sword attack")]
        [SerializeField] int swordsWavesCount = 3;
        [Tooltip("Time between waves of the sword attack")]
        [SerializeField] float durationBetweenSwordWaves = 1f;
        [Tooltip("The damage of each sword")]
        [SerializeField] float swordDamage = 10f;
        [Tooltip("The swords will spawn at this points")]
        [SerializeField] List<Transform> swordSpawnPoints;

        [Header("Moving")]
        [Tooltip("Time the slime moves between attacks")]
        [SerializeField] float movingDuration = 3f;

        [Header("Spawning")]
        [Tooltip("The type of an enemy that will be spawned during this attack")]
        [SerializeField] EnemyType spawnedEnemyType = EnemyType.Slime;
        [Tooltip("The amount of waves of the spawn attack")]
        [SerializeField] int spawningWavesCount;
        [Tooltip("The amount of enemies that will be spawned in one wave of the spawn attack")]
        [SerializeField] int spawnedEnemiesCount;
        [Tooltip("The time between waves of spawnAttacks")]
        [SerializeField] float durationBetweenSpawning = 1f;
        [Tooltip("The time the warning circle is active before the enemy is spawned")]
        [SerializeField] float durationBetweenWarningAndSpawning = 0.5f;

        [Space]
        [SerializeField] ParticleSystem spawningParticle;

        List<WarningCircleBehavior> warningCircles = new List<WarningCircleBehavior>();

        private PoolComponent<EnemyMegaSlimeProjectileBehavior> swordsPool;

        private List<EnemyMegaSlimeProjectileBehavior> swords = new List<EnemyMegaSlimeProjectileBehavior>();

        protected override void Awake()
        {
            base.Awake();

            swordsPool = new PoolComponent<EnemyMegaSlimeProjectileBehavior>(swordProjectilePrefab, 6);
        }

        public override void Play()
        {
            base.Play();

            StartCoroutine(BehaviorCoroutine());
        }

        private IEnumerator BehaviorCoroutine()
        {
            while(true)
            {
                IsMoving = false;

                for (int i = 0; i < swordsWavesCount; i++) {

                    animator.SetTrigger(CHARGE_TRIGGER);
                    yield return new WaitForSeconds(durationBetweenSwordWaves);
                }
                IsMoving = true;

                yield return new WaitForSeconds(movingDuration);

                IsMoving = false;

                for (int i = 0; i < spawningWavesCount; i++)
                {
                    StartCoroutine(SpawnCoroutine());

                    yield return new WaitForSeconds(durationBetweenSpawning);
                }

                IsMoving = true;

                yield return new WaitForSeconds(movingDuration);
            }
        }

        public void SwordsAttack()
        {
            StartCoroutine(SwordsAttackCoroutine());
        }

        private IEnumerator SpawnCoroutine()
        {
            spawningParticle.Play();

            animator.SetTrigger(SPAWN_TRIGGER);

            for (int j = 0; j < spawnedEnemiesCount; j++)
            {
                var spawnPosition = StageController.FieldManager.Fence.GetRandomPointInside(0.5f);

                var warningCircle = StageController.PoolsManager.GetEntity<WarningCircleBehavior>("Warning Circle");

                warningCircle.transform.position = spawnPosition;

                warningCircle.Play(1f, 0.3f, 100, null);

                warningCircles.Add(warningCircle);
            }

            yield return new WaitForSeconds(durationBetweenWarningAndSpawning);

            for(int i = 0; i < spawnedEnemiesCount; i++)
            {
                var warningCircle = warningCircles[i];

                StageController.EnemiesSpawner.Spawn(spawnedEnemyType, warningCircle.transform.position);

                warningCircle.gameObject.SetActive(false);
            }

            warningCircles.Clear();

            spawningParticle.Stop();
        }

        private IEnumerator SwordsAttackCoroutine()
        {
            for(int i = 0; i < swordSpawnPoints.Count; i++)
            {
                var spawnPoint = swordSpawnPoints[i];

                var sword = swordsPool.GetEntity();

                sword.Init(spawnPoint.transform.position, Vector2.zero);
                sword.Damage = StageController.Stage.EnemyDamage * swordDamage;
                sword.onFinished += OnSwordFinished;

                swords.Add(sword);

                yield return new WaitForSeconds(0.2f);
            }
        }

        private void OnSwordFinished(EnemyMegaSlimeProjectileBehavior sword)
        {
            sword.onFinished -= OnSwordFinished;

            swords.Remove(sword);
        }

        protected override void Die(bool flash)
        {
            base.Die(flash);

            for(int i = 0; i < swords.Count; i++)
            {
                var sword = swords[i];

                sword.onFinished -= OnSwordFinished;

                sword.Clear();
            }

            for(int i = 0; i < warningCircles.Count; i++)
            {
                warningCircles[i].gameObject.SetActive(false);
            }

            warningCircles.Clear();

            StopAllCoroutines();
        }
    }
}