using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core
{
    [System.Serializable]
    [UnlockCondition("One Of Stages Completed")]
    public class AnyStageCompletedCondition : StageUnlockCondition
    {
        [SerializeField, HideInInspector] protected string name = "One Of Stages Completed";

        [SerializeField] protected List<StageData> stages;
        public List<StageData> Stages => stages;

        public override bool IsMet(StagesDatabase database, int stageIndex, StageSave stageSave)
        {
            for (int i = 0; i < stages.Count; i++)
            {
                var stageData = stages[i];

                if (stageData == null) continue;
                if (stageSave.IsStageCompleted(stageData)) return true;
            }

            return false;
        }
    }
}