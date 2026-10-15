using System;
using System.Globalization;
using VXMonster.Core;
using VXMonster.Save;

namespace VXMonster.Gameplay
{
    public static class LoginClaimUtility
    {
        public const int BaseGold = 50;
        public const int StreakBonusGold = 10;
        public const int MaxStreakBonusDays = 7;

        public static int PreviewGold(LoginClaimSave save, string utcDateKey)
        {
            if (save == null || save.HasClaimedToday(utcDateKey)) return 0;
            var nextStreak = NextStreak(save, utcDateKey);
            var bonusDays = Clamp(nextStreak - 1, 0, MaxStreakBonusDays);
            return BaseGold + bonusDays * StreakBonusGold;
        }

        public static bool TryClaim(out int goldGranted, out int streak)
        {
            goldGranted = 0;
            streak = 0;
            if (GameController.SaveManager == null || GameController.CurrenciesManager == null) return false;

            var save = GameController.SaveManager.GetSave<LoginClaimSave>(LoginClaimSave.SaveKey);
            var dateKey = GameSessionManager.GetUtcDateKey();
            if (save.HasClaimedToday(dateKey)) return false;

            streak = NextStreak(save, dateKey);
            var bonusDays = Clamp(streak - 1, 0, MaxStreakBonusDays);
            goldGranted = BaseGold + bonusDays * StreakBonusGold;

            var gold = GameController.CurrenciesManager.GetDefaultCurrency(false);
            if (gold == null) return false;

            gold.Deposit(goldGranted);
            save.LastClaimUtcDate = dateKey;
            save.LoginStreak = streak;
            GameController.SaveManager.Save(false);
            return true;
        }

        private static int NextStreak(LoginClaimSave save, string utcDateKey)
        {
            if (string.IsNullOrEmpty(save.LastClaimUtcDate)) return 1;
            if (!DateTime.TryParseExact(save.LastClaimUtcDate, "yyyyMMdd", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var last)
                || !DateTime.TryParseExact(utcDateKey, "yyyyMMdd", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var today))
            {
                return 1;
            }

            var delta = (today.Date - last.Date).Days;
            if (delta == 1) return Math.Max(1, save.LoginStreak + 1);
            return 1;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
    }
}
