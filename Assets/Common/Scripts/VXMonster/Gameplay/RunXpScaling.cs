using UnityEngine;
using VXMonster.Core;

namespace VXMonster.Gameplay
{
    /// <summary>
    /// Runtime gem XP multiplier: stage base × difficulty × time survived × endless loops × daily XP blessing.
    /// </summary>
    public static class RunXpScaling
    {
        const float TimeBonusPerMinute = 0.02f;
        const float TimeBonusCap = 0.50f;

        const float EndlessBonusPerLoop = 0.12f;
        const float EndlessBonusCap = 1.20f;

        public static float GetRunTimeSeconds()
        {
            if (!StageController.IsLoaded || StageController.Director == null)
                return 0f;

            var director = StageController.Director;
            var segmentTime = (float)director.time;
            var session = GameSessionManager.Instance;

            if (session != null && session.RunMode == RunMode.Endless)
                return session.EndlessLoopCount * (float)director.duration + segmentTime;

            return segmentTime;
        }

        public static float GetGemXpMultiplier(float runTimeSeconds, float stageXpMultiplier, GameSessionManager session)
        {
            var mult = Mathf.Max(0.1f, stageXpMultiplier);

            if (session != null)
            {
                mult *= session.Difficulty.RewardMultiplier();

                if (session.RunMode == RunMode.Endless)
                {
                    var loopBonus = Mathf.Min(session.EndlessLoopCount * EndlessBonusPerLoop, EndlessBonusCap);
                    mult *= 1f + loopBonus;
                }

                foreach (var modifier in session.ActiveModifiers)
                {
                    if (modifier.Id == "bless_xp")
                        mult *= modifier.RewardMultiplier;
                }
            }

            var timeBonus = Mathf.Min(runTimeSeconds / 60f * TimeBonusPerMinute, TimeBonusCap);
            mult *= 1f + timeBonus;

            return Mathf.Max(0.1f, mult);
        }

        public static float GetCurrentGemXpMultiplier()
        {
            var stageMult = StageController.IsLoaded && StageController.Stage != null
                ? StageController.Stage.XpGainMultiplier
                : 1f;

            return GetGemXpMultiplier(GetRunTimeSeconds(), stageMult, GameSessionManager.Instance);
        }

        const float TierBiasFromStageSpan = 0.25f;
        const float TierBiasPerMinute = 0.015f;
        const float TierBiasTimeCap = 0.35f;
        const float TierBiasPerEndlessLoop = 0.08f;
        const float TierBiasEndlessCap = 0.40f;

        public static float GetGemTierBias(float runTimeSeconds, float stageXpMultiplier, GameSessionManager session)
        {
            var bias = Mathf.Clamp01((stageXpMultiplier - 1f) / 0.2f * TierBiasFromStageSpan);

            if (session != null)
            {
                bias += Mathf.Max(0f, session.Difficulty.RewardMultiplier() - 1f) * 0.3f;

                if (session.RunMode == RunMode.Endless)
                    bias += Mathf.Min(session.EndlessLoopCount * TierBiasPerEndlessLoop, TierBiasEndlessCap);

                foreach (var modifier in session.ActiveModifiers)
                {
                    if (modifier.Id == "bless_xp")
                        bias += 0.12f;
                }
            }

            bias += Mathf.Min(runTimeSeconds / 60f * TierBiasPerMinute, TierBiasTimeCap);

            return Mathf.Clamp01(bias);
        }

        public static float GetCurrentGemTierBias()
        {
            var stageMult = StageController.IsLoaded && StageController.Stage != null
                ? StageController.Stage.XpGainMultiplier
                : 1f;

            return GetGemTierBias(GetRunTimeSeconds(), stageMult, GameSessionManager.Instance);
        }

        public static bool IsGemDrop(DropType type) =>
            type >= DropType.SmallGem && type <= DropType.LargeGem;

        public static float GetBiasedGemDropChance(DropType type, float baseChance, float tierBias)
        {
            if (!IsGemDrop(type) || baseChance <= 0f)
                return baseChance;

            var tier = GemTierIndex(type);
            if (tier < 0)
                return baseChance;

            var scale = 1f + tierBias * tier * 0.35f;
            return Mathf.Min(baseChance * scale, 100f);
        }

        public static DropType ResolveGemDrop(DropType rolledType, float tierBias)
        {
            if (!IsGemDrop(rolledType) || tierBias <= 0f)
                return rolledType;

            var tier = GemTierIndex(rolledType);
            var maxTier = GemTierIndex(DropType.LargeGem);

            while (tier < maxTier)
            {
                var upgradeChance = tierBias * (0.22f + tier * 0.05f);
                if (Random.value >= upgradeChance)
                    break;
                tier++;
            }

            return GemFromTier(tier);
        }

        static int GemTierIndex(DropType type) => type switch
        {
            DropType.SmallGem => 0,
            DropType.MediumGem => 1,
            DropType.BigGem => 2,
            DropType.LargeGem => 3,
            _ => -1,
        };

        static DropType GemFromTier(int tier) => tier switch
        {
            0 => DropType.SmallGem,
            1 => DropType.MediumGem,
            2 => DropType.BigGem,
            3 => DropType.LargeGem,
            _ => DropType.SmallGem,
        };
    }
}
