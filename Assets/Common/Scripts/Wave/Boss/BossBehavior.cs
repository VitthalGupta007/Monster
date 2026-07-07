using OctoberStudio.Bossfight;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline.Bossfight
{
    public class BossBehavior : PlayableBehaviour
    {
        public BossType BossType { get; set; }
        public GameObject FencePrefab { get; set; }
        public bool ShouldSpawnChest { get; set; }

        public float WarningDuration { get; set; }
        public float BossRedCircleStayDuration { get; set; }
        public float BossRedCircleSpawnDuration { get; set; }
        public Vector2 BossSpawnOffset { get; set; }

        bool hasStarted = false;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (hasStarted) return;

            hasStarted = true;

            StageController.GameScreen.ShowBossfightWarning();

            EasingManager.DoAfter(WarningDuration, () => {
                StageController.Director.Pause();

                StageController.GameScreen.HideBossFightWarning();

                // Doing it next frame because there still could be spawns during this one
                EasingManager.DoNextFrame().SetOnFinish(StageController.EnemiesSpawner.KillEveryEnemy);

                var bossSpawnPosition = StageController.FieldManager.SpawnFence(BossType, BossSpawnOffset);

                if (StageController.Stage.RemovePropFromBossfight && StageController.Stage.SpawnProp)
                {
                    StageController.FieldManager.RemovePropFromFence();
                }

                EasingManager.DoAfter(0.3f, () => 
                {
                    var data = StageController.EnemiesSpawner.GetBossData(BossType);
                    StageController.GameScreen.ShowBossHealthBar(data);
                });

                var warningPool = StageController.PoolsManager.GetPool("Warning Circle");
                var warning = warningPool.GetEntity<WarningCircleBehavior>();

                warning.transform.position = bossSpawnPosition;
                warning.Play(2, BossRedCircleSpawnDuration, BossRedCircleStayDuration, () => 
                {
                    var boss = StageController.EnemiesSpawner.SpawnBoss(BossType, bossSpawnPosition, OnBossDied);
                    boss.ShouldSpawnChestOnDeath = ShouldSpawnChest;
                    StageController.GameScreen.LinkBossToHealthbar(boss);
                });

                StageController.EnemiesSpawner.IsBossfightActive = true;
            });
        }

        private void OnBossDied(EnemyBehavior boss)
        {
            boss.onEnemyDied -= OnBossDied;

            StageController.GameScreen.HideBossHealthbar();

            StageController.FieldManager.RemoveFence();
            StageController.Director.Play();

            StageController.EnemiesSpawner.IsBossfightActive = false;
        }
    }
}