using UnityEngine.Playables;

namespace VXMonster.Core.Timeline
{
    public class MaintainRushWaveBehavior : RushWaveBehavior
    {
        protected int keepAliveCount = -1;
        protected int aliveEnemiesCounter;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            keepAliveCount = EnemiesCount;
            aliveEnemiesCounter = 0;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (aliveEnemiesCounter < keepAliveCount)
            {
                StageController.EnemiesSpawner.SpawnRush(EnemyType, RushSpawnData, WaveOverride, CircularSpawn, keepAliveCount - aliveEnemiesCounter, OnEnemyDied, OnEnemyHidden);

                aliveEnemiesCounter = keepAliveCount;
            }
        }

        protected virtual void OnEnemyDied(EnemyBehavior enemy)
        {
            enemy.onEnemyDied -= OnEnemyDied;
            enemy.onEnemyHidden -= OnEnemyHidden;

            aliveEnemiesCounter--;
        }

        protected virtual void OnEnemyHidden(EnemyBehavior enemy)
        {
            enemy.onEnemyDied -= OnEnemyDied;
            enemy.onEnemyHidden -= OnEnemyHidden;

            aliveEnemiesCounter--;
        }
    }
}