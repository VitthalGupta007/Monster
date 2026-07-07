using VXMonster.Platform.Analytics;

namespace VXMonster.Gameplay
{
    public static class TalentPointUtility
    {
        public static void AwardBossKillPoints()
        {
            var session = GameSessionManager.Instance;
            if (session?.TalentTree == null) return;

            var points = session.Difficulty.TalentPointsPerBossKill();
            session.TalentTree.AddPoints(points);
            AnalyticsEvents.LogBossKilled(points);
        }
    }
}
