using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Input;
using OctoberStudio.Upgrades;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class StageFailedScreen : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Button reviveButton;
        [SerializeField] Button exitButton;

        private Canvas canvas;

        private bool revivedAlready = false;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();

            reviveButton.onClick.AddListener(ReviveButtonClick);
            exitButton.onClick.AddListener(ExitButtonClick);

            revivedAlready = false;
        }

        public void Show()
        {
            gameObject.SetActive(true);

            canvasGroup.alpha = 0;
            canvasGroup.DoAlpha(1, 0.3f).SetUnscaledTime(true);

            if (GameController.UpgradesManager.IsUpgradeAquired(UpgradeType.Revive) && !revivedAlready)
            {
                reviveButton.gameObject.SetActive(true);

                EventSystem.current.SetSelectedGameObject(reviveButton.gameObject);
            } else
            {
                reviveButton.gameObject.SetActive(false);
                EventSystem.current.SetSelectedGameObject(exitButton.gameObject);
            }

            GameController.InputManager.onInputChanged += OnInputChanged;
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
            Hide(StageController.ResurrectPlayer);
            revivedAlready = true;
        }

        private void ExitButtonClick()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            Time.timeScale = 1;
            StageController.ReturnToMainMenu();

            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        private void OnInputChanged(InputType prevInput, InputType inputType)
        {
            if (prevInput == InputType.UIJoystick)
            {
                if (GameController.UpgradesManager.IsUpgradeAquired(UpgradeType.Revive) && !revivedAlready)
                {
                    EventSystem.current.SetSelectedGameObject(reviveButton.gameObject);
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(exitButton.gameObject);
                }
            }
        }
    }
}