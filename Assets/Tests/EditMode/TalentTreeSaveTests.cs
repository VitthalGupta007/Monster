#if UNITY_EDITOR
using NUnit.Framework;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.Tests.EditMode
{
    public class TalentTreeSaveTests
    {
        [Test]
        public void TryUnlock_SpendsPointsAndMarksUnlocked()
        {
            var save = new TalentTreeSave();
            save.AddPoints(5);

            Assert.IsTrue(save.TryUnlock(TalentTreeIds.ExtraReroll, 3));
            Assert.AreEqual(2, save.TalentPoints);
            Assert.IsTrue(save.IsUnlocked(TalentTreeIds.ExtraReroll));
        }

        [Test]
        public void TryUnlock_FailsWhenInsufficientPoints()
        {
            var save = new TalentTreeSave();
            save.AddPoints(2);

            Assert.IsFalse(save.TryUnlock(TalentTreeIds.ExpandedMind, 5));
            Assert.AreEqual(2, save.TalentPoints);
            Assert.IsFalse(save.IsUnlocked(TalentTreeIds.ExpandedMind));
        }

        [Test]
        public void TryUnlock_FailsWhenAlreadyUnlocked()
        {
            var save = new TalentTreeSave();
            save.AddPoints(10);
            Assert.IsTrue(save.TryUnlock(TalentTreeIds.GoldenInstinct, 8));
            Assert.IsFalse(save.TryUnlock(TalentTreeIds.GoldenInstinct, 0));
            Assert.AreEqual(2, save.TalentPoints);
        }

        [Test]
        public void TalentTreeIds_IncludePhase2MetaNodes()
        {
            Assert.AreEqual("talent_hp", TalentTreeIds.IronWill);
            Assert.AreEqual("talent_speed", TalentTreeIds.QuickFeet);
            Assert.AreEqual("talent_codex", TalentTreeIds.ScholarsEye);
        }
    }
}
#endif
