using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class BurstRushWave : RushWave
    {
        [Header("Burst Settings")]
        [SerializeField, Min(1)] int enemiesCount = 1;
        [SerializeField, Min(1)] int burstCount = 1;

        public override int EnemiesCount => enemiesCount;

        protected BurstRushWaveBehavior template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<BurstRushWaveBehavior>.Create(graph, template);
            var waveData = wavePlayable.GetBehaviour();

            waveData.EnemyType = EnemyType;

            waveData.BurstCount = burstCount;
            waveData.EnemiesCount = enemiesCount;

            waveData.WaveOverride = waveOverride;
            waveData.CircularSpawn = circularSpawn;

            waveData.RushSpawnData = RushSpawnData;

            return wavePlayable;
        }

        protected virtual void OnValidate()
        {
            if (enemiesCount < 1) enemiesCount = 1;
            if (burstCount < 1) burstCount = 1;
        }
    }
}