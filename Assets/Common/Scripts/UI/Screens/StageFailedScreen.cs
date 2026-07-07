using VXMonster.Core.Audio;
using VXMonster.Core.Easing;
using VXMonster.Core.Input;
using VXMonster.Core.Upgrades;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VXMonster.Gameplay;
using VXMonster.Platform;

namespace VXMonster.Core.UI
{
    public class StageFailedScreen : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button reviveButton;
        [SerializeField] Button adReviveButton;
        [SerializeField] Button exitButton;

        private Canvas canvas;

        private bool upgradeReviveUsed;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();

            reviveButton.onClick.AddListener(ReviveButtonClick);
            if (adReviveButton != null)
            {
                adReviveButton.onClick.AddListener(AdReviveButtonClick);
            }

            exitButton.onClick.AddListener(ExitButtonClick);

            upgradeReviveUsed = false;
        }

        public void Show()
        {
            gameObject.SetActive(true);

            canvasGroup.alpha = 0;
            canvasGroup.DoAlpha(1, 0.3f).SetUnscaledTime(true);

            RefreshReviveButtons();

            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        private void RefreshReviveButtons()
        {
            var session = GameSessionManager.Instance?.RunSession;
            var canUpgradeRevive = GameController.UpgradesManager.IsUpgradeAquired(UpgradeType.Revive)
                                   && !upgradeReviveUsed
                                   && (session == null || !session.UpgradeReviveUsed);

            var canAdRevive = session == null || !session.AdReviveUsed;
            var adReady = PlatformServices.AdService != null && PlatformServices.AdService.IsRewardedReady;

            reviveButton.gameObject.SetActive(canUpgradeRevive);
            if (adReviveButton != null)
            {
                adReviveButton.gameObject.SetActive(canAdRevive && adReady);
            }

            if (canUpgradeRevive)
            {
                EventSystem.current.SetSelectedGameObject(reviveButton.gameObject);
            }
            else if (adReviveButton != null && canAdRevive && adReady)
            {
                EventSystem.current.SetSelectedGameObject(adReviveButton.gameObject);
            }
            else
            {
                EventSystem.current.SetSelectedGameObject(exitButton.gameObject);
            }
        }

        public void Hide(UnityAction onFinish)
        {
            canvasGroup.DoAlpha(0, 0.3f).SetUnscaledTime(true).SetOnFinish(() => {
                gameObject.SetActive(false);
                onFinish?.Invoke();
            });

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        private void ReviveButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            upgradeReviveUsed = true;
            if (GameSessionManager.Instance?.RunSession != null)
            {
                GameSessionManager.Instance.RunSession.UpgradeReviveUsed = true;
            }

            Hide(StageController.ResurrectPlayer);
        }

        private void AdReviveButtonClick()
        {
            if (PlatformServices.AdService == null || !PlatformServices.AdService.IsRewardedReady) return;

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            PlatformServices.AdService.ShowRewarded(
                onRewardGranted: () =>
                {
                    if (GameSessionManager.Instance?.RunSession != null)
                    {
                        GameSessionManager.Instance.RunSession.AdReviveUsed = true;
                    }

                    Hide(StageController.ResurrectPlayer);
                },
                onClosed: () =>
                {
                    PlatformServices.AdService.LoadRewarded();
                });
        }

        private void ExitButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            Time.timeScale = 1;

            if (!PlatformServices.TryShowInterstitial(() => StageController.ReturnToMainMenu()))
            {
                StageController.ReturnToMainMenu();
            }

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        private void OnInputChanged(InputType prevInput, InputType inputType)
        {
            if (prevInput == InputType.UIJoystick)
            {
                RefreshReviveButtons();
            }
        }
    }
}
