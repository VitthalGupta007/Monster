using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.U2D;

namespace OctoberStudio.Timeline
{
    public class CameraControlBehavior : PlayableBehaviour
    {
        public float TargetCameraSize { get; set; }
        public EasingType Easing { get; set; }

        private float startCameraSize;
        private PixelPerfectCamera pixelPerfectCamera;

        private int startPPU;
        private int targetPPU;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            pixelPerfectCamera = Camera.main.GetComponent<PixelPerfectCamera>();

            if(pixelPerfectCamera == null)
            {
                startCameraSize = Camera.main.orthographicSize;
            } else
            {
                startCameraSize = Camera.main.orthographicSize;
                startPPU = pixelPerfectCamera.assetsPPU;

                targetPPU = (int)(pixelPerfectCamera.assetsPPU / (TargetCameraSize / startCameraSize));
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {       
            float time = (float)playable.GetTime();
            float duration = (float)playable.GetDuration();

            float t = EasingFunctions.ApplyEasing(time / duration, Easing);

            if (pixelPerfectCamera != null) pixelPerfectCamera.assetsPPU = (int) Mathf.Lerp(startPPU, targetPPU, t);
            StageController.CameraController.SetSize(Mathf.Lerp(startCameraSize, TargetCameraSize, t));
        }
    }
}