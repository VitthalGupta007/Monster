namespace OctoberStudio
{
    [System.Serializable]
    public abstract class StageUnlockCondition
    {
        public abstract bool IsMet(StagesDatabase database, int stageIndex, StageSave stageSave);
    }
}