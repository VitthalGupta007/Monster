using System;

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
            onComplete?.Invoke(true);
        }

        public void SignIn(Action<bool> onComplete)
        {
            IsAuthenticated = false;
            onComplete?.Invoke(false);
        }

        public void SubmitScore(string leaderboardId, long score, Action<bool> onComplete = null)
        {
            onComplete?.Invoke(false);
        }

        public void UnlockAchievement(string achievementId, Action<bool> onComplete = null)
        {
            onComplete?.Invoke(false);
        }

        public void ShowLeaderboard(string leaderboardId = null) { }

        public void ShowAchievements() { }
    }
}
