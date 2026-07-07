using UnityEngine.Playables;

namespace VXMonster.Core.Timeline
{
    public class WaveBehavior : PlayableBehaviour
    {
        public EnemyType EnemyType { get; set; }
        public int EnemiesCount { get; set; }

        public WaveOverride WaveOverride { get; set; }
        public bool CircularSpawn { get; set; }
    }
}
