#if UNITY_IOS
using System.Runtime.InteropServices;
using UnityEngine;
using System;
#endif

namespace VXMonster.Core.Vibration
{
    public class IOSVibrationHandler : SimpleVibrationHandler
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void Initialize();

        [DllImport("__Internal")]
        private static extern void Play(float duration, float intensity);
#endif

        public IOSVibrationHandler(VibrationManager manager) : base(manager)
        {
#if UNITY_IOS
            try
            {
                Initialize();
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
            }
#endif
        }

        public override bool Vibrate(float duration, float intensity)
        {
#if UNITY_IOS
            try 
            {
                Play(duration, intensity);
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