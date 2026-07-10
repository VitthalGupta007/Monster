#if UNITY_EDITOR
using NUnit.Framework;
using VXMonster.Gameplay;

namespace VXMonster.Tests.EditMode
{
    public class DifficultyTierTests
    {
        [Test]
        public void TalentPointsPerBossKill_MatchesTiers()
        {
            Assert.AreEqual(1, DifficultyTier.Easy.TalentPointsPerBossKill());
            Assert.AreEqual(2, DifficultyTier.Normal.TalentPointsPerBossKill());
            Assert.AreEqual(4, DifficultyTier.Hard.TalentPointsPerBossKill());
            Assert.AreEqual(7, DifficultyTier.Nightmare.TalentPointsPerBossKill());
        }

        [Test]
        public void EnemyHpMultiplier_ScalesWithDifficulty()
        {
            Assert.Less(DifficultyTier.Easy.EnemyHpMultiplier(), DifficultyTier.Normal.EnemyHpMultiplier());
            Assert.Less(DifficultyTier.Normal.EnemyHpMultiplier(), DifficultyTier.Hard.EnemyHpMultiplier());
            Assert.Less(DifficultyTier.Hard.EnemyHpMultiplier(), DifficultyTier.Nightmare.EnemyHpMultiplier());
        }
    }
}
#endif
