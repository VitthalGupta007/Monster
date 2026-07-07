using System.Collections.Generic;
using VXMonster.Core.Save;
using UnityEngine;

namespace VXMonster.Save
{
    public class RunSessionSave : ISave
    {
        [SerializeField] protected List<string> activeRelicIds = new List<string>();
        [SerializeField] protected int comboBurstCount;
        [SerializeField] protected bool banishUsed;
        [SerializeField] protected int rerollsRemaining = 1;
        [SerializeField] protected bool adReviveUsed;
        [SerializeField] protected bool upgradeReviveUsed;

        public IReadOnlyList<string> ActiveRelicIds => activeRelicIds;

        public int ComboBurstCount
        {
            get => comboBurstCount;
            set => comboBurstCount = value;
        }

        public bool BanishUsed
        {
            get => banishUsed;
            set => banishUsed = value;
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

        public void ResetForNewRun(int startingRerolls = 1)
        {
            activeRelicIds.Clear();
            comboBurstCount = 0;
            banishUsed = false;
            rerollsRemaining = startingRerolls;
            adReviveUsed = false;
            upgradeReviveUsed = false;
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
