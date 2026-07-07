using System;

namespace VXMonster.Platform.PlayGames
{
    public interface IPlayGamesService
    {
        bool IsAuthenticated { get; }
        bool IsReady { get; }

        void Initialize(Action<bool> onComplete);
        void SignIn(Action<bool> onComplete);
        void SubmitScore(string leaderboardId, long score, Action<bool> onComplete = null);
        void UnlockAchievement(string achievementId, Action<bool> onComplete = null);
        void ShowLeaderboard(string leaderboardId = null);
        void ShowAchievements();
    }
}
