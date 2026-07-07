using VXMonster.Core.Audio;
using VXMonster.Core.Easing;
using VXMonster.Core.Input;
using VXMonster.Core.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VXMonster.Gameplay;
using VXMonster.Platform;
using VXMonster.Save;
using VXMonster.UI;
using System.Collections;

namespace VXMonster.Core.UI
{
    public class StageFailedScreen : MonoBehaviour
    {
        private const string AdReviveReadyLabel = "Watch Ad   Revive";
        private const string AdReviveLoadingLabel = "Loading Ad...";
        private const string FreeReviveLabel = "Free Revive";

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button reviveButton;
        [SerializeField] Button adReviveButton;
        [SerializeField] Button exitButton;
        [SerializeField] TMP_Text statsText;

        private TextMeshProUGUI adReviveLabel;
        private Coroutine refreshRoutine;

        private bool upgradeReviveUsed;

        private void Awake()
        {
            reviveButton.onClick.AddListener(ReviveButtonClick);
            if (adReviveButton != null)
            {
                adReviveButton.onClick.AddListener(AdReviveButtonClick);
                adReviveLabel = adReviveButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            exitButton.onClick.AddListener(ExitButtonClick);

            upgradeReviveUsed = false;
        }

        private static bool HasRemoveAds()
        {
            if (GameController.SaveManager == null) return false;
            return GameController.SaveManager.GetSave<EntitlementsSave>("VX Entitlements").RemoveAdsPurchased;
        }

        public void Show()
        {
            gameObject.SetActive(true);

            canvasGroup.alpha = 0;
            canvasGroup.DoAlpha(1, 0.3f).SetUnscaledTime(true);

            if (statsText != null)
            {
                statsText.text = RunResultsFormatter.BuildSummary(false);
            }

            RefreshReviveButtons();
            refreshRoutine = StartCoroutine(RefreshReviveButtonsUntilReady());

            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        private IEnumerator RefreshReviveButtonsUntilReady()
        {
            while (gameObject.activeInHierarchy)
            {
                var session = GameSessionManager.Instance?.RunSession;
                var canBonusRevive = session == null || !session.AdReviveUsed;
                var removeAds = HasRemoveAds();
                var adReady = removeAds || (PlatformServices.AdService != null && PlatformServices.AdService.IsRewardedReady);

                RefreshReviveButtons();

                if (!canBonusRevive || adReady)
                {
                    yield break;
                }

                yield return new WaitForSecondsRealtime(0.5f);
            }
        }

        private void RefreshReviveButtons()
        {
            var session = GameSessionManager.Instance?.RunSession;
            var canUpgradeRevive = GameController.UpgradesManager.IsUpgradeAquired(UpgradeType.Revive)
                                   && !upgradeReviveUsed
                                   && (session == null || !session.UpgradeReviveUsed);

            var canBonusRevive = session == null || !session.AdReviveUsed;
            var removeAds = HasRemoveAds();
            var adReady = removeAds || (PlatformServices.AdService != null && PlatformServices.AdService.IsRewardedReady);

            reviveButton.gameObject.SetActive(canUpgradeRevive);
            if (adReviveButton != null)
            {
                adReviveButton.gameObject.SetActive(canBonusRevive);
                adReviveButton.interactable = adReady;

                if (adReviveLabel != null)
                {
                    if (removeAds)
                    {
                        adReviveLabel.text = FreeReviveLabel;
                    }
                    else
                    {
                        adReviveLabel.text = adReady ? AdReviveReadyLabel : AdReviveLoadingLabel;
                    }
                }
            }

            if (canUpgradeRevive)
            {
                EventSystem.current.SetSelectedGameObject(reviveButton.gameObject);
            }
            else if (adReviveButton != null && canBonusRevive && adReady)
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
            if (refreshRoutine != null)
            {
                StopCoroutine(refreshRoutine);
                refreshRoutine = null;
            }

            canvasGroup.DoAlpha(0, 0.3f).SetUnscaledTime(true).SetOnFinish(() =>
            {
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
            var session = GameSessionManager.Instance?.RunSession;
            if (session != null && session.AdReviveUsed) return;

            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            if (HasRemoveAds())
            {
                session.AdReviveUsed = true;
                Hide(StageController.ResurrectPlayer);
                return;
            }

            if (PlatformServices.AdService == null || !PlatformServices.AdService.IsRewardedReady) return;

            if (adReviveButton != null)
            {
                adReviveButton.interactable = false;
            }

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
                    if (adReviveButton != null && gameObject.activeInHierarchy)
                    {
                        adReviveButton.interactable = PlatformServices.AdService.IsRewardedReady;
                    }
                });
        }

        private void ExitButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            if (refreshRoutine != null)
            {
                StopCoroutine(refreshRoutine);
                refreshRoutine = null;
            }

            GameController.InputManager.onInputChanged -= OnInputChanged;

            void ReturnToMenu()
            {
                Time.timeScale = 1;
                gameObject.SetActive(false);
                StageController.ReturnToMainMenu();
            }

            Time.timeScale = 1;

            if (!PlatformServices.TryShowInterstitial(ReturnToMenu))
            {
                ReturnToMenu();
            }
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
