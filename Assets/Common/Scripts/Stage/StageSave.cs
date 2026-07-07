using OctoberStudio.Save;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class StageSave : ISave
    {
        [SerializeField] protected StageSaveData[] stages;
        protected List<StageSaveData> Stages { get; set; }

        [Obsolete]
        [SerializeField] int maxReachedStageId;
        [SerializeField] int selectedStageId;

        [SerializeField] bool isPlaying;
        [SerializeField] float time;
        [SerializeField] bool resetAbilities;
        [SerializeField] int xpLevel;
        [SerializeField] float xp;
        [SerializeField] int enemiesKilled;

        public bool loadedBefore = false;

        public event UnityAction<int> onSelectedStageChanged;

        public int SelectedStageId => selectedStageId;
        [Obsolete]
        public int MaxReachedStageId => maxReachedStageId;

        public bool IsFirstStageSelected => selectedStageId == 0;
        [Obsolete]
        public bool IsMaxReachedStageSelected => selectedStageId == maxReachedStageId;

        public bool IsPlaying { get => isPlaying; set => isPlaying = value; }

        public float Time { get => time; set => time = value; }
        public bool ResetStageData { get => resetAbilities; set => resetAbilities = value; }
        public int XPLEVEL { get => xpLevel; set => xpLevel = value; }
        public float XP { get => xp; set => xp = value; }
        public int EnemiesKilled { get => enemiesKilled; set => enemiesKilled = value; }

        public virtual void Init()
        {
            if (stages == null) stages = new StageSaveData[0];
            Stages = new List<StageSaveData>(stages);
        }

        #region Stage

        public virtual bool IsStageUnlocked(StageData stageData)
        {
            return GetStageSave(stageData).IsUnlocked;
        }

        public virtual void UnlockStage(StageData stageData)
        {
            GetStageSave(stageData).Unlock();
        }

        public virtual bool IsStageCompleted(StageData stageData)
        {
            return GetStageSave(stageData).IsCompleted;
        }

        public virtual void CompleteStage(StageData stageData)
        {
            GetStageSave(stageData).Complete();
        }

        public virtual void IncrementStageAttempts(StageData stageData)
        {
            GetStageSave(stageData).IncrementAttempts();
        }

        public virtual int GetStageAttempts(StageData stageData)
        {
            return GetStageSave(stageData).AttemptsCount;
        }

        protected StageSaveData GetStageSave(StageData stageData)
        {
            for (int i = 0; i < Stages.Count; i++)
            {
                if (stageData.Id == Stages[i].StageId) return Stages[i];
            }

            var stageSaveData = new StageSaveData(stageData.Id);
            Stages.Add(stageSaveData);

            return stageSaveData;
        }
        public void SetSelectedStageId(int selectedStageId)
        {
            this.selectedStageId = selectedStageId;

            onSelectedStageChanged?.Invoke(selectedStageId);
        }

        [Obsolete]
        public void SetMaxReachedStageId(int maxReachedStageId)
        {
            this.maxReachedStageId = maxReachedStageId;
        }

#endregion

        public void Flush()
        {
            stages = Stages.ToArray();
        }

        [System.Serializable]
        public class StageSaveData
        {
            [SerializeField] protected string stageId;
            [SerializeField] protected bool isUnlocked = false;
            [SerializeField] protected bool isCompleted = false;
            [SerializeField] protected int attemptsCount = 0;

            public string StageId => stageId;
            public bool IsUnlocked => isUnlocked;
            public bool IsCompleted => isCompleted;
            public int AttemptsCount => attemptsCount;

            public StageSaveData(string stageId)
            {
                this.stageId = stageId;
            }

            public void Unlock()
            {
                isUnlocked = true;
            }

            public void Complete()
            {
                isCompleted = true;
            }

            public void IncrementAttempts()
            {
                attemptsCount++;
            }
        }
    }
}