using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core
{
    [System.Serializable]
    public class StageUnlockData
    {
        [SerializeField, TextArea] protected string unlockDescription;
        public string UnlockDescription => unlockDescription;

        [SerializeReference] protected List<StageUnlockCondition> unlockConditions;
        public List<StageUnlockCondition> UnlockConditions => unlockConditions;

        public virtual PurchasedCondition GetPurchaseUnlockCondition()
        {
            for (int i = 0; i < unlockConditions.Count; i++)
            {
                if (unlockConditions[i] is PurchasedCondition condition) return condition;
            }

            return null;
        }
    }
}