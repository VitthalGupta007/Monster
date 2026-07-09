using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.UI
{
    public class TutorialOverlayBehavior : MonoBehaviour
    {
        private const string SaveKey = "VX Tutorial";

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TMP_Text bodyText;
        [SerializeField] Button nextButton;
        [SerializeField] Button skipButton;

        private TutorialSave tutorialSave;
        private int stepIndex;
        private readonly string[] steps =
        {
            "Drag the joystick to move. Avoid enemies and collect XP gems.",
            "Level up to choose new abilities. Combine elements for combo bursts!",
            "Open chests for gold and relics. Relics change your whole run.",
            "Try Daily Challenge and Endless from the lobby. Pick your difficulty before you play."
        };

        private void Awake()
        {
            if (nextButton != null) nextButton.onClick.AddListener(Advance);
            if (skipButton != null) skipButton.onClick.AddListener(Complete);
        }

        private void Start()
        {
            TryShowFirstRun();
        }

        public void TryShowFirstRun()
        {
            if (GameController.SaveManager == null) return;

            tutorialSave = GameController.SaveManager.GetSave<TutorialSave>(SaveKey);
            if (tutorialSave.Completed) return;

            stepIndex = tutorialSave.LastStepIndex;
            gameObject.SetActive(true);
            if (canvasGroup != null) canvasGroup.alpha = 1f;
            ShowStep();
        }

        private void ShowStep()
        {
            if (bodyText != null && stepIndex < steps.Length)
            {
                bodyText.text = steps[stepIndex];
            }

            if (nextButton != null)
            {
                var nextLabel = nextButton.GetComponentInChildren<TMP_Text>();
                if (nextLabel != null)
                {
                    nextLabel.text = stepIndex >= steps.Length - 1 ? "Done" : "Next";
                }
            }
        }

        private void Advance()
        {
            stepIndex++;
            if (stepIndex >= steps.Length)
            {
                Complete();
                return;
            }

            if (tutorialSave != null)
            {
                tutorialSave.LastStepIndex = stepIndex;
                GameController.SaveManager?.Save(false);
            }

            ShowStep();
        }

        private void Complete()
        {
            if (tutorialSave != null)
            {
                tutorialSave.Completed = true;
                GameController.SaveManager?.Save(false);
            }

            gameObject.SetActive(false);
        }
    }
}
