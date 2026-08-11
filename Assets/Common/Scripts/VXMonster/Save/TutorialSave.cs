using System.Collections.Generic;
using UnityEngine;
using VXMonster.Core.Save;

namespace VXMonster.Save
{
    [System.Serializable]
    public class TutorialSave : ISave
    {
        public const string SaveKey = "VX Tutorial";

        public static int SaveHash
        {
            get
            {
                unchecked
                {
                    var hash = 17;
                    var key = SaveKey;
                    for (int i = 0; i < key.Length; i++)
                    {
                        hash = hash * 31 + key[i];
                    }

                    return hash;
                }
            }
        }

        [SerializeField] protected bool completed;
        [SerializeField] protected int lastStepIndex;
        [SerializeField] protected List<string> seenStepIds = new List<string>();

        public bool Completed
        {
            get => completed;
            set => completed = value;
        }

        public int LastStepIndex
        {
            get => lastStepIndex;
            set => lastStepIndex = value;
        }

        public bool HasSeenStep(string stepId)
        {
            if (string.IsNullOrEmpty(stepId) || seenStepIds == null) return false;
            return seenStepIds.Contains(stepId);
        }

        public void MarkStepSeen(string stepId)
        {
            if (string.IsNullOrEmpty(stepId)) return;
            if (seenStepIds == null) seenStepIds = new List<string>();
            if (!seenStepIds.Contains(stepId)) seenStepIds.Add(stepId);
        }

        public void ResetLobby()
        {
            completed = false;
            lastStepIndex = 0;
        }

        public void ResetAll()
        {
            completed = false;
            lastStepIndex = 0;
            if (seenStepIds == null) seenStepIds = new List<string>();
            else seenStepIds.Clear();
        }

        public void Flush() { }
    }
}
