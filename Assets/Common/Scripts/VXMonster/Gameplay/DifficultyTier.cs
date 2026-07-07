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
    }
}
