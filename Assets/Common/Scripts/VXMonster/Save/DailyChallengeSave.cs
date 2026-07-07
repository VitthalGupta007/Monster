using VXMonster.Core.Save;
using UnityEngine;

namespace VXMonster.Save
{
    public class DailyChallengeSave : ISave
    {
        [SerializeField] protected string lastScoredDate = string.Empty;
        [SerializeField] protected int bestDailyScore;
        [SerializeField] protected int todayBestScore;

        public string LastScoredDate
        {
            get => lastScoredDate;
            set => lastScoredDate = value;
        }

        public int BestDailyScore
        {
            get => bestDailyScore;
            set => bestDailyScore = value;
        }

        public int TodayBestScore
        {
            get => todayBestScore;
            set => todayBestScore = value;
        }

        public bool HasScoredToday(string utcDateKey)
        {
            return lastScoredDate == utcDateKey;
        }

        public void RecordScore(string utcDateKey, int score)
        {
            if (lastScoredDate != utcDateKey)
            {
                todayBestScore = 0;
                lastScoredDate = utcDateKey;
            }

            todayBestScore = Mathf.Max(todayBestScore, score);
            bestDailyScore = Mathf.Max(bestDailyScore, score);
        }

        public void Flush() { }
    }
}
