using VXMonster.Core.Save;
using UnityEngine;

namespace VXMonster.Save
{
    public class WeeklyChallengeSave : ISave
    {
        public const string SaveKey = "VX Weekly Challenge";

        [SerializeField] protected string lastScoredWeek = string.Empty;
        [SerializeField] protected int bestWeeklyScore;
        [SerializeField] protected int thisWeekBestScore;

        public string LastScoredWeek
        {
            get => lastScoredWeek;
            set => lastScoredWeek = value;
        }

        public int BestWeeklyScore
        {
            get => bestWeeklyScore;
            set => bestWeeklyScore = value;
        }

        public int ThisWeekBestScore
        {
            get => thisWeekBestScore;
            set => thisWeekBestScore = value;
        }

        public bool HasScoredThisWeek(string utcWeekKey)
        {
            return lastScoredWeek == utcWeekKey;
        }

        public void RecordScore(string utcWeekKey, int score)
        {
            if (lastScoredWeek != utcWeekKey)
            {
                thisWeekBestScore = 0;
                lastScoredWeek = utcWeekKey;
            }

            thisWeekBestScore = Mathf.Max(thisWeekBestScore, score);
            bestWeeklyScore = Mathf.Max(bestWeeklyScore, score);
        }

        public void Flush() { }
    }
}
