using VXMonster.Core.Abilities.UI;
using VXMonster.Core.Audio;
using VXMonster.Core.Easing;
using VXMonster.Core.Input;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VXMonster.Gameplay;
using VXMonster.Tutorial;

namespace VXMonster.Core.UI
{
    public class PauseWindowBehavior : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button continueButton;
        [SerializeField] Button exitButton;
        [SerializeField] List<AbilitiesIndicatorsListBehavior> pauseAbilitiesLists;

        [Header("Settings")]
        [SerializeField] ToggleBehavior soundToggle;
        [SerializeField] ToggleBehavior musicToggle;
        [SerializeField] ToggleBehavior vibrationToggle;
        [SerializeField] Button howToPlayButton;

        public event UnityAction OnStartedClosing;
        public event UnityAction OnClosed;

        private StageSave stageSave;

        private void Awake()
        {
            WireButtons();
        }

        private void OnEnable()
        {
            WireButtons();
        }

        private void WireButtons()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(ContinueButtonClick);
                continueButton.onClick.AddListener(ContinueButtonClick);
            }

            if (exitButton != null)
            {
                exitButton.onClick.RemoveListener(ExitButtonClick);
                exitButton.onClick.AddListener(ExitButtonClick);
            }

            if (howToPlayButton != null)
            {
                howToPlayButton.onClick.RemoveListener(OnHowToPlayClicked);
                howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
            }
        }

        private void Start()
        {
            soundToggle.SetToggle(GameController.AudioManager.SoundVolume != 0);
            musicToggle.SetToggle(GameController.AudioManager.MusicVolume != 0);
            vibrationToggle.SetToggle(GameController.VibrationManager.IsVibrationEnabled);

            soundToggle.onChanged += (soundEnabled) => GameController.AudioManager.SoundVolume = soundEnabled ? 1 : 0;
            musicToggle.onChanged += (musicEnabled) => GameController.AudioManager.MusicVolume = musicEnabled ? 1 : 0;
            vibrationToggle.onChanged += (vibrationEnabled) => GameController.VibrationManager.IsVibrationEnabled = vibrationEnabled;

            stageSave = GameController.SaveManager.GetSave<StageSave>("Stage");
        }

        public void Open()
        {
            gameObject.SetActive(true);

            canvasGroup.alpha = 0f;
            canvasGroup.DoAlpha(1, 0.3f).SetUnscaledTime(true);

            Time.timeScale = 0f;

            for (int i = 0; i < pauseAbilitiesLists.Count; i++)
            {
                var abilityList = pauseAbilitiesLists[i];

                abilityList.Show();
                abilityList.Refresh();
            }

            EasingManager.DoNextFrame(() => {
                EventSystem.current.SetSelectedGameObject(null);
                soundToggle.Select();
                GameController.InputManager.InputAsset.UI.Back.performed += OnBackInputClicked;
            });

            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        public void Close()
        {
            canvasGroup.DoAlpha(0, 0.3f).SetUnscaledTime(true).SetOnFinish(() => {
                gameObject.SetActive(false);
                Time.timeScale = 1f;

                GameController.InputManager.onInputChanged -= OnInputChanged;
                GameController.InputManager.InputAsset.UI.Back.performed -= OnBackInputClicked;

                OnClosed?.Invoke();
            });

            OnStartedClosing?.Invoke();
        }

        private void OnInputChanged(InputType prevInput, InputType inputType)
        {
            if (prevInput == InputType.UIJoystick)
            {
                EasingManager.DoNextFrame(soundToggle.Select);
            }
        }

        private void OnHowToPlayClicked()
        {
            if (GameController.AudioManager != null)
            {
                GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            }

            SetPauseChromeVisible(false);
            TutorialUiFactory.ShowStandaloneGuide(transform, TutorialCopy.HowToPlayTitle, TutorialCopy.HowToPlayBody, RestorePauseChrome);
        }

        private void ContinueButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            Close();
        }

        private void ExitButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            Time.timeScale = 1f;

            if (StageController.IsLoaded)
            {
                stageSave.Time = (float)StageController.Director.time;
            }

            stageSave.IsPlaying = false;
            GameSessionManager.Instance?.ClearSessionContext();

            GameController.SaveManager?.Save(true);
            StageController.ReturnToMainMenu();
        }

        private void OnBackInputClicked(InputAction.CallbackContext context)
        {
            if (TutorialUiFactory.HideStandaloneGuide(transform))
            {
                RestorePauseChrome();
                return;
            }
            continueButton.onClick?.Invoke();
        }

        private void RestorePauseChrome()
        {
            SetPauseChromeVisible(true);
        }

        private void SetPauseChromeVisible(bool visible)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }

        private void OnDestroy()
        {
            GameController.InputManager.onInputChanged -= OnInputChanged;
            GameController.InputManager.InputAsset.UI.Back.performed -= OnBackInputClicked;
        }
    }
}