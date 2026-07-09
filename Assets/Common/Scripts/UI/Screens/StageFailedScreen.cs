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
        private const string AdReviveReadyLabel = "WATCH AD\nREVIVE";
        private const string AdReviveLoadingLabel = "LOADING AD...";
        private const string FreeReviveLabel = "FREE REVIVE";

        // Lower-third stack. Only visible buttons consume a slot — no empty Ad gap.
        private const float ButtonStartY = -120f;
        private const float ButtonStep = 130f;
        private const float ButtonWidth = 400f;
        private const float ButtonHeight = 110f;

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button reviveButton;
        [SerializeField] Button adReviveButton;
        [SerializeField] Button exitButton;
        [SerializeField] Button retryButton;
        [SerializeField] TMP_Text statsText;

        private TextMeshProUGUI adReviveLabel;
        private TextMeshProUGUI retryLabel;
        private Coroutine refreshRoutine;

        private bool upgradeReviveUsed;

        private void Awake()
        {
            reviveButton.onClick.AddListener(ReviveButtonClick);
            if (adReviveButton != null)
            {
                adReviveButton.onClick.AddListener(AdReviveButtonClick);
                adReviveLabel = adReviveButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            exitButton.onClick.AddListener(ExitButtonClick);
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RetryButtonClick);
                retryLabel = retryButton.GetComponentInChildren<TextMeshProUGUI>(true);
            }

            upgradeReviveUsed = false;
            NormalizeButtonLabels();
        }

        private void NormalizeButtonLabels()
        {
            if (retryLabel != null)
            {
                ConfigureButtonLabel(retryLabel, "RETRY", 48f, false);
            }

            if (adReviveLabel != null)
            {
                ConfigureButtonLabel(adReviveLabel, AdReviveReadyLabel, 36f, true);
            }

            var reviveLabel = reviveButton != null
                ? reviveButton.GetComponentInChildren<TextMeshProUGUI>(true)
                : null;
            if (reviveLabel != null)
            {
                ConfigureButtonLabel(reviveLabel, "REVIVE", 48f, false);
            }

            var exitLabel = exitButton != null
                ? exitButton.GetComponentInChildren<TextMeshProUGUI>(true)
                : null;
            if (exitLabel != null)
            {
                ConfigureButtonLabel(exitLabel, "EXIT", 48f, false);
            }
        }

        private static void ConfigureButtonLabel(TextMeshProUGUI label, string text, float fontSize, bool allowWrap)
        {
            label.text = text;
            label.fontSize = fontSize;
            label.enableAutoSizing = false;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = allowWrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.characterSpacing = 0f;
            label.lineSpacing = allowWrap ? -10f : 0f;
            label.raycastTarget = false;
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
                var statsRt = statsText.GetComponent<RectTransform>();
                if (statsRt != null)
                {
                    // Band between YOU DIED and the dead character — not on the hat.
                    statsRt.anchoredPosition = new Vector2(0f, 280f);
                    statsRt.sizeDelta = new Vector2(860f, 80f);
                }

                statsText.fontSize = 22f;
                statsText.enableAutoSizing = false;
                statsText.alignment = TextAlignmentOptions.Center;
                statsText.textWrappingMode = TextWrappingModes.Normal;
                statsText.overflowMode = TextOverflowModes.Overflow;
                statsText.lineSpacing = 0f;
            }

            NormalizeButtonLabels();
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
            var canUpgradeRevive = GameController.UpgradesManager != null
                                   && GameController.UpgradesManager.IsUpgradeAquired(UpgradeType.Revive)
                                   && !upgradeReviveUsed
                                   && (session == null || !session.UpgradeReviveUsed);

            var canBonusRevive = session == null || !session.AdReviveUsed;
            var removeAds = HasRemoveAds();
            var adReady = removeAds || (PlatformServices.AdService != null && PlatformServices.AdService.IsRewardedReady);

            if (reviveButton != null) reviveButton.gameObject.SetActive(canUpgradeRevive);
            if (adReviveButton != null)
            {
                adReviveButton.gameObject.SetActive(canBonusRevive);
                adReviveButton.interactable = adReady;
            }

            if (adReviveLabel != null)
            {
                if (removeAds) adReviveLabel.text = FreeReviveLabel;
                else adReviveLabel.text = adReady ? AdReviveReadyLabel : AdReviveLoadingLabel;
            }

            // Pack only active buttons so hiding Ad Revive collapses the Exit gap.
            var y = ButtonStartY;
            PlaceActiveButton(retryButton, ref y);
            PlaceActiveButton(reviveButton, ref y);
            PlaceActiveButton(adReviveButton, ref y);
            PlaceActiveButton(exitButton, ref y);

            if (canUpgradeRevive)
            {
                EventSystem.current.SetSelectedGameObject(reviveButton.gameObject);
            }
            else if (adReviveButton != null && canBonusRevive && adReady)
            {
                EventSystem.current.SetSelectedGameObject(adReviveButton.gameObject);
            }
            else if (retryButton != null)
            {
                EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
            }
            else if (exitButton != null)
            {
                EventSystem.current.SetSelectedGameObject(exitButton.gameObject);
            }
        }

        private static void PlaceActiveButton(Button button, ref float y)
        {
            if (button == null || !button.gameObject.activeSelf) return;

            var rt = button.GetComponent<RectTransform>();
            if (rt == null) return;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            y -= ButtonStep;
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
                if (session != null) session.AdReviveUsed = true;
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

        private void RetryButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            if (refreshRoutine != null)
            {
                StopCoroutine(refreshRoutine);
                refreshRoutine = null;
            }

            GameController.InputManager.onInputChanged -= OnInputChanged;
            Hide(null);
            RunRetryUtility.RetryCurrentRun();
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
