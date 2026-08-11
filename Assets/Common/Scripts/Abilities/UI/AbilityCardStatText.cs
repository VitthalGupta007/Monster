using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace VXMonster.Core.Abilities.UI
{
    public static class AbilityCardStatText
    {
        public static string Build(AbilityData abilityData, int saveLevel)
        {
            if (abilityData == null) return string.Empty;

            var flavor = abilityData.Description ?? string.Empty;
            if (abilityData.LevelsCount <= 0) return flavor;

            var index = abilityData.IsEndgameAbility || saveLevel < 0 ? 0 : saveLevel + 1;
            if (index >= abilityData.LevelsCount) index = abilityData.LevelsCount - 1;

            var level = abilityData.GetLevel(index);
            var stats = FormatLevel(level);
            if (string.IsNullOrEmpty(stats)) return flavor;
            return string.IsNullOrEmpty(flavor) ? stats : flavor + "\n" + stats;
        }

        private static string FormatLevel(AbilityLevel level)
        {
            if (level == null) return string.Empty;

            if (level is RestoreHPAbilityLevel restore)
            {
                return $"{restore.RestoredHPPercent}% max HP every {FormatNumber(restore.Cooldown)}s";
            }

            if (level is HealEndgameAbilityLevel heal)
            {
                return $"{heal.HealPersentage}% max HP";
            }

            if (level is GoldEndgameAbilityLevel gold)
            {
                return $"Gold {gold.GoldAmount}";
            }

            var parts = new List<string>();
            Append(level, "Damage", "Damage {0}", parts, multiplier: true);
            Append(level, "DamagePerSecond", "Damage/s {0}", parts, multiplier: true);
            Append(level, "AdditionalDamage", "Extra damage {0}", parts, multiplier: true);
            Append(level, "AdditionalDamagePerSecond", "Extra damage/s {0}", parts, multiplier: true);
            Append(level, "WaveDamage", "Wave damage {0}", parts, multiplier: true);
            Append(level, "SpikeDamage", "Spike damage {0}", parts, multiplier: true);
            Append(level, "BallDamage", "Ball damage {0}", parts, multiplier: true);
            Append(level, "DamageMultiplier", "Damage {0}", parts, multiplier: true);
            Append(level, "AbilityCooldown", "Cooldown {0}s", parts);
            Append(level, "DamageCooldown", "every {0}s", parts);
            Append(level, "Cooldown", "every {0}s", parts);
            Append(level, "CooldownMultiplier", "Cooldown {0}", parts, multiplier: true);
            Append(level, "ProjectileLifetime", "Duration {0}s", parts);
            Append(level, "MineLifetime", "Duration {0}s", parts);
            Append(level, "FireballLifetime", "Duration {0}s", parts);
            Append(level, "DurationMultiplier", "Duration {0}", parts, multiplier: true);
            Append(level, "DurationBetweenHits", "Hit delay {0}s", parts);
            Append(level, "TimeBetweenProjectiles", "Shot delay {0}s", parts);
            Append(level, "TimeBetweenFireballs", "Shot delay {0}s", parts);
            Append(level, "TimeBetweenSlashes", "Slash delay {0}s", parts);
            Append(level, "RestoredHPPercent", "{0}% max HP", parts);
            Append(level, "HealPersentage", "{0}% max HP", parts);
            Append(level, "MaxHPMultiplier", "Max HP {0}", parts, multiplier: true);
            Append(level, "SpeedMultiplier", "Move speed {0}", parts, multiplier: true);
            Append(level, "XPMultiplier", "XP {0}", parts, multiplier: true);
            Append(level, "SizeMultiplier", "Size {0}", parts, multiplier: true);
            Append(level, "ProjectileSpeedMultiplier", "Projectile speed {0}", parts, multiplier: true);
            Append(level, "GoldMultiplier", "Gold {0}", parts, multiplier: true);
            Append(level, "RadiusMultiplier", "Pickup radius {0}", parts, multiplier: true);
            Append(level, "DamageReductionPercent", "Damage reduction {0}%", parts);
            Append(level, "GoldAmount", "Gold {0}", parts);

            if (parts.Count == 0) return string.Empty;
            if (parts.Count > 2) parts.RemoveRange(2, parts.Count - 2);
            return string.Join(" / ", parts);
        }

        private static void Append(AbilityLevel level, string propertyName, string format, List<string> parts, bool multiplier = false)
        {
            var property = level.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property == null) return;

            var value = property.GetValue(level);
            if (value == null) return;

            string formatted;
            switch (value)
            {
                case float f:
                    formatted = multiplier ? FormatMultiplier(f) : FormatNumber(f);
                    break;
                case int i:
                    formatted = multiplier ? FormatMultiplier(i) : i.ToString(CultureInfo.InvariantCulture);
                    break;
                default:
                    return;
            }

            parts.Add(string.Format(CultureInfo.InvariantCulture, format, formatted));
        }

        private static string FormatMultiplier(float value)
        {
            if (value > 1f && value < 2.5f && Mathf.Abs(value % 1f) > 0.001f)
            {
                return "+" + FormatNumber((value - 1f) * 100f) + "%";
            }

            return "x" + FormatNumber(value);
        }

        private static string FormatNumber(float value)
        {
            if (Mathf.Abs(value % 1f) < 0.001f)
            {
                return Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);
            }

            return value.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');
        }
    }
}
