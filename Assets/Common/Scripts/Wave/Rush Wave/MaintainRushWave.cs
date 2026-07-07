using UnityEngine;
using UnityEngine.Playables;

namespace VXMonster.Core.Timeline
{
    public class MaintainRushWave : RushWave
    {
        [Header("Maintain Settings")]
        [SerializeField, Min(1)] int enemiesCount = 1;

        public override int EnemiesCount => enemiesCount;

        protected MaintainRushWaveBehavior template;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var wavePlayable = ScriptPlayable<MaintainRushWaveBehavior>.Create(graph, template);
            var waveData = wavePlayable.GetBehaviour();

            waveData.EnemyType = EnemyType;

            waveData.EnemiesCount = enemiesCount;
            waveData.WaveOverride = waveOverride;
            waveData.CircularSpawn = circularSpawn;

            waveData.RushSpawnData = rushSpawnData;

            return wavePlayable;
        }

        protected virtual void OnValidate()
        {
            if (enemiesCount < 0) enemiesCount = 0;
        }
    }
}