namespace VXMonster.Gameplay
{
    public enum DifficultyTier
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Nightmare = 3
    }

    public static class DifficultyTierExtensions
    {
        public static float EnemyHpMultiplier(this DifficultyTier tier)
        {
            return tier switch
            {
                DifficultyTier.Easy => 0.75f,
                DifficultyTier.Normal => 1f,
                DifficultyTier.Hard => 1.35f,
                DifficultyTier.Nightmare => 1.75f,
                _ => 1f
            };
        }

        public static float EnemyDamageMultiplier(this DifficultyTier tier)
        {
            return tier switch
            {
                DifficultyTier.Easy => 0.75f,
                DifficultyTier.Normal => 1f,
                DifficultyTier.Hard => 1.25f,
                DifficultyTier.Nightmare => 1.5f,
                _ => 1f
            };
        }

        public static float RewardMultiplier(this DifficultyTier tier)
        {
            return tier switch
            {
                DifficultyTier.Easy => 0.75f,
                DifficultyTier.Normal => 1f,
                DifficultyTier.Hard => 1.25f,
                DifficultyTier.Nightmare => 1.5f,
                _ => 1f
            };
        }

        public static int TalentPointsPerBossKill(this DifficultyTier tier)
        {
            return tier switch
            {
                DifficultyTier.Easy => 1,
                DifficultyTier.Normal => 2,
                DifficultyTier.Hard => 4,
                DifficultyTier.Nightmare => 7,
                _ => 2
            };
        }

        public static string DisplayLabel(this DifficultyTier tier)
        {
            return tier switch
            {
                DifficultyTier.Easy => "Easy",
                DifficultyTier.Normal => "Normal",
                DifficultyTier.Hard => "Hard",
                DifficultyTier.Nightmare => "Nightmare",
                _ => tier.ToString()
            };
        }
    }
}
