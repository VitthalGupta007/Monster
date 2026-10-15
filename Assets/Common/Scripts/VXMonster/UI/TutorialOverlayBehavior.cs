using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core;
using VXMonster.Core.Save;
using VXMonster.Platform.Analytics;
using VXMonster.Save;
using VXMonster.Tutorial;

namespace VXMonster.UI
{
    public class TutorialOverlayBehavior : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] Button nextButton;
        [SerializeField] Button skipButton;
        [SerializeField] TutorialSequenceDefinition lobbySequence;

        private TutorialSave tutorialSave;
        private int stepIndex;
        private TutorialStepData[] steps;
        private bool showing;

        /// <summary>True while the lobby first-run tutorial is visible and blocking input.</summary>
        public bool IsShowing => showing && gameObject.activeInHierarchy;

        private void OnEnable()
        {
            BindButtons();
        }

        private void OnDisable()
        {
            if (nextButton != null) nextButton.onClick.RemoveListener(Advance);
            if (skipButton != null) skipButton.onClick.RemoveListener(Skip);
        }

        public void OnNextClicked()
        {
            Advance();
        }

        public void OnSkipClicked()
        {
            Skip();
        }

        private void BindButtons()
        {
            if (nextButton == null)
            {
                var nextTransform = transform.Find("Card/Next Button") ?? transform.Find("Next Button");
                if (nextTransform != null) nextButton = nextTransform.GetComponent<Button>();
            }

            if (skipButton == null)
            {
                var skipTransform = transform.Find("Card/Skip Button") ?? transform.Find("Skip Button");
                if (skipTransform != null) skipButton = skipTransform.GetComponent<Button>();
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(Advance);
                nextButton.onClick.AddListener(Advance);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveListener(Skip);
                skipButton.onClick.AddListener(Skip);
            }
        }

        private void Start()
        {
            // Lobby open order: Tutorial (new user) → Daily Claim → Continue run.
            TryShowFirstRun();
        }

        public void TryShowFirstRun()
        {
            tutorialSave = ResolveTutorialSave();
            if (tutorialSave == null || tutorialSave.Completed)
            {
                Hide();
                return;
            }

            ShowFrom(tutorialSave.LastStepIndex);
        }

        public void ReplayLobby()
        {
            tutorialSave = ResolveTutorialSave();
            if (tutorialSave == null) return;

            tutorialSave.ResetLobby();
            PersistSave();
            ShowFrom(0);
        }

        private static TutorialSave ResolveTutorialSave()
        {
            var saveManager = ResolveSaveManager();
            if (saveManager == null) return null;
            return saveManager.GetSave<TutorialSave>(TutorialSave.SaveHash);
        }

        private static ISaveManager ResolveSaveManager()
        {
            if (GameController.SaveManager != null) return GameController.SaveManager;

            var found = Object.FindAnyObjectByType<SaveManager>(FindObjectsInactive.Include);
            if (found == null) return null;

            GameController.RegisterSaveManager(found);
            return found;
        }

        private void PersistSave()
        {
            var saveManager = ResolveSaveManager();
            if (saveManager is SaveManager concrete)
            {
                concrete.SaveImmediate();
                return;
            }

            saveManager?.Save(false);
        }

        private void ShowFrom(int startIndex)
        {
            steps = ResolveSteps();
            stepIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, steps.Length - 1));

            showing = true;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }

            ShowStep();
        }

        private TutorialStepData[] ResolveSteps()
        {
            if (lobbySequence != null && lobbySequence.steps != null && lobbySequence.steps.Length > 0)
            {
                return lobbySequence.steps;
            }

            return new[]
            {
                new TutorialStepData
                {
                    stepId = TutorialCopy.LobbyStepWelcomeId,
                    title = TutorialCopy.LobbyWelcomeTitle,
                    body = TutorialCopy.LobbyWelcomeBody
                },
                new TutorialStepData
                {
                    stepId = TutorialCopy.LobbyStepModesId,
                    title = TutorialCopy.LobbyModesTitle,
                    body = TutorialCopy.LobbyModesBody
                }
            };
        }

        private void ShowStep()
        {
            if (steps == null || stepIndex < 0 || stepIndex >= steps.Length) return;

            var step = steps[stepIndex];
            if (titleText != null)
            {
                titleText.text = step.title ?? string.Empty;
                titleText.gameObject.SetActive(!string.IsNullOrEmpty(step.title));
            }

            if (bodyText != null)
            {
                bodyText.text = step.body ?? string.Empty;
            }

            if (nextButton != null)
            {
                var nextLabel = nextButton.GetComponentInChildren<TMP_Text>();
                if (nextLabel != null)
                {
                    nextLabel.text = stepIndex >= steps.Length - 1 ? "Done" : "Next";
                }
            }

            AnalyticsEvents.LogTutorialStep(step.stepId ?? $"lobby_{stepIndex}");
        }

        private void Advance()
        {
            stepIndex++;
            if (steps == null || stepIndex >= steps.Length)
            {
                Complete(false);
                return;
            }

            if (tutorialSave == null) tutorialSave = ResolveTutorialSave();
            if (tutorialSave != null)
            {
                tutorialSave.LastStepIndex = stepIndex;
                PersistSave();
            }

            ShowStep();
        }

        private void Skip()
        {
            var stepId = steps != null && stepIndex >= 0 && stepIndex < steps.Length
                ? steps[stepIndex].stepId
                : "lobby";
            AnalyticsEvents.LogTutorialSkip(stepId);
            Complete(true);
        }

        private void Complete(bool skipped)
        {
            if (tutorialSave == null) tutorialSave = ResolveTutorialSave();
            if (tutorialSave != null)
            {
                tutorialSave.Completed = true;
                PersistSave();
            }

            if (!skipped)
            {
                AnalyticsEvents.LogTutorialComplete(lobbySequence != null ? lobbySequence.sequenceId : "LobbyIntro");
            }

            Hide();
        }

        private void Hide()
        {
            showing = false;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
        }
    }
}
