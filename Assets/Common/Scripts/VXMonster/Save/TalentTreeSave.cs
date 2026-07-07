using System.Collections.Generic;
using OctoberStudio.Save;
using UnityEngine;

namespace VXMonster.Save
{
    public class TalentTreeSave : ISave
    {
        [SerializeField] protected List<string> unlockedNodeIds = new List<string>();

        public IReadOnlyList<string> UnlockedNodeIds => unlockedNodeIds;

        public bool IsUnlocked(string nodeId)
        {
            return unlockedNodeIds.Contains(nodeId);
        }

        public bool TryUnlock(string nodeId)
        {
            if (unlockedNodeIds.Contains(nodeId)) return false;
            unlockedNodeIds.Add(nodeId);
            return true;
        }

        public void Flush() { }
    }
}
