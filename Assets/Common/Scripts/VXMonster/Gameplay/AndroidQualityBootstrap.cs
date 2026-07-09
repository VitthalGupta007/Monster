using UnityEngine;

namespace VXMonster.Gameplay
{
    /// <summary>
    /// Applies lightweight quality presets on low-RAM Android devices.
    /// </summary>
    public static class AndroidQualityBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var ramMb = SystemInfo.systemMemorySize;
            if (ramMb > 0 && ramMb <= 4096)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 45;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.particleRaycastBudget = 64;
            }
            else if (ramMb > 4096 && ramMb <= 6144)
            {
                Application.targetFrameRate = 60;
                QualitySettings.shadows = ShadowQuality.HardOnly;
            }
#endif
        }
    }
}
