using VXMonster.Core.Easing;
using UnityEngine;
using UnityEngine.Playables;

namespace VXMonster.Core.Timeline
{
    public class CameraControl : PlayableAsset
    {
        [SerializeField] float targetCameraSize = 10;
        [SerializeField] EasingType easingType = EasingType.Linear;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var cameraPlayable = ScriptPlayable<CameraControlBehavior>.Create(graph);

            var behavior = cameraPlayable.GetBehaviour();

            behavior.TargetCameraSize = targetCameraSize;
            behavior.Easing = easingType;

            return cameraPlayable;
        }
    }
}

