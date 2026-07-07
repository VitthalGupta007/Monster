using OctoberStudio.Save;
using UnityEngine;

namespace VXMonster.Save
{
    public class LifetimeStatsSave : ISave
    {
        [SerializeField] protected int totalEnemiesKilled;
        [SerializeField] protected int totalRunsCompleted;
        [SerializeField] protected int totalRunsFailed;
        [SerializeField] protected int endlessLoopsBest;
        [SerializeField] protected int dailyStreak;
        [SerializeField] protected string lastDailyDate = string.Empty;

        public int TotalEnemiesKilled
        {
            get => totalEnemiesKilled;
            set => totalEnemiesKilled = value;
        }

        public int TotalRunsCompleted
        {
            get => totalRunsCompleted;
            set => totalRunsCompleted = value;
        }

        public int TotalRunsFailed
        {
            get => totalRunsFailed;
            set => totalRunsFailed = value;
        }

        public int EndlessLoopsBest
        {
            get => endlessLoopsBest;
            set => endlessLoopsBest = Mathf.Max(endlessLoopsBest, value);
        }

        public int DailyStreak
        {
            get => dailyStreak;
            set => dailyStreak = value;
        }

        public string LastDailyDate
        {
            get => lastDailyDate;
            set => lastDailyDate = value;
        }

        public void AddKills(int count)
        {
            if (count <= 0) return;
            totalEnemiesKilled += count;
        }

        public void IncrementRunsCompleted()
        {
            totalRunsCompleted++;
        }

        public void IncrementRunsFailed()
        {
            totalRunsFailed++;
        }

        public void UpdateEndlessLoopsBest(int loops)
        {
            endlessLoopsBest = Mathf.Max(endlessLoopsBest, loops);
        }

        public void Flush() { }
    }
}
