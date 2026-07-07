namespace VXMonster.Gameplay
{
    /// <summary>
    /// Lobby-selected difficulty for campaign and endless runs.
    /// Daily challenge always uses Normal in GameSessionManager.
    /// </summary>
    public static class VXDifficultySelection
    {
        public static DifficultyTier Selected { get; private set; } = DifficultyTier.Normal;

        public static void Set(DifficultyTier tier)
        {
            Selected = tier;
        }

        public static void CycleNext()
        {
            var next = (int)Selected + 1;
            if (next > (int)DifficultyTier.Nightmare) next = 0;
            Selected = (DifficultyTier)next;
        }

        public static void CyclePrevious()
        {
            var prev = (int)Selected - 1;
            if (prev < 0) prev = (int)DifficultyTier.Nightmare;
            Selected = (DifficultyTier)prev;
        }
    }
}
