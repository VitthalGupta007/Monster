using System;
using UnityEngine;

namespace VXMonster.Core.Vibration
{
    public class AndroidVibrationHandler : SimpleVibrationHandler
    {
        private AndroidJavaObject vibrationService;
        private int sdkVersion;

        public AndroidVibrationHandler(VibrationManager manager) : base(manager)
        {
#if UNITY_ANDROID
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                    vibrationService = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }

                using (AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    sdkVersion = versionClass.GetStatic<int>("SDK_INT");
                }
            }
            catch(Exception ex)
            {
                Debug.LogError(ex.Message);
            }
#endif
        }

        public override bool Vibrate(float duration, float intensity)
        {
            if (vibrationService == null) return false;

#if UNITY_ANDROID
            try
            {
                if (sdkVersion >= 26)
                {
                    using (AndroidJavaClass vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    {
                        AndroidJavaObject vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", (long)(duration * 1000), (int)Mathf.Lerp(1, 255, intensity));

                        vibrationService.Call("vibrate", vibrationEffect);
                    }
                }
                else
                {
                    vibrationService.Call("vibrate", duration);
                }

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