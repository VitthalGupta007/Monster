using System;
using VXMonster.Core;
using VXMonster.Save;

namespace VXMonster.Gameplay
{
    public static class DailyStreakUtility
    {
        public static void RecordScoredDailyRun(string utcDateKey)
        {
            if (GameController.SaveManager == null) return;

            var lifetime = GameController.SaveManager.GetSave<LifetimeStatsSave>("VX Lifetime Stats");
            if (lifetime == null) return;

            if (string.IsNullOrEmpty(lifetime.LastDailyDate))
            {
                lifetime.DailyStreak = 1;
                lifetime.LastDailyDate = utcDateKey;
                return;
            }

            if (lifetime.LastDailyDate == utcDateKey)
            {
                return;
            }

            if (TryParseDateKey(lifetime.LastDailyDate, out var last) &&
                TryParseDateKey(utcDateKey, out var current) &&
                (current - last).TotalDays <= 1.5)
            {
                lifetime.DailyStreak = Math.Max(1, lifetime.DailyStreak + 1);
            }
            else
            {
                lifetime.DailyStreak = 1;
            }

            lifetime.LastDailyDate = utcDateKey;
        }

        private static bool TryParseDateKey(string key, out DateTime date)
        {
            return DateTime.TryParseExact(key, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out date);
        }
    }
}
