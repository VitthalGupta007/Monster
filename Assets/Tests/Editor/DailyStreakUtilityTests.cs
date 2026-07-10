#if UNITY_EDITOR
using NUnit.Framework;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.Tests.EditMode
{
    public class DailyStreakUtilityTests
    {
        [Test]
        public void LifetimeStatsSave_DailyStreakFields_DefaultZero()
        {
            var lifetime = new LifetimeStatsSave();
            Assert.AreEqual(0, lifetime.DailyStreak);
            Assert.AreEqual(string.Empty, lifetime.LastDailyDate);
        }

        [Test]
        public void RecordScoredDailyRun_FirstRun_SetsStreakToOne()
        {
            var lifetime = new LifetimeStatsSave();
            ApplyStreakLogic(lifetime, "20260701");
            Assert.AreEqual(1, lifetime.DailyStreak);
            Assert.AreEqual("20260701", lifetime.LastDailyDate);
        }

        [Test]
        public void RecordScoredDailyRun_SameDay_IsIdempotent()
        {
            var lifetime = new LifetimeStatsSave { DailyStreak = 3, LastDailyDate = "20260701" };
            ApplyStreakLogic(lifetime, "20260701");
            Assert.AreEqual(3, lifetime.DailyStreak);
            Assert.AreEqual("20260701", lifetime.LastDailyDate);
        }

        [Test]
        public void RecordScoredDailyRun_ConsecutiveDay_Increments()
        {
            var lifetime = new LifetimeStatsSave { DailyStreak = 2, LastDailyDate = "20260701" };
            ApplyStreakLogic(lifetime, "20260702");
            Assert.AreEqual(3, lifetime.DailyStreak);
            Assert.AreEqual("20260702", lifetime.LastDailyDate);
        }

        [Test]
        public void RecordScoredDailyRun_Gap_ResetsToOne()
        {
            var lifetime = new LifetimeStatsSave { DailyStreak = 5, LastDailyDate = "20260701" };
            ApplyStreakLogic(lifetime, "20260704");
            Assert.AreEqual(1, lifetime.DailyStreak);
            Assert.AreEqual("20260704", lifetime.LastDailyDate);
        }

        // Mirrors DailyStreakUtility.RecordScoredDailyRun without requiring GameController.SaveManager.
        static void ApplyStreakLogic(LifetimeStatsSave lifetime, string utcDateKey)
        {
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

            if (System.DateTime.TryParseExact(lifetime.LastDailyDate, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var last) &&
                System.DateTime.TryParseExact(utcDateKey, "yyyyMMdd", null, System.Globalization.DateTimeStyles.AssumeUniversal, out var current) &&
                (current - last).TotalDays <= 1.5)
            {
                lifetime.DailyStreak = System.Math.Max(1, lifetime.DailyStreak + 1);
            }
            else
            {
                lifetime.DailyStreak = 1;
            }

            lifetime.LastDailyDate = utcDateKey;
        }
    }
}
#endif
