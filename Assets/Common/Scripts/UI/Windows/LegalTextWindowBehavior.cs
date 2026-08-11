using VXMonster.Core.Easing;
using VXMonster.Core.Input;
using VXMonster.Tutorial;
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

        public bool IsOpen => gameObject.activeSelf;

        private void Awake()
        {
            WireBackButton();
        }

        private void OnEnable()
        {
            WireBackButton();
        }

        public void Init(UnityAction onBackClicked)
        {
            onBack = onBackClicked;
            WireBackButton();
        }

        private void WireBackButton()
        {
            if (backButton == null) return;
            backButton.onClick.RemoveListener(HandleBackClicked);
            backButton.onClick.AddListener(HandleBackClicked);
        }

        public void Open(string title, string body)
        {
            WireBackButton();
            PrepareVisuals();

            if (bodyText != null)
            {
                bodyText.text = string.IsNullOrEmpty(title)
                    ? (body ?? string.Empty)
                    : $"<b>{title}</b>\n\n{body}";
                bodyText.ForceMeshUpdate();
            }

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (backButton != null)
            {
                backButton.transform.SetAsLastSibling();
            }

            RebuildScrollContent();
            Canvas.ForceUpdateCanvases();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }

            if (backButton != null) backButton.Select();
            if (GameController.InputManager != null)
            {
                GameController.InputManager.InputAsset.UI.Back.performed -= OnBackInputClicked;
                GameController.InputManager.InputAsset.UI.Back.performed += OnBackInputClicked;
                GameController.InputManager.onInputChanged -= OnInputChanged;
                GameController.InputManager.onInputChanged += OnInputChanged;
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
            if (GameController.InputManager == null) return;
            GameController.InputManager.InputAsset.UI.Back.performed -= OnBackInputClicked;
            GameController.InputManager.onInputChanged -= OnInputChanged;
        }

        private void HandleBackClicked()
        {
            Close();
            onBack?.Invoke();
        }

        private void OnBackInputClicked(InputAction.CallbackContext context)
        {
            HandleBackClicked();
        }

        private void OnInputChanged(InputType prevInput, InputType inputType)
        {
            if (prevInput == InputType.UIJoystick && backButton != null)
            {
                EasingManager.DoNextFrame(backButton.Select);
            }
        }

        private void PrepareVisuals()
        {
            if (backButton != null)
            {
                backButton.interactable = true;
                backButton.enabled = true;
            }

            var dimmer = GetComponent<Image>();
            if (dimmer != null)
            {
                TutorialUiAssets.StyleDimmer(dimmer);
                dimmer.color = new Color(0.07f, 0.05f, 0.12f, 0.72f);
            }

            var card = transform.Find("Card");
            if (card != null)
            {
                TutorialUiAssets.StyleWindow(card.GetComponent<Image>());
            }

            ClearBlockingImage(transform.Find("Legal Body"));
            if (scrollRect != null)
            {
                if (scrollRect.viewport != null)
                {
                    var viewport = scrollRect.viewport;
                    var viewportImage = viewport.GetComponent<Image>();
                    if (viewportImage != null)
                    {
                        if (viewportImage.sprite == null)
                        {
                            viewportImage.sprite = TutorialUiAssets.DimSprite;
                        }
                        viewportImage.color = new Color(1f, 1f, 1f, 0f);
                        viewportImage.raycastTarget = true;
                    }

                    var mask = viewport.GetComponent<Mask>();
                    if (mask != null) mask.enabled = false;
                    if (viewport.GetComponent<RectMask2D>() == null)
                    {
                        viewport.gameObject.AddComponent<RectMask2D>();
                    }
                }
                if (scrollRect.content != null)
                {
                    var fitter = scrollRect.content.GetComponent<ContentSizeFitter>();
                    if (fitter != null) fitter.enabled = false;
                }
            }

            if (bodyText == null) return;

            bodyText.raycastTarget = false;
            bodyText.richText = true;
            bodyText.enableWordWrapping = true;
            bodyText.overflowMode = TextOverflowModes.Overflow;
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            bodyText.margin = new Vector4(24f, 16f, 24f, 16f);
            bodyText.lineSpacing = 8f;
            bodyText.color = Color.white;
            if (TutorialUiAssets.PlainFont != null)
            {
                bodyText.font = TutorialUiAssets.PlainFont;
            }
        }

        private void RebuildScrollContent()
        {
            if (bodyText == null || scrollRect == null || scrollRect.content == null) return;

            bodyText.ForceMeshUpdate();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            var height = Mathf.Max(bodyText.preferredHeight + 32f, scrollRect.viewport != null ? scrollRect.viewport.rect.height : 0f);
            var size = scrollRect.content.sizeDelta;
            size.y = height;
            scrollRect.content.sizeDelta = size;
        }

        private static void ClearBlockingImage(Transform target)
        {
            if (target == null) return;
            var image = target.GetComponent<Image>();
            if (image == null) return;
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = false;
        }
    }
}
