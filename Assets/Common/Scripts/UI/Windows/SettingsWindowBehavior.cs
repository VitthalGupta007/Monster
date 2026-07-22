using VXMonster.Core.Easing;
using VXMonster.Core.Input;
#if GOOGLE_MOBILE_ADS_AVAILABLE
using VXMonster.Platform.Ads;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace VXMonster.Core.UI
{
    public class SettingsWindowBehavior : MonoBehaviour
    {
        [SerializeField] ToggleBehavior soundToggle;
        [SerializeField] ToggleBehavior musicToggle;
        [SerializeField] ToggleBehavior vibrationToggle;

        [Space]
        [SerializeField] Button backButton;
        [SerializeField] Button exitButton;
        [SerializeField] Button privacyButton;
        [SerializeField] Button termsButton;
        [SerializeField] LegalTextWindowBehavior legalTextWindow;

        private void Start()
        {
            EasingManager.DoNextFrame().SetOnFinish(InitToggles);
        }

        private void InitToggles()
        {
            soundToggle.SetToggle(GameController.AudioManager.SoundVolume != 0);
            musicToggle.SetToggle(GameController.AudioManager.MusicVolume != 0);
            vibrationToggle.SetToggle(GameController.VibrationManager.IsVibrationEnabled);

            soundToggle.onChanged += (soundEnabled) => GameController.AudioManager.SoundVolume = soundEnabled ? 1 : 0;
            musicToggle.onChanged += (musicEnabled) => GameController.AudioManager.MusicVolume = musicEnabled ? 1 : 0;
            vibrationToggle.onChanged += (vibrationEnabled) => GameController.VibrationManager.IsVibrationEnabled = vibrationEnabled;
        }

        public void Init(UnityAction onBackButtonClicked)
        {
            backButton.onClick.AddListener(onBackButtonClicked);

            if (privacyButton != null && legalTextWindow != null)
            {
                privacyButton.onClick.AddListener(OnPrivacyButtonClicked);
            }

            if (termsButton != null && legalTextWindow != null)
            {
                termsButton.onClick.AddListener(() =>
                {
                    legalTextWindow.Open(LegalTexts.TermsTitle, LegalTexts.TermsBody);
                });
            }

            if (legalTextWindow != null)
            {
                legalTextWindow.Init(CloseLegalWindow);
            }

            if (exitButton != null)
            {
                exitButton.gameObject.SetActive(true);
                exitButton.onClick.AddListener(OnExitButtonClicked);
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
            EasingManager.DoNextFrame(() => {
                soundToggle.Select();
                GameController.InputManager.InputAsset.UI.Back.performed += OnBackInputClicked;
            });
            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        public void Close()
        {
            gameObject.SetActive(false);

            GameController.InputManager.InputAsset.UI.Back.performed -= OnBackInputClicked;
            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        private void OnBackInputClicked(InputAction.CallbackContext context)
        {
            backButton.onClick?.Invoke();
        }

        private void OnInputChanged(InputType prevInput, InputType inputType)
        {
            if (prevInput == InputType.UIJoystick)
            {
                EasingManager.DoNextFrame(soundToggle.Select);
            }
        }

        private void OnExitButtonClicked()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnPrivacyButtonClicked()
        {
#if GOOGLE_MOBILE_ADS_AVAILABLE
            if (GoogleMobileAdsConsentController.PrivacyOptionsRequired)
            {
                GoogleMobileAdsConsentController.ShowPrivacyOptionsForm(message =>
                {
                    if (!string.IsNullOrEmpty(message))
                        Debug.LogWarning($"[UMP] Privacy options: {message}");
                });
                return;
            }
#endif
            legalTextWindow?.Open(LegalTexts.PrivacyPolicyTitle, LegalTexts.PrivacyPolicyBody);
        }

        private void CloseLegalWindow()
        {
            legalTextWindow?.Close();
        }
    }
}