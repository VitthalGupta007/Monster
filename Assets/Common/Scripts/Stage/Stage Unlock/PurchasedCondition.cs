using OctoberStudio.Currency;
using UnityEngine;

namespace OctoberStudio
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