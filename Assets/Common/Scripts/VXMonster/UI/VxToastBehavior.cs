using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Tutorial;

namespace VXMonster.UI
{
    /// <summary>
    /// Short auto-dismiss toast using the same window sprite / font as tutorial tips.
    /// </summary>
    public class VxToastBehavior : MonoBehaviour
    {
        private static VxToastBehavior instance;

        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TMP_Text bodyText;

        private Coroutine hideRoutine;

        public static void Show(string message, float duration = 2.4f)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            var toast = EnsureInstance();
            if (toast == null) return;
            toast.ShowInternal(message, duration);
        }

        private static VxToastBehavior EnsureInstance()
        {
            if (instance != null) return instance;

            var canvas = Object.FindAnyObjectByType<Canvas>();
            if (canvas == null) return null;

            var root = new GameObject("VX Toast", typeof(RectTransform), typeof(CanvasGroup), typeof(VxToastBehavior));
            var rt = root.GetComponent<RectTransform>();
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -36f);
            rt.sizeDelta = new Vector2(880f, 160f);
            rt.SetAsLastSibling();

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.SetParent(rt, false);
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;
            TutorialUiAssets.StyleWindow(panel.GetComponent<Image>());
            panel.GetComponent<Image>().raycastTarget = false;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.SetParent(panelRt, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(28f, 16f);
            textRt.offsetMax = new Vector2(-28f, -16f);
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            TutorialUiFactory.ApplyGameFont(tmp);
            tmp.fontSize = 28f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;

            instance = root.GetComponent<VxToastBehavior>();
            instance.canvasGroup = root.GetComponent<CanvasGroup>();
            instance.bodyText = tmp;
            instance.HideImmediate();
            return instance;
        }

        private void ShowInternal(string message, float duration)
        {
            if (bodyText != null) bodyText.text = message;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            if (hideRoutine != null) StopCoroutine(hideRoutine);
            hideRoutine = StartCoroutine(HideAfter(duration));
        }

        private IEnumerator HideAfter(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            HideImmediate();
        }

        private void HideImmediate()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
