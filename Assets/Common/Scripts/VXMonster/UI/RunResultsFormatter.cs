using UnityEngine;
using VXMonster.Core;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.UI
{
    public static class RunResultsFormatter
    {
        public static string BuildSummary(bool victory)
        {
            var session = GameSessionManager.Instance;
            var stageSave = GameController.SaveManager?.GetSave<StageSave>("Stage");
            if (session == null || stageSave == null) return string.Empty;

            var kills = stageSave.EnemiesKilled;
            var time = FormatTime(stageSave.Time);
            var combos = session.RunSession?.ComboBurstCount ?? 0;
            var difficulty = session.Difficulty.DisplayLabel();
            var mode = session.RunMode.ToString();

            var summary = $"{mode} · {difficulty}\nKills: {kills}  Time: {time}\nCombos: {combos}";

            if (session.RunMode == RunMode.DailyChallenge && session.IsDailyScoredRun)
            {
                var score = session.CalculateDailyScore(kills, stageSave.Time, combos);
                summary += $"\nScore: {score}";
            }

            if (session.RunMode == RunMode.Endless)
            {
                summary += victory
                    ? $"\nLoops cleared: {session.EndlessLoopCount + 1}"
                    : $"\nBest loop: {session.EndlessLoopCount}";
            }

            return summary;
        }

        private static string FormatTime(float seconds)
        {
            var total = Mathf.Max(0, Mathf.FloorToInt(seconds));
            var minutes = total / 60;
            var secs = total % 60;
            return $"{minutes:00}:{secs:00}";
        }
    }
}
