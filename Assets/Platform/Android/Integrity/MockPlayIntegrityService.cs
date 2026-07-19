using System;
using UnityEngine;

namespace VXMonster.Platform.Integrity
{
    public class MockPlayIntegrityService : IPlayIntegrityService
    {
        public void RequestCheck(Action<IntegrityCheckResult> onComplete)
        {
            Debug.Log("[PlayIntegrity] Skipped (editor or package unavailable).");
            onComplete?.Invoke(IntegrityCheckResult.Failed("unavailable"));
        }
    }
}
