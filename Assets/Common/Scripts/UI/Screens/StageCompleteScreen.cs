using VXMonster.Core.Audio;
using VXMonster.Core.Easing;
using VXMonster.Core.Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VXMonster.Platform;
using VXMonster.UI;

namespace VXMonster.Core.UI
{
    public class StageCompleteScreen : MonoBehaviour
    {
        private Canvas canvas;

        private static readonly int STAGE_COMPLETE_HASH = "Stage Complete".GetHashCode();

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button button;
        [SerializeField] TMP_Text statsText;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();

            button.onClick.AddListener(OnButtonClicked);
        }

        public void Show(UnityAction onFinish = null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.DoAlpha(1f, 0.3f).SetUnscaledTime(true).SetOnFinish(onFinish);

            gameObject.SetActive(true);

            if (statsText != null)
            {
                statsText.text = RunResultsFormatter.BuildSummary(true);
            }

            GameController.AudioManager.PlaySound(STAGE_COMPLETE_HASH);

            EventSystem.current.SetSelectedGameObject(button.gameObject);

            GameController.InputManager.onInputChanged += OnInputChanged;
        }

        public void Hide(UnityAction onFinish = null)
        {
            canvasGroup.DoAlpha(0f, 0.3f).SetUnscaledTime(true).SetOnFinish(() => {
                gameObject.SetActive(false);
                onFinish?.Invoke();
            });

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        private void OnButtonClicked()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            Time.timeScale = 1;

            PlatformServices.HideBanner();

            if (!PlatformServices.TryShowInterstitial(() => GameController.LoadMainMenu()))
            {
                GameController.LoadMainMenu();
            }

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        private void OnInputChanged(InputType prevInput, InputType inputType)
        {
            if (prevInput == InputType.UIJoystick)
            {
                EventSystem.current.SetSelectedGameObject(button.gameObject);
            }
        }
    }
}