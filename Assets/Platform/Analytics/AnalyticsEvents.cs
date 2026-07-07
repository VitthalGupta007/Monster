using VXMonster.Gameplay;

namespace VXMonster.Platform.Analytics
{
    public static class AnalyticsEvents
    {
        private static IAnalyticsService service = new MockAnalyticsService();

        public static void Bind(IAnalyticsService analyticsService)
        {
            service = analyticsService ?? new MockAnalyticsService();
        }

        public static void Initialize()
        {
            service?.Initialize();
        }

        public static void LogRunStart()
        {
            var session = GameSessionManager.Instance;
            service?.LogEvent("run_start",
                ("mode", session?.RunMode.ToString() ?? "unknown"),
                ("difficulty", session?.Difficulty.ToString() ?? "unknown"));
        }

        public static void LogRunEnd(bool victory)
        {
            var session = GameSessionManager.Instance;
            service?.LogEvent("run_end",
                ("victory", victory ? 1 : 0),
                ("mode", session?.RunMode.ToString() ?? "unknown"));
        }

        public static void LogIapPurchase(string productId, bool success)
        {
            service?.LogEvent("iap_purchase",
                ("product_id", productId),
                ("success", success ? 1 : 0));
        }

        public static void LogAdImpression(string format)
        {
            service?.LogEvent("ad_impression", ("format", format));
        }

        public static void LogBossKilled(int talentPoints)
        {
            service?.LogEvent("boss_killed", ("talent_points", talentPoints));
        }

        public static void LogTalentUnlock(string nodeId)
        {
            service?.LogEvent("talent_unlock", ("node_id", nodeId));
        }
    }
}
