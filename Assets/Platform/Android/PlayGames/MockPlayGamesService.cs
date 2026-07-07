using System;
using UnityEngine;

namespace VXMonster.Platform.PlayGames
{
    /// <summary>
    /// Offline stub until Play Console payment and live GPGS resource IDs are configured.
    /// </summary>
    public class MockPlayGamesService : IPlayGamesService
    {
        public bool IsAuthenticated { get; private set; }
        public bool IsReady { get; private set; }

        public void Initialize(Action<bool> onComplete)
        {
            IsReady = true;
            Debug.Log("[VX GPGS] MockPlayGamesService ready (offline mode).");
            onComplete?.Invoke(true);
        }

        public void SignIn(Action<bool> onComplete)
        {
            IsAuthenticated = false;
            Debug.Log("[VX GPGS] Mock sign-in skipped — GPGS not linked yet.");
            onComplete?.Invoke(false);
        }

        public void SubmitScore(string leaderboardId, long score, Action<bool> onComplete = null)
        {
            Debug.Log($"[VX GPGS] Mock score submit: {leaderboardId} = {score}");
            onComplete?.Invoke(false);
        }

        public void UnlockAchievement(string achievementId, Action<bool> onComplete = null)
        {
            Debug.Log($"[VX GPGS] Mock achievement unlock: {achievementId}");
            onComplete?.Invoke(false);
        }

        public void ShowLeaderboard(string leaderboardId = null)
        {
            Debug.Log("[VX GPGS] Mock leaderboard UI (not available offline).");
        }

        public void ShowAchievements()
        {
            Debug.Log("[VX GPGS] Mock achievements UI (not available offline).");
        }
    }
}
