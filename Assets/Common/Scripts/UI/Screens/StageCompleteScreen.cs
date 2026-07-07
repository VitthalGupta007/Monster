using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Input;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class StageCompleteScreen : MonoBehaviour
    {
        private Canvas canvas;

        private static readonly int STAGE_COMPLETE_HASH = "Stage Complete".GetHashCode();

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button button;

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
            GameController.LoadMainMenu();

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