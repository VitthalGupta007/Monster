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
    /// Wires lobby mode buttons under Lobby Window.
    /// Weekly sits in the same row as Daily / Practice / Endless (never over Play).
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class VXLobbyModePanel : MonoBehaviour
    {
        private const string RootName = "VX Mode Buttons";

        // Same band as DAILY / PRACTICE / ENDLESS — clear gap above Play's frame.
        private const float ModeRowY = -440f;
        private const float ModeButtonWidth = 155f;
        private const float ModeButtonHeight = 64f;
        private const float ModeStep = 168f;

        [SerializeField] LobbyWindowBehavior lobbyWindow;
        [SerializeField] Button difficultyButton;
        [SerializeField] Button dailyButton;
        [SerializeField] Button practiceButton;
        [SerializeField] Button endlessButton;
        [SerializeField] Button weeklyButton;
        [SerializeField] TMP_Text difficultyLabel;

        private DailyChallengeSave dailySave;
        private WeeklyChallengeSave weeklySave;
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
            EnsureWeeklyButton();
            LayoutModeRow();

            if (GameController.SaveManager != null)
            {
                dailySave = GameController.SaveManager.GetSave<DailyChallengeSave>("VX Daily Challenge");
                weeklySave = GameController.SaveManager.GetSave<WeeklyChallengeSave>(WeeklyChallengeSave.SaveKey);
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

            if (weeklyButton != null)
            {
                weeklyButton.onClick.RemoveListener(OnWeeklyClicked);
                weeklyButton.onClick.AddListener(OnWeeklyClicked);
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

            if (weeklyButton == null)
            {
                weeklyButton = root.Find("Weekly Challenge")?.GetComponent<Button>();
            }
        }

        private void EnsureWeeklyButton()
        {
            var root = transform.Find(RootName);
            if (root == null || dailyButton == null) return;

            if (weeklyButton == null)
            {
                var clone = Instantiate(dailyButton.gameObject, root);
                clone.name = "Weekly Challenge";
                var label = clone.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = "WEEKLY";
                weeklyButton = clone.GetComponent<Button>();
            }

            // Draw mode chips above Play so the brown Play frame never clips them.
            // Vertical gap is large enough that they do not cover the PLAY label.
            var playBg = transform.Find("Play Button Background");
            if (playBg != null)
            {
                root.SetSiblingIndex(playBg.GetSiblingIndex() + 1);
            }
        }

        /// <summary>
        /// Four equal mode chips in one row: Daily · Practice · Endless · Weekly.
        /// </summary>
        private void LayoutModeRow()
        {
            PlaceModeButton(dailyButton, -1.5f * ModeStep);
            PlaceModeButton(practiceButton, -0.5f * ModeStep);
            PlaceModeButton(endlessButton, 0.5f * ModeStep);
            PlaceModeButton(weeklyButton, 1.5f * ModeStep);
        }

        private static void PlaceModeButton(Button button, float x)
        {
            if (button == null) return;
            var rect = button.transform as RectTransform;
            if (rect == null) return;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, ModeRowY);
            rect.sizeDelta = new Vector2(ModeButtonWidth, ModeButtonHeight);
            rect.localScale = Vector3.one;
            button.transform.SetAsLastSibling();
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

        private void OnWeeklyClicked()
        {
            var weekKey = GameSessionManager.GetUtcWeekKey();
            var scored = weeklySave == null || !weeklySave.HasScoredThisWeek(weekKey);
            lobbyWindow.StartWeeklyChallenge(scored);
        }
    }
}
