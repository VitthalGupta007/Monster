#if GOOGLE_PLAY_GAMES_AVAILABLE
using System;
using UnityEngine;

namespace VXMonster.Platform.PlayGames
{
    /// <summary>
    /// v1.1 — wire Google Play Games plugin here. Until then, use MockPlayGamesService.
    /// </summary>
    public class GooglePlayGamesService : IPlayGamesService
    {
        public bool IsAuthenticated { get; private set; }
        public bool IsReady => false;

        public void Initialize(Action<bool> onComplete)
        {
            Debug.LogWarning("[GPGS] GooglePlayGamesService stub — install Play Games plugin for v1.1");
            onComplete?.Invoke(false);
        }

        public void SignIn(Action<bool> onComplete) => onComplete?.Invoke(false);

        public void SubmitScore(string leaderboardId, long score, Action<bool> onComplete = null)
        {
            Debug.Log($"[GPGS] SubmitScore {leaderboardId} = {score}");
            onComplete?.Invoke(true);
        }

        public void UnlockAchievement(string achievementId, Action<bool> onComplete = null)
        {
            Debug.Log($"[GPGS] Unlock {achievementId}");
            onComplete?.Invoke(true);
        }

        public void ShowLeaderboard(string leaderboardId = null) { }
        public void ShowAchievements() { }
    }
}
#endif
