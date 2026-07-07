using VXMonster.Core.Easing;
using VXMonster.Core.Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace VXMonster.Core.UI
{
    public class LegalTextWindowBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text bodyText;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] Button backButton;

        private UnityAction onBack;

        public void Init(UnityAction onBackClicked)
        {
            onBack = onBackClicked;
            backButton.onClick.AddListener(() => onBack?.Invoke());
        }

        public void Open(string title, string body)
        {
            if (bodyText != null)
            {
                bodyText.text = $"<b>{title}</b>\n\n{body}";
            }

            gameObject.SetActive(true);

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }

            EasingManager.DoNextFrame(() =>
            {
                backButton.Select();
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
            onBack?.Invoke();
        }

        private void OnInputChanged(InputType prevInput, InputType inputType)
        {
            if (prevInput == InputType.UIJoystick)
            {
                EasingManager.DoNextFrame(backButton.Select);
            }
        }
    }
}
