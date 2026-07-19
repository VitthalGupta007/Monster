using System;
using UnityEngine;

namespace VXMonster.Platform.Integrity
{
    public static class PlayIntegrityBootstrap
    {
        static IPlayIntegrityService service;
        static bool requested;

        public static void RequestCheckOnce()
        {
            if (requested) return;
            requested = true;

            service ??= CreateService();
            service.RequestCheck(result =>
            {
                if (!result.Success && !string.IsNullOrEmpty(result.Error))
                    Debug.Log($"[PlayIntegrity] Check finished: {result.Error}");
            });
        }

        static IPlayIntegrityService CreateService()
        {
#if PLAY_INTEGRITY_AVAILABLE && UNITY_ANDROID && !UNITY_EDITOR
            return new PlayIntegrityService();
#else
            return new MockPlayIntegrityService();
#endif
        }
    }
}
