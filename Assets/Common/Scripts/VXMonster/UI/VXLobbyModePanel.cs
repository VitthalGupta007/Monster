using VXMonster.Core;
using VXMonster.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.UI
{
    /// <summary>
    /// Wires lobby mode buttons that live as editable prefab children under Lobby Window.
    /// Does not create or reposition UI at runtime — move them in the prefab / Scene view.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class VXLobbyModePanel : MonoBehaviour
    {
        private const string RootName = "VX Mode Buttons";

        [SerializeField] LobbyWindowBehavior lobbyWindow;
        [SerializeField] Button difficultyButton;
        [SerializeField] Button dailyButton;
        [SerializeField] Button practiceButton;
        [SerializeField] Button endlessButton;
        [SerializeField] TMP_Text difficultyLabel;

        private DailyChallengeSave dailySave;
        private bool wired;

        private void Start()
        {
            Wire();
        }

        private void Wire()
        {
            if (wired) return;

            if (lobbyWindow == null)
            {
                lobbyWindow = GetComponent<LobbyWindowBehavior>() ?? FindAnyObjectByType<LobbyWindowBehavior>();
            }

            if (lobbyWindow == null) return;

            ResolveButtonRefs();

            if (GameController.SaveManager != null)
            {
                dailySave = GameController.SaveManager.GetSave<DailyChallengeSave>("VX Daily Challenge");
            }

            if (difficultyButton != null)
            {
                difficultyButton.onClick.RemoveListener(OnDifficultyClicked);
                difficultyButton.onClick.AddListener(OnDifficultyClicked);
            }

            if (dailyButton != null)
            {
                dailyButton.onClick.RemoveListener(OnDailyScoredClicked);
                dailyButton.onClick.AddListener(OnDailyScoredClicked);
            }

            if (practiceButton != null)
            {
                practiceButton.onClick.RemoveListener(OnDailyPracticeClicked);
                practiceButton.onClick.AddListener(OnDailyPracticeClicked);
            }

            if (endlessButton != null)
            {
                endlessButton.onClick.RemoveListener(OnEndlessClicked);
                endlessButton.onClick.AddListener(OnEndlessClicked);
            }

            if (difficultyLabel == null && difficultyButton != null)
            {
                difficultyLabel = difficultyButton.GetComponentInChildren<TMP_Text>();
            }

            RefreshDifficultyLabel();
            wired = true;
        }

        private void ResolveButtonRefs()
        {
            var root = transform.Find(RootName);
            if (root == null) return;

            if (difficultyButton == null)
            {
                difficultyButton = root.Find("Difficulty Button")?.GetComponent<Button>();
            }

            if (dailyButton == null)
            {
                dailyButton = root.Find("Daily Challenge")?.GetComponent<Button>();
            }

            if (practiceButton == null)
            {
                practiceButton = root.Find("Practice")?.GetComponent<Button>();
            }

            if (endlessButton == null)
            {
                endlessButton = root.Find("Endless")?.GetComponent<Button>();
            }
        }

        private void RefreshDifficultyLabel()
        {
            if (difficultyLabel != null)
            {
                difficultyLabel.text = FormatDifficultyLabel(VXDifficultySelection.Selected);
            }
        }

        private static string FormatDifficultyLabel(DifficultyTier tier)
        {
            return $"DIFF · {tier.DisplayLabel().ToUpperInvariant()}";
        }

        private void OnDifficultyClicked()
        {
            var modal = FindAnyObjectByType<DifficultyModalWindowBehavior>();
            if (modal != null)
            {
                modal.Open(RefreshDifficultyLabel);
                return;
            }

            VXDifficultySelection.CycleNext();
            RefreshDifficultyLabel();
        }

        private void OnDailyScoredClicked()
        {
            if (dailySave != null && dailySave.HasScoredToday(GameSessionManager.GetUtcDateKey()))
            {
                lobbyWindow.StartDailyChallenge(false);
                return;
            }

            lobbyWindow.StartDailyChallenge(true);
        }

        private void OnDailyPracticeClicked()
        {
            lobbyWindow.StartDailyChallenge(false);
        }

        private void OnEndlessClicked()
        {
            lobbyWindow.StartEndlessRun(VXDifficultySelection.Selected);
        }
    }
}
