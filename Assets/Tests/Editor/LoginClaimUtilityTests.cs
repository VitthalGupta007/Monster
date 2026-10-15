using NUnit.Framework;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.Tests.Editor
{
    public class LoginClaimUtilityTests
    {
        [Test]
        public void PreviewGold_FirstClaim_IsBaseGold()
        {
            var save = new LoginClaimSave();
            Assert.AreEqual(LoginClaimUtility.BaseGold, LoginClaimUtility.PreviewGold(save, "20261015"));
        }

        [Test]
        public void PreviewGold_AlreadyClaimed_IsZero()
        {
            var save = new LoginClaimSave
            {
                LastClaimUtcDate = "20261015",
                LoginStreak = 2
            };
            Assert.AreEqual(0, LoginClaimUtility.PreviewGold(save, "20261015"));
        }

        [Test]
        public void GetUtcWeekKey_IsNumeric()
        {
            var key = GameSessionManager.GetUtcWeekKey();
            Assert.IsTrue(int.TryParse(key, out _), $"Week key should be numeric, got '{key}'");
        }
    }
}
