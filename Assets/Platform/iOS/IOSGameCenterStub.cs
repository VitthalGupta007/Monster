using System;

namespace VXMonster.Platform.iOS
{
    /// <summary>
    /// Placeholder for Game Center (Phase 8 / v1.1).
    /// </summary>
    public class IOSGameCenterStub : VXMonster.Platform.PlayGames.IPlayGamesService
    {
        public bool IsAuthenticated => false;
        public bool IsReady => true;

        public void Initialize(Action<bool> onComplete) => onComplete?.Invoke(false);
        public void SignIn(Action<bool> onComplete) => onComplete?.Invoke(false);
        public void SubmitScore(string leaderboardId, long score, Action<bool> onComplete = null) => onComplete?.Invoke(true);
        public void UnlockAchievement(string achievementId, Action<bool> onComplete = null) => onComplete?.Invoke(true);
        public void ShowLeaderboard(string leaderboardId = null) { }
        public void ShowAchievements() { }
        public void SyncCloudSave(Action<bool> onComplete = null) => onComplete?.Invoke(true);
        public void PushCloudSave(Action<bool> onComplete = null) => onComplete?.Invoke(true);
    }
}
