using VXMonster.Core.Upgrades;
using VXMonster.Gameplay;

namespace VXMonster.UI
{
    /// <summary>
    /// Fallback player-facing copy when ScriptableObject fields are empty.
    /// Magnitudes match code defaults in TalentTreeIds / RelicsManager / RunModifierUtility.
    /// </summary>
    public static class PlayerFacingCopy
    {
        public static string Character(string characterId, string characterName)
        {
            switch ((characterId ?? string.Empty).ToUpperInvariant())
            {
                case "WIZARD":
                    return "Balanced starter. Learn the loop before spending gold.";
                case "MAGE":
                    return "Higher base power. Good all-round caster.";
                case "ROGUE":
                    return "Starts with Flying Daggers. Aggressive close-mid skirmisher.";
                case "PALADIN":
                    return "Starts with Steel Sword. Durable front-line fighter.";
                case "NECROMANCER":
                    return "Starts with Magic Rune. Zone control and traps.";
                case "RANGER":
                    return "Starts with Boomerang. Safe distance and pickups.";
                case "BERSERKER":
                    return "Starts with Rolling Stone. High risk, high pressure.";
                case "SAGE":
                    return "Starts with Guardian Eye. Defensive aura play.";
                default:
                    return string.IsNullOrEmpty(characterName)
                        ? "Unlock to expand your roster."
                        : $"{characterName}: unique starting kit and stats.";
            }
        }

        public static string Upgrade(UpgradeType type, float nextValue)
        {
            switch (type)
            {
                case UpgradeType.Armor:
                    return nextValue > 0f
                        ? $"Permanently reduces damage taken. Next: {nextValue:0.##}× incoming."
                        : "Permanently reduces damage taken.";
                case UpgradeType.Health:
                    return nextValue > 0f
                        ? $"Raises max HP for every run. Next bonus value: {nextValue:0.##}."
                        : "Raises max HP for every run.";
                case UpgradeType.Healing:
                    return nextValue > 0f
                        ? $"Improves heals and food. Next bonus: +{nextValue:0.##}."
                        : "Improves heals and food.";
                case UpgradeType.Damage:
                    return nextValue > 0f
                        ? $"Boosts ability damage. Next multiplier: {nextValue:0.##}×."
                        : "Boosts ability damage.";
                case UpgradeType.Revive:
                    return "Unlock one free revive per failed run (before ads).";
                default:
                    return "Permanent lobby upgrade.";
            }
        }

        public static string Talent(string nodeId)
        {
            switch (nodeId)
            {
                case TalentTreeIds.ExtraReroll:
                    return "+1 ability reroll at the start of every run.";
                case TalentTreeIds.ExpandedMind:
                    return "+1 passive ability slot every run.";
                case TalentTreeIds.GoldenInstinct:
                    return "1.1× gold from runs.";
                case TalentTreeIds.IronWill:
                    return "+20 max HP every run.";
                case TalentTreeIds.QuickFeet:
                    return "1.08× move speed every run.";
                case TalentTreeIds.ScholarsEye:
                    return "+15 gold whenever you discover a new relic.";
                default:
                    return "Permanent talent unlock.";
            }
        }

        public static string FormatRelic(RelicData relic)
        {
            if (relic == null) return string.Empty;
            var desc = string.IsNullOrWhiteSpace(relic.Description) ? relic.DisplayName : relic.Description;
            switch (relic.EffectType)
            {
                case RelicEffectType.DamageBoost:
                    return $"{desc} (+{Pct(relic.Magnitude, 0.4f)} damage).";
                case RelicEffectType.PickupRadius:
                    return $"{desc} (+{Pct(relic.Magnitude, 2f)} pickup radius).";
                case RelicEffectType.ComboAmplifier:
                    return $"{desc} (+{Pct(relic.Magnitude, 0.25f)} combo power).";
                case RelicEffectType.BossDamage:
                    return $"{desc} (+{Pct(relic.Magnitude, 0.5f)} vs bosses).";
                case RelicEffectType.CooldownReduction:
                    return $"{desc} (-{Pct(relic.Magnitude, 0.1f)} cooldowns).";
                case RelicEffectType.GoldBoost:
                    return $"{desc} (+{Pct(relic.Magnitude, 0.25f)} gold).";
                case RelicEffectType.DamageReduction:
                    return $"{desc} (-{Pct(relic.Magnitude, 0.15f)} damage taken).";
                case RelicEffectType.MoveSpeed:
                    return $"{desc} (+{Pct(relic.Magnitude, 0.12f)} move speed).";
                case RelicEffectType.Lifesteal:
                    return $"{desc} (heal {Pct(relic.Magnitude, 0.03f)} of damage dealt).";
                case RelicEffectType.SecondWind:
                    return $"{desc} (once: heal {Pct(relic.Magnitude, 0.4f)} HP when low).";
                case RelicEffectType.BonusBanish:
                    return $"{desc} (+1 banish this run).";
                default:
                    return desc;
            }
        }

        private static string Pct(float magnitude, float fallback)
        {
            var value = magnitude > 0f ? magnitude : fallback;
            return $"{value * 100f:0.#}%";
        }
    }
}
