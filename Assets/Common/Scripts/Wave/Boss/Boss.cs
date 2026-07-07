using OctoberStudio.Bossfight;
using UnityEngine;
using UnityEngine.Playables;

namespace OctoberStudio.Timeline.Bossfight
{
    public class Boss : PlayableAsset
    {
        [SerializeField] BossType bossType;
        public BossType BossType => bossType;

        [Space]
        [SerializeField, Min(1f)] float bossUIWarningDuration = 2f;
        
        [SerializeField, Min(1f)] float bossRedCircleSpawnDuration = 1f;
        [SerializeField, Min(1f)] float bossRedCircleStayDuration = 2f;

        [SerializeField] Vector2 bossSpawnOffsetFromPlayer = Vector2.up * 1.5f;

        [Space]
        [SerializeField] bool shouldSpawnChest;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var cameraPlayable = ScriptPlayable<BossBehavior>.Create(graph);

            var behavior = cameraPlayable.GetBehaviour();

            behavior.BossType = bossType;
            behavior.ShouldSpawnChest = shouldSpawnChest;
            behavior.WarningDuration = bossUIWarningDuration;
            behavior.BossSpawnOffset = bossSpawnOffsetFromPlayer;
            behavior.BossRedCircleSpawnDuration = bossRedCircleSpawnDuration;
            behavior.BossRedCircleStayDuration = bossRedCircleStayDuration;

            return cameraPlayable;
        }
    }
}