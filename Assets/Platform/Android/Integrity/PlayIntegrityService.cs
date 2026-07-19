#if PLAY_INTEGRITY_AVAILABLE && UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Security.Cryptography;
using Google.Play.Common;
using Google.Play.Integrity;
using UnityEngine;
using VXMonster.Platform.Analytics;

namespace VXMonster.Platform.Integrity
{
    public class PlayIntegrityService : IPlayIntegrityService
    {
        public void RequestCheck(Action<IntegrityCheckResult> onComplete)
        {
            var nonce = GenerateNonce();
            var manager = new IntegrityManager();
            var request = new IntegrityTokenRequest(nonce);
            var operation = manager.RequestIntegrityToken(request);

            operation.Completed += op =>
            {
                if (op.Error != IntegrityErrorCode.NoError)
                {
                    var error = op.Error.ToString();
                    Debug.LogWarning($"[PlayIntegrity] Token request failed: {error}");
                    AnalyticsEvents.LogIntegrityCheck(false, error);
                    onComplete?.Invoke(IntegrityCheckResult.Failed(error));
                    return;
                }

                var token = op.GetResult().Token;
                Debug.Log("[PlayIntegrity] Token received.");
                AnalyticsEvents.LogIntegrityCheck(true, "token_received");
                onComplete?.Invoke(IntegrityCheckResult.Succeeded(token));
            };
        }

        private static string GenerateNonce()
        {
            var bytes = new byte[32];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}
#endif
