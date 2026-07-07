using UnityEngine;

namespace OctoberStudio
{
    [System.Serializable]
    [UnlockCondition("Previous Stage Completed")]
    public class PreviousStageCompletedCondition : StageUnlockCondition
    {
        [SerializeField, HideInInspector] protected string name = "Previous Stage Completed";

        public override bool IsMet(StagesDatabase database, int stageIndex, StageSave stageSave)
        {
            if (stageIndex == 0) return true;
            var prevStage = database.GetStage(stageIndex - 1);

            return stageSave.IsStageCompleted(prevStage);
        }
    }
}