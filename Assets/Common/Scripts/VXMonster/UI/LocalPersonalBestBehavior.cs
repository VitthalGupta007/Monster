using TMPro;
using UnityEngine;
using VXMonster.Core;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.UI
{
    public class LocalPersonalBestBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text dailyBestLabel;
        [SerializeField] TMP_Text endlessBestLabel;
        [SerializeField] TMP_Text streakLabel;
        [SerializeField] string dailyFormat = "Daily best: {0}";
        [SerializeField] string endlessFormat = "Endless best: {0} loops";
        [SerializeField] string streakFormat = "Daily streak: {0}";

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (GameController.SaveManager == null) return;

            var daily = GameController.SaveManager.GetSave<DailyChallengeSave>("VX Daily Challenge");
            var lifetime = GameController.SaveManager.GetSave<LifetimeStatsSave>("VX Lifetime Stats");

            if (dailyBestLabel != null)
            {
                dailyBestLabel.text = string.Format(dailyFormat, daily.BestDailyScore);
            }

            if (endlessBestLabel != null)
            {
                endlessBestLabel.text = string.Format(endlessFormat, lifetime.EndlessLoopsBest);
            }

            if (streakLabel != null)
            {
                streakLabel.text = lifetime.DailyStreak > 0
                    ? string.Format(streakFormat, lifetime.DailyStreak)
                    : string.Empty;
            }
        }
    }
}
