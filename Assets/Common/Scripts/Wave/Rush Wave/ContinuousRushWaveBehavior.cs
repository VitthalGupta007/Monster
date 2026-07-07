using UnityEngine.Playables;

namespace VXMonster.Core.Timeline
{
    public class ContinuousRushWaveBehavior : RushWaveBehavior
    {
        public float ContinuousSpawnPerSecond { get; set; }

        protected float spawnRate;
        protected float lastSpawnTime;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            spawnRate = 1f / ContinuousSpawnPerSecond;
            lastSpawnTime = -spawnRate;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            float time = (float)playable.GetTime();

            int amount = 0;
            while (lastSpawnTime + spawnRate < time)
            {
                lastSpawnTime += spawnRate;
                amount++;
            }

            if (amount > 0)
            {
                StageController.EnemiesSpawner.SpawnRush(EnemyType, RushSpawnData, WaveOverride, CircularSpawn, amount);
            }
        }
    }
}