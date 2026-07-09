using UnityEngine;
using VXMonster.Core;
using VXMonster.Gameplay;

namespace VXMonster.UI
{
    public static class RunRetryUtility
    {
        public static void RetryCurrentRun()
        {
            Time.timeScale = 1f;

            if (GameController.SaveManager == null) return;

            var save = GameController.SaveManager.GetSave<StageSave>("Stage");
            var session = GameSessionManager.Instance;

            save.IsPlaying = true;
            save.ResetStageData = true;
            save.Time = 0f;
            save.XP = 0f;
            save.XPLEVEL = 0;
            save.EnemiesKilled = 0;

            if (session != null)
            {
                // OnGameFailed clears persisted mid-run snapshot but keeps in-memory RunMode.
                switch (session.RunMode)
                {
                    case RunMode.Campaign:
                        session.ConfigureCampaign(session.Difficulty);
                        break;
                    case RunMode.DailyChallenge:
                        session.ConfigureDailyChallenge(session.IsDailyScoredRun);
                        break;
                    case RunMode.Endless:
                        session.ConfigureEndless(session.Difficulty);
                        break;
                }
            }

            GameController.LoadStage();
        }
    }
}
