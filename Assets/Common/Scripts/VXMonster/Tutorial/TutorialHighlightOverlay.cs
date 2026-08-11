using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VXMonster.Tutorial
{
    public class TutorialHighlightOverlay : MonoBehaviour
    {
        [SerializeField] RectTransform top;
        [SerializeField] RectTransform bottom;
        [SerializeField] RectTransform left;
        [SerializeField] RectTransform right;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TMP_Text tipText;
        [SerializeField] Button skipButton;

        public Button SkipButton => skipButton;

        public static TutorialHighlightOverlay Create(Transform parent)
        {
            var existing = parent.GetComponentInChildren<TutorialHighlightOverlay>(true);
            if (existing != null) return existing;

            var root = new GameObject("VX In-Run Tutorial Overlay", typeof(RectTransform), typeof(CanvasGroup), typeof(TutorialHighlightOverlay));
            var rt = root.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.SetAsLastSibling();

            var overlay = root.GetComponent<TutorialHighlightOverlay>();
            overlay.canvasGroup = root.GetComponent<CanvasGroup>();
            overlay.tipText = CreateTip(rt);
            overlay.skipButton = CreateSkip(rt);
            overlay.Hide();
            return overlay;
        }

        private static TMP_Text CreateTip(Transform parent)
        {
            var panel = new GameObject("Tip Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.SetParent(parent, false);
            panelRt.anchorMin = new Vector2(0.5f, 1f);
            panelRt.anchorMax = new Vector2(0.5f, 1f);
            panelRt.pivot = new Vector2(0.5f, 1f);
            panelRt.anchoredPosition = new Vector2(0f, -48f);
            panelRt.sizeDelta = new Vector2(920f, 240f);
            var panelImage = panel.GetComponent<Image>();
            TutorialUiAssets.StyleWindow(panelImage);
            panelImage.raycastTarget = false;

            var go = new GameObject("Tip Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(panelRt, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(36f, 24f);
            rect.offsetMax = new Vector2(-36f, -24f);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            TutorialUiFactory.ApplyGameFont(tmp);
            tmp.fontSize = 32f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static Button CreateSkip(Transform parent)
        {
            var go = new GameObject("Skip Tip Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 80f);
            rect.sizeDelta = new Vector2(280f, 72f);
            var image = go.GetComponent<Image>();
            TutorialUiAssets.StyleSkipButton(image);
            go.GetComponent<Button>().targetGraphic = image;
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.SetParent(rect, false);
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;
            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.text = "Skip";
            TutorialUiFactory.ApplyGameFont(tmp);
            tmp.fontSize = 28f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return go.GetComponent<Button>();
        }

        public void BindSkip(UnityEngine.Events.UnityAction onSkip)
        {
            if (skipButton == null) return;
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(onSkip);
            skipButton.interactable = true;
            skipButton.transform.SetAsLastSibling();
        }

        public void Show(string body, RectTransform focusTarget, float padding = 16f)
        {
            if (tipText != null) tipText.text = body;

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            HideScrims();
            if (skipButton != null)
            {
                skipButton.transform.SetAsLastSibling();
                skipButton.interactable = true;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
        }

        private void HideScrims()
        {
            HideScrim(top);
            HideScrim(bottom);
            HideScrim(left);
            HideScrim(right);
        }

        private static void HideScrim(RectTransform rect)
        {
            if (rect == null) return;
            rect.gameObject.SetActive(false);
            var image = rect.GetComponent<Image>();
            if (image != null)
            {
                image.enabled = false;
                image.raycastTarget = false;
            }
        }

        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
        }

    }
}
