using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class ContinuousWaveBehavior : WaveBehavior
    {
        public float ContinuousSpawnPerSecond { get; set; }

        private float spawnRate;
        private float lastSpawnTime;

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

            if(amount > 0)
            {
                StageController.EnemiesSpawner.Spawn(EnemyType, WaveOverride, CircularSpawn, amount);
            }
        }
    }
}