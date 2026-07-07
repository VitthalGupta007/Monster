using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline
{
    public class MaintainWave : WaveAsset
    {
        [Header("Maintain Settings")]
        [SerializeField, Min(1)] int enemiesCount = 1;

        public override int EnemiesCount => enemiesCount;

        protected MaintainWaveBehavior template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<MaintainWaveBehavior>.Create(graph, template);
            var waveData = wavePlayable.GetBehaviour();

            waveData.EnemyType = EnemyType;

            waveData.EnemiesCount = enemiesCount;
            waveData.WaveOverride = waveOverride;
            waveData.CircularSpawn = circularSpawn;

            return wavePlayable;
        }

        private void OnValidate()
        {
            if (enemiesCount < 0) enemiesCount = 0;
        }
    }
}