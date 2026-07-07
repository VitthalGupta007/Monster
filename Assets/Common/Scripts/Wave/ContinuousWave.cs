using UnityEngine;
using UnityEngine.Playables;

namespace VXMonster.Core.Timeline
{
    public class ContinuousWave : WaveAsset
    {
        [Header("Continuous Settings")]
        [SerializeField, Min(0.01f)] float continuousSpawnPerSecond = 1;

        public override int EnemiesCount => (int)(continuousSpawnPerSecond * duration / 2);

        protected ContinuousWaveBehavior template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<ContinuousWaveBehavior>.Create(graph, template);
            var waveData = wavePlayable.GetBehaviour();

            waveData.EnemyType = EnemyType;
            waveData.ContinuousSpawnPerSecond = continuousSpawnPerSecond;

            waveData.WaveOverride = waveOverride;
            waveData.CircularSpawn = circularSpawn;

            return wavePlayable;
        }

        private void OnValidate()
        {
            if (continuousSpawnPerSecond < 0) continuousSpawnPerSecond = 0;
        }
    }
}