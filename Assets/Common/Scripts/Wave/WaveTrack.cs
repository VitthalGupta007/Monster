using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace OctoberStudio.Timeline
{
    [TrackClipType(typeof(WaveAsset))]
    public class WaveTrack : TrackAsset
    {
        [SerializeField] EnemyType enemyType;
        public EnemyType EnemyType => enemyType;

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            if(clip.asset is WaveAsset wave)
            {
                wave.EnemyType = enemyType;
            }

            var playable = base.CreatePlayable(graph, gameObject, clip);

            return playable;
        }
    }
}