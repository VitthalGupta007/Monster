using UnityEngine;
using UnityEngine.Playables;

namespace VXMonster.Core.Timeline
{
    public class BurstWave : WaveAsset
    {
        [Header("Burst Settings")]
        [SerializeField, Min(1)] int enemiesCount = 1;
        [SerializeField, Min(1)] int burstCount = 1;

        public override int EnemiesCount => (int)(enemiesCount * 1.2f);

        protected BurstWaveBehavior template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<BurstWaveBehavior>.Create(graph, template);
            var waveData = wavePlayable.GetBehaviour();

            waveData.EnemyType = EnemyType;
            
            waveData.BurstCount = burstCount;
            waveData.EnemiesCount = enemiesCount;

            waveData.WaveOverride = waveOverride;
            waveData.CircularSpawn = circularSpawn;

            return wavePlayable;
        }

        private void OnValidate()
        {
            if (enemiesCount < 0) enemiesCount = 0;
            if (burstCount < 0) burstCount = 0;
        }
    }
}