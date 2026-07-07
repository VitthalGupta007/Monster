#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine;
using System;
#endif

using UnityEngine.InputSystem;

namespace OctoberStudio.Vibration
{
    public class WebGLVibrationHandler : SimpleVibrationHandler
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void GamepadVibrate(int duration, float weak, float strong);

        [DllImport("__Internal")]
        private static extern void Vibrate(int duration);
#endif

        public WebGLVibrationHandler(VibrationManager manager) : base(manager)
        {

        }

        public override bool Vibrate(float duration, float intensity)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try 
            {
                Vibrate((int)(duration * 1000));
                return true;
            }
            catch
            {
                return false;
            }
#else
            return false;
#endif
        }

        public override bool VibrateGamepad(float duration, float intensity)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                GamepadVibrate((int)(duration * 1000), intensity, intensity);
                return true;
            }
            catch
            {
                return false;
            }
#else
            return false;
#endif
        }

    }
}