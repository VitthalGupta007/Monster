using System.Collections.Generic;
using VXMonster.Core.Save;
using VXMonster.Gameplay;
using UnityEngine;

namespace VXMonster.Save
{
    public class RunSessionSave : ISave
    {
        [SerializeField] protected List<string> activeRelicIds = new List<string>();
        [SerializeField] protected int comboBurstCount;
        [SerializeField] protected bool banishUsed;
        [SerializeField] protected int banishesRemaining = 1;
        [SerializeField] protected int rerollsRemaining = 1;
        [SerializeField] protected bool adReviveUsed;
        [SerializeField] protected bool upgradeReviveUsed;
        [SerializeField] protected bool phoenixReviveUsed;
        [SerializeField] protected bool secondWindUsed;

        [SerializeField] protected bool hasSessionContext;
        [SerializeField] protected int runMode;
        [SerializeField] protected int difficulty;
        [SerializeField] protected int endlessLoopCount;
        [SerializeField] protected int dailySeed;
        [SerializeField] protected bool isDailyScoredRun;

        public IReadOnlyList<string> ActiveRelicIds => activeRelicIds;

        public int ComboBurstCount
        {
            get => comboBurstCount;
            set => comboBurstCount = value;
        }

        public bool BanishUsed
        {
            get => banishesRemaining <= 0 || banishUsed;
            set
            {
                banishUsed = value;
                if (value) banishesRemaining = 0;
            }
        }

        public int BanishesRemaining
        {
            get => banishesRemaining;
            set
            {
                banishesRemaining = value;
                if (banishesRemaining > 0) banishUsed = false;
            }
        }

        public bool SecondWindUsed
        {
            get => secondWindUsed;
            set => secondWindUsed = value;
        }

        public int RerollsRemaining
        {
            get => rerollsRemaining;
            set => rerollsRemaining = value;
        }

        public bool AdReviveUsed
        {
            get => adReviveUsed;
            set => adReviveUsed = value;
        }

        public bool UpgradeReviveUsed
        {
            get => upgradeReviveUsed;
            set => upgradeReviveUsed = value;
        }

        public bool PhoenixReviveUsed
        {
            get => phoenixReviveUsed;
            set => phoenixReviveUsed = value;
        }

        public bool HasSessionContext => hasSessionContext;
        public RunMode SavedRunMode => (RunMode)runMode;
        public DifficultyTier SavedDifficulty => (DifficultyTier)difficulty;
        public int SavedEndlessLoopCount => endlessLoopCount;
        public int SavedDailySeed => dailySeed;
        public bool SavedIsDailyScoredRun => isDailyScoredRun;

        public void CaptureSessionContext(RunMode mode, DifficultyTier tier, int endlessLoops, int seed, bool dailyScored)
        {
            hasSessionContext = true;
            runMode = (int)mode;
            difficulty = (int)tier;
            endlessLoopCount = endlessLoops;
            dailySeed = seed;
            isDailyScoredRun = dailyScored;
        }

        public void ClearSessionContext()
        {
            hasSessionContext = false;
            runMode = 0;
            difficulty = (int)DifficultyTier.Normal;
            endlessLoopCount = 0;
            dailySeed = 0;
            isDailyScoredRun = false;
        }

        public void ResetForNewRun(int startingRerolls = 1, int startingBanishes = 1)
        {
            activeRelicIds.Clear();
            comboBurstCount = 0;
            banishUsed = false;
            banishesRemaining = Mathf.Max(1, startingBanishes);
            rerollsRemaining = startingRerolls;
            adReviveUsed = false;
            upgradeReviveUsed = false;
            phoenixReviveUsed = false;
            secondWindUsed = false;
        }

        public bool TryConsumeBanish()
        {
            if (banishesRemaining <= 0)
            {
                banishUsed = true;
                return false;
            }

            banishesRemaining--;
            if (banishesRemaining <= 0) banishUsed = true;
            return true;
        }

        public bool TryAddRelic(string relicId, int maxSlots)
        {
            if (activeRelicIds.Contains(relicId) || activeRelicIds.Count >= maxSlots) return false;
            activeRelicIds.Add(relicId);
            return true;
        }

        public void IncrementComboBurstCount()
        {
            comboBurstCount++;
        }

        public void Flush() { }
    }
}
