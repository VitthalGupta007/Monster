using System.Collections.Generic;
using UnityEngine;
using VXMonster.Core;
using VXMonster.Core.Abilities.UI;
using VXMonster.Core.UI;
using VXMonster.Gameplay;
using VXMonster.Platform.Analytics;
using VXMonster.Save;
using VXMonster.UI;

namespace VXMonster.Tutorial
{
    public class TutorialInRunController : MonoBehaviour
    {
        [SerializeField] JoystickBehavior joystick;
        [SerializeField] AbilitiesWindowBehavior abilitiesWindow;
        [SerializeField] ChestWindowBehavior chestWindow;
        [SerializeField] RectTransform relicHud;
        [SerializeField] RectTransform comboHud;
        [SerializeField] TutorialHighlightOverlay overlay;

        private TutorialSave tutorialSave;
        private string activeStepId;
        private readonly Queue<PendingTip> queue = new Queue<PendingTip>();
        private bool waitingForMove;
        private bool started;
        private bool movePending;

        private struct PendingTip
        {
            public string stepId;
            public string body;
            public RectTransform focus;
        }

        public void Bind(
            JoystickBehavior joystickRef,
            AbilitiesWindowBehavior abilities,
            ChestWindowBehavior chest,
            RectTransform relic,
            RectTransform combo)
        {
            joystick = joystickRef;
            abilitiesWindow = abilities;
            chestWindow = chest;
            relicHud = relic;
            comboHud = combo;
        }

        public void Begin()
        {
            if (started || GameController.SaveManager == null) return;
            started = true;

            tutorialSave = GameController.SaveManager.GetSave<TutorialSave>(TutorialSave.SaveHash);

            if (abilitiesWindow != null)
            {
                abilitiesWindow.onPanelClosed -= OnAbilitiesClosed;
                abilitiesWindow.onPanelClosed += OnAbilitiesClosed;
            }

            if (chestWindow != null)
            {
                chestWindow.OnClosed -= OnChestClosed;
                chestWindow.OnClosed += OnChestClosed;
            }

            RelicsManager.RelicGranted -= OnRelicGranted;
            RelicsManager.RelicGranted += OnRelicGranted;
            ComboResolver.ComboTriggered -= OnComboTriggered;
            ComboResolver.ComboTriggered += OnComboTriggered;

            movePending = !tutorialSave.HasSeenStep(TutorialCopy.InRunMoveId);
        }

        private void OnDestroy()
        {
            if (abilitiesWindow != null)
            {
                abilitiesWindow.onPanelClosed -= OnAbilitiesClosed;
            }

            if (chestWindow != null)
            {
                chestWindow.OnClosed -= OnChestClosed;
            }

            RelicsManager.RelicGranted -= OnRelicGranted;
            ComboResolver.ComboTriggered -= OnComboTriggered;
        }

        private void Update()
        {
            if (movePending && string.IsNullOrEmpty(activeStepId) && CanShowMoveTip())
            {
                movePending = false;
                TryShow(TutorialCopy.InRunMoveId, TutorialCopy.MoveTip, joystick != null ? joystick.transform as RectTransform : null);
            }

            if (!waitingForMove || GameController.InputManager == null) return;
            if (GameController.InputManager.MovementValue.sqrMagnitude <= 0f) return;

            CompleteActive();
        }

        private bool CanShowMoveTip()
        {
            if (Time.timeScale <= 0f) return false;
            return abilitiesWindow == null || !abilitiesWindow.gameObject.activeInHierarchy;
        }

        public void NotifyAbilityPanelShown()
        {
            if (activeStepId == TutorialCopy.InRunMoveId)
            {
                waitingForMove = false;
                movePending = true;
                activeStepId = null;
                overlay?.Hide();
            }

            var focus = abilitiesWindow != null ? abilitiesWindow.transform as RectTransform : null;
            TryShow(TutorialCopy.InRunLevelUpId, TutorialCopy.LevelUpTip, focus);
        }

        public void NotifyChestOpened()
        {
            var focus = chestWindow != null ? chestWindow.transform as RectTransform : null;
            TryShow(TutorialCopy.InRunChestId, TutorialCopy.ChestTip, focus);
        }

        private void OnRelicGranted()
        {
            TryShow(TutorialCopy.InRunRelicId, TutorialCopy.RelicTip, relicHud);
        }

        private void OnComboTriggered()
        {
            TryShow(TutorialCopy.InRunComboId, TutorialCopy.ComboTip, comboHud);
        }

        private void OnAbilitiesClosed()
        {
            if (activeStepId == TutorialCopy.InRunLevelUpId) CompleteActive();
        }

        private void OnChestClosed()
        {
            if (activeStepId == TutorialCopy.InRunChestId) CompleteActive();
        }

        private void TryShow(string stepId, string body, RectTransform focus)
        {
            if (tutorialSave == null || tutorialSave.HasSeenStep(stepId)) return;

            if (!string.IsNullOrEmpty(activeStepId))
            {
                if (queue.Count == 0)
                {
                    queue.Enqueue(new PendingTip { stepId = stepId, body = body, focus = focus });
                }

                return;
            }

            ShowNow(stepId, body, focus);
        }

        private void EnsureOverlay()
        {
            if (overlay == null)
            {
                overlay = TutorialHighlightOverlay.Create(transform);
            }

            overlay.BindSkip(SkipActive);
        }

        private void ShowNow(string stepId, string body, RectTransform focus)
        {
            EnsureOverlay();
            activeStepId = stepId;
            waitingForMove = stepId == TutorialCopy.InRunMoveId;
            overlay.Show(body, focus);
            AnalyticsEvents.LogTutorialStep(stepId);
        }

        public void SkipActive()
        {
            if (string.IsNullOrEmpty(activeStepId)) return;
            AnalyticsEvents.LogTutorialSkip(activeStepId);
            MarkAndAdvance(activeStepId);
        }

        private void CompleteActive()
        {
            if (string.IsNullOrEmpty(activeStepId)) return;
            AnalyticsEvents.LogTutorialComplete(activeStepId);
            MarkAndAdvance(activeStepId);
        }

        private void MarkAndAdvance(string stepId)
        {
            tutorialSave.MarkStepSeen(stepId);
            if (GameController.SaveManager is VXMonster.Core.Save.SaveManager saveManager)
            {
                saveManager.SaveImmediate();
            }
            else
            {
                GameController.SaveManager?.Save(false);
            }
            waitingForMove = false;
            activeStepId = null;
            overlay.Hide();

            if (queue.Count == 0) return;

            var next = queue.Dequeue();
            if (!tutorialSave.HasSeenStep(next.stepId))
            {
                ShowNow(next.stepId, next.body, next.focus);
            }
        }
    }
}
