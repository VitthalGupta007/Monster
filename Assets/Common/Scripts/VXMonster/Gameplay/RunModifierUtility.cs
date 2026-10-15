using System;
using System.Collections.Generic;

namespace VXMonster.Gameplay
{
    [Serializable]
    public struct RunModifierDefinition
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public float EnemyHpMultiplier;
        public float EnemyDamageMultiplier;
        public float RewardMultiplier;
        public int PassiveSlotBonus;
        public bool IsCurse;
    }

    public static class RunModifierUtility
    {
        private static readonly RunModifierDefinition BlessingXp = new RunModifierDefinition
        {
            Id = "bless_xp",
            DisplayName = "Swift Growth",
            Description = "+20% run rewards (XP/gold scaling).",
            EnemyHpMultiplier = 1f,
            EnemyDamageMultiplier = 1f,
            RewardMultiplier = 1.2f,
            IsCurse = false
        };

        private static readonly RunModifierDefinition BlessingChests = new RunModifierDefinition
        {
            Id = "bless_chests",
            DisplayName = "Treasure Tide",
            Description = "+15% run rewards. Richer pickups feel.",
            EnemyHpMultiplier = 1f,
            EnemyDamageMultiplier = 1f,
            RewardMultiplier = 1.15f,
            IsCurse = false
        };

        private static readonly RunModifierDefinition CurseFragile = new RunModifierDefinition
        {
            Id = "curse_fragile",
            DisplayName = "Fragile Hero",
            Description = "Enemies deal +20% damage.",
            EnemyHpMultiplier = 1f,
            EnemyDamageMultiplier = 1.2f,
            RewardMultiplier = 1f,
            IsCurse = true
        };

        private static readonly RunModifierDefinition CurseHorde = new RunModifierDefinition
        {
            Id = "curse_horde",
            DisplayName = "Restless Horde",
            Description = "Enemies have +15% HP.",
            EnemyHpMultiplier = 1.15f,
            EnemyDamageMultiplier = 1f,
            RewardMultiplier = 1f,
            IsCurse = true
        };

        private static readonly RunModifierDefinition BlessingSlots = new RunModifierDefinition
        {
            Id = "bless_slots",
            DisplayName = "Expanded Mind",
            Description = "+1 passive ability slot for this run.",
            EnemyHpMultiplier = 1f,
            EnemyDamageMultiplier = 1f,
            RewardMultiplier = 1f,
            PassiveSlotBonus = 1,
            IsCurse = false
        };

        public static int GetUtcDailySeed()
        {
            var dateKey = int.Parse(GameSessionManager.GetUtcDateKey());
            return dateKey * 31337;
        }

        public static int GetUtcWeeklySeed()
        {
            var weekKey = int.Parse(GameSessionManager.GetUtcWeekKey());
            return weekKey * 7919;
        }

        public static IReadOnlyList<RunModifierDefinition> GetDailyModifiers(int seed)
        {
            return PickModifiers(seed);
        }

        public static IReadOnlyList<RunModifierDefinition> GetWeeklyModifiers(int seed)
        {
            return PickModifiers(seed ^ 0x5A5A5A5A);
        }

        private static IReadOnlyList<RunModifierDefinition> PickModifiers(int seed)
        {
            var rng = new Random(seed);
            var pool = new List<RunModifierDefinition>
            {
                BlessingXp,
                BlessingChests,
                BlessingSlots,
                CurseFragile,
                CurseHorde
            };

            var blessings = new List<RunModifierDefinition>();
            var curses = new List<RunModifierDefinition>();

            foreach (var modifier in pool)
            {
                if (modifier.IsCurse) curses.Add(modifier);
                else blessings.Add(modifier);
            }

            Shuffle(blessings, rng);
            Shuffle(curses, rng);

            var result = new List<RunModifierDefinition>(3);
            if (blessings.Count > 0) result.Add(blessings[0]);
            if (blessings.Count > 1) result.Add(blessings[1]);
            if (curses.Count > 0) result.Add(curses[0]);
            return result;
        }

        private static void Shuffle(List<RunModifierDefinition> list, Random rng)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
