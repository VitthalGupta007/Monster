using VXMonster.Core.Currency;
using UnityEngine;

namespace VXMonster.Core
{
    [System.Serializable]
    [UnlockCondition("Purchased")]
    public class PurchasedCondition : StageUnlockCondition
    {
        [SerializeField] protected Price price;
        public Price Price => price;

        public override bool IsMet(StagesDatabase database, int stageIndex, StageSave stageSave)
        {
            return false;
        }
    }
}