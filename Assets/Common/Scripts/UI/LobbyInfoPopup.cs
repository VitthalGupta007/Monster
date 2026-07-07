using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class LobbyInfoPopup : MonoBehaviour
    {
        [Space]
        [SerializeField] protected RectTransform descriptionRect;
        [SerializeField] protected TMP_Text descriptionText;

        [SerializeField] protected Button infoButton;

        protected float heightDifference;
        protected float rectWidth;

        protected UnityAction onHidden;
        protected Navigation buttonNavigation;

        protected virtual void Awake()
        {
            heightDifference = descriptionRect.sizeDelta.y - descriptionText.preferredHeight;
            rectWidth = descriptionRect.sizeDelta.x;
        }

        public virtual void Show(StageData stage, UnityAction onHidden = null)
        {
            gameObject.SetActive(true);
            UpdateText(stage);

            var rectheight = descriptionText.preferredHeight + heightDifference;

            descriptionRect.sizeDelta = new Vector2(rectWidth, rectheight);

            SubscribeToInputEvents();

            this.onHidden = onHidden;

            buttonNavigation = infoButton.navigation;
            infoButton.navigation = new Navigation() { mode = Navigation.Mode.None };
        }

        protected virtual void SubscribeToInputEvents()
        {
            GameController.InputManager.InputAsset.UI.Back.performed += Hide;
        }

        protected virtual void UnsubscribeFromInputEvents()
        {
            GameController.InputManager.InputAsset.UI.Back.performed -= Hide;
        }

        protected virtual void Hide(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            Hide();
        }

        protected virtual void UpdateText(StageData stage)
        {
            descriptionText.SetText(stage.StageUnlockData.UnlockDescription);
            descriptionText.ForceMeshUpdate();
        }

        public virtual void Hide()
        {
            UnsubscribeFromInputEvents();
            gameObject.SetActive(false);

            infoButton.navigation = buttonNavigation;

            onHidden?.Invoke();
        }
    }
}