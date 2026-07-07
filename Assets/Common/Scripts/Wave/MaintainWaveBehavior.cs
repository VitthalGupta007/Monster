using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class MaintainWaveBehavior : WaveBehavior
    {
        private int keepAliveCount = -1;
        private int aliveEnemiesCounter;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            keepAliveCount = EnemiesCount;
            aliveEnemiesCounter = 0;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (aliveEnemiesCounter < keepAliveCount)
            {
                StageController.EnemiesSpawner.Spawn(EnemyType, WaveOverride, CircularSpawn, keepAliveCount - aliveEnemiesCounter, OnEnemyDied);

                aliveEnemiesCounter = keepAliveCount;
            }
        }

        private void OnEnemyDied(EnemyBehavior enemy)
        {
            enemy.onEnemyDied -= OnEnemyDied;
            aliveEnemiesCounter--;
        }
    }
}