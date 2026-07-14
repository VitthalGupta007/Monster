using System.Collections.Generic;
using VXMonster.Core;
using VXMonster.Core.Abilities;

namespace VXMonster.Gameplay
{
    /// <summary>
    /// Stage-climbing ability pool rules (Phase 7.7). Relics are intentionally unchanged.
    /// Stage indices are 0-based to match <see cref="StageSave.SelectedStageId"/>.
    /// </summary>
    public static class StageAbilityProgression
    {
        public readonly struct ChestTierBonus
        {
            public ChestTierBonus(float tier3, float tier5)
            {
                Tier3 = tier3;
                Tier5 = tier5;
            }

            public float Tier3 { get; }
            public float Tier5 { get; }
        }

        static readonly Dictionary<AbilityType, int> MinStageByAbility = new()
        {
            // Early actives — Stages 1–2
            { AbilityType.ShootingStar, 0 },
            { AbilityType.GuardianEye, 0 },
            { AbilityType.LightningAmulet, 0 },
            { AbilityType.RollingStone, 0 },
            { AbilityType.Boomerang, 0 },

            // Mid actives — Stage 3+
            { AbilityType.IceShard, 2 },
            { AbilityType.Fireball, 2 },

            // Late actives — Stage 4+
            { AbilityType.MagicRune, 3 },
            { AbilityType.FlyingDagger, 3 },

            // Endgame active — Stage 5+
            { AbilityType.SolarMagnifier, 4 },

            // Core passives — Stage 1
            { AbilityType.Magnet, 0 },
            { AbilityType.MoveSpeed, 0 },
            { AbilityType.Damage, 0 },
            { AbilityType.MaxHP, 0 },
            { AbilityType.XP, 0 },
            { AbilityType.RestoreHP, 0 },

            // Early-mid passives — Stage 2+
            { AbilityType.Cooldown, 1 },
            { AbilityType.Size, 1 },

            // Mid passives — Stage 3+
            { AbilityType.ProjectileSpeed, 2 },
            { AbilityType.DamageReduction, 2 },

            // Late passives — Stage 4+
            { AbilityType.Duration, 3 },
            { AbilityType.IncreasedGold, 3 },

            // Evolutions — Stage 3+ (also gated by requirements)
            { AbilityType.VoidStar, 2 },
            { AbilityType.TimeGazer, 2 },
            { AbilityType.ThunderRing, 2 },
            { AbilityType.MaceBall, 2 },
            { AbilityType.SilverStakes, 2 },
            { AbilityType.SpikyTrap, 2 },
            { AbilityType.Meteor, 2 },
            { AbilityType.Recoiler, 2 },
            { AbilityType.TwinDagger, 2 },
            { AbilityType.LunarProjector, 2 },
            { AbilityType.AncientScepter, 2 },
            { AbilityType.SacredBlade, 2 },

            // Weapons / endgame — always eligible when their own rules allow
            { AbilityType.WoodenWand, 0 },
            { AbilityType.SteelSword, 0 },
            { AbilityType.HealEndgame, 0 },
            { AbilityType.GoldEndgame, 0 },
        };

        /// <summary>One-based stage → additive chance bonuses for boss ability chest tiers.</summary>
        public static readonly (int stage, float tier3, float tier5)[] StageChestBonusTargets =
        {
            (1, 0f, 0f),
            (2, 0f, 0f),
            (3, 0.05f, 0.02f),
            (4, 0.10f, 0.05f),
            (5, 0.15f, 0.10f),
            (6, 0.20f, 0.15f),
        };

        public static int GetMinStageId(AbilityType type)
        {
            return MinStageByAbility.TryGetValue(type, out var min) ? min : 0;
        }

        public static bool IsEligibleForStage(AbilityType type, int stageIdZeroBased)
        {
            return GetMinStageId(type) <= stageIdZeroBased;
        }

        public static float GetStageWeightMultiplier(AbilityType type, int stageIdZeroBased)
        {
            if (stageIdZeroBased <= 0) return 1f;

            switch (type)
            {
                case AbilityType.Damage:
                case AbilityType.MaxHP:
                case AbilityType.Cooldown:
                case AbilityType.ProjectileSpeed:
                    return 1f + 0.08f * stageIdZeroBased;

                case AbilityType.VoidStar:
                case AbilityType.TimeGazer:
                case AbilityType.ThunderRing:
                case AbilityType.MaceBall:
                case AbilityType.SilverStakes:
                case AbilityType.SpikyTrap:
                case AbilityType.Meteor:
                case AbilityType.Recoiler:
                case AbilityType.TwinDagger:
                case AbilityType.LunarProjector:
                case AbilityType.AncientScepter:
                case AbilityType.SacredBlade:
                    return stageIdZeroBased >= 2
                        ? 1f + 0.12f * (stageIdZeroBased - 1)
                        : 1f;

                default:
                    return 1f;
            }
        }

        public static ChestTierBonus GetChestTierBonus(int stageIdZeroBased)
        {
            var stageOneBased = stageIdZeroBased + 1;
            foreach (var (stage, tier3, tier5) in StageChestBonusTargets)
            {
                if (stage == stageOneBased)
                    return new ChestTierBonus(tier3, tier5);
            }

            return new ChestTierBonus(0f, 0f);
        }

        public static int CountEligibleAbilities(AbilitiesDatabase database, int stageIdZeroBased)
        {
            if (database == null) return 0;

            var count = 0;
            for (var i = 0; i < database.AbilitiesCount; i++)
            {
                var ability = database.GetAbility(i);
                if (ability == null || ability.IsEndgameAbility || ability.IsWeaponAbility) continue;
                if (!IsEligibleForStage(ability.AbilityType, stageIdZeroBased)) continue;
                count++;
            }

            return count;
        }
    }
}
