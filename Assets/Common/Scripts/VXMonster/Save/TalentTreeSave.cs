using System.Collections.Generic;
using VXMonster.Core.Save;
using UnityEngine;

namespace VXMonster.Save
{
    public class TalentTreeSave : ISave
    {
        [SerializeField] protected List<string> unlockedNodeIds = new List<string>();
        [SerializeField] protected int talentPoints;

        public IReadOnlyList<string> UnlockedNodeIds => unlockedNodeIds;
        public int TalentPoints => talentPoints;

        public bool IsUnlocked(string nodeId)
        {
            return unlockedNodeIds.Contains(nodeId);
        }

        public bool TryUnlock(string nodeId, int cost = 0)
        {
            if (unlockedNodeIds.Contains(nodeId)) return false;
            if (cost > 0 && talentPoints < cost) return false;

            if (cost > 0) talentPoints -= cost;
            unlockedNodeIds.Add(nodeId);
            return true;
        }

        public void AddPoints(int amount)
        {
            if (amount <= 0) return;
            talentPoints += amount;
        }

        public void Flush() { }
    }
}
