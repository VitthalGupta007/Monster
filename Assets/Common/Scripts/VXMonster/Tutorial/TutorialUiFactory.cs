using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace VXMonster.Tutorial
{
    public static class TutorialUiFactory
    {
        public const string StandaloneGuideName = "VX How To Play Overlay";

        public static void ApplyGameFont(TMP_Text tmp)
        {
            if (tmp == null || tmp.font != null) return;
            var font = TutorialUiAssets.GameFont;
            if (font != null) tmp.font = font;
        }

        public static void ShowStandaloneGuide(Transform parent, string title, string body, UnityAction onClosed)
        {
            var canvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;
            var host = canvas != null ? canvas.transform : parent;

            var existing = host != null ? host.Find(StandaloneGuideName) : null;
            if (existing == null && parent != null) existing = parent.Find(StandaloneGuideName);

            GameObject root;
            if (existing != null)
            {
                root = existing.gameObject;
                if (host != null && root.transform.parent != host)
                {
                    root.transform.SetParent(host, false);
                }

                EnsureGuideLayout(root.transform);
            }
            else
            {
                root = CreateGuideRoot(host);
            }

            var bodyLabel = root.transform.Find("Card/Body")?.GetComponent<TMP_Text>();
            if (bodyLabel != null)
            {
                bodyLabel.font = TutorialUiAssets.GameFont != null ? TutorialUiAssets.GameFont : TutorialUiAssets.PlainFont;
                bodyLabel.text = string.IsNullOrEmpty(title)
                    ? (body ?? string.Empty)
                    : $"<size=48><b>{title}</b></size>\n\n{body}";
                bodyLabel.ForceMeshUpdate();
            }

            var back = root.transform.Find("Card/How To Play Back")?.GetComponent<Button>();
            if (back != null)
            {
                back.onClick.RemoveAllListeners();
                back.onClick.AddListener(() =>
                {
                    root.SetActive(false);
                    onClosed?.Invoke();
                });
            }

            var overlayCanvas = root.GetComponent<Canvas>();
            if (overlayCanvas == null) overlayCanvas = root.AddComponent<Canvas>();
            overlayCanvas.overrideSorting = true;
            overlayCanvas.sortingOrder = 500;
            if (root.GetComponent<GraphicRaycaster>() == null)
            {
                root.AddComponent<GraphicRaycaster>();
            }

            root.SetActive(true);
            root.transform.SetAsLastSibling();
        }

        public static bool HideStandaloneGuide(Transform parent)
        {
            var canvas = parent != null ? parent.GetComponentInParent<Canvas>() : null;
            var host = canvas != null ? canvas.transform : parent;
            var existing = host != null ? host.Find(StandaloneGuideName) : null;
            if (existing == null && parent != null) existing = parent.Find(StandaloneGuideName);
            if (existing == null || !existing.gameObject.activeSelf) return false;
            existing.gameObject.SetActive(false);
            return true;
        }

        static GameObject CreateGuideRoot(Transform host)
        {
            var root = new GameObject(StandaloneGuideName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = root.GetComponent<RectTransform>();
            rt.SetParent(host, false);
            StretchFull(rt);
            var dimmer = root.GetComponent<Image>();
            TutorialUiAssets.StyleDimmer(dimmer);
            dimmer.color = new Color(0.07f, 0.05f, 0.12f, 0.86f);
            dimmer.raycastTarget = true;
            EnsureGuideLayout(rt);
            return root;
        }

        static void EnsureGuideLayout(Transform root)
        {
            StretchFull(root as RectTransform);

            var dimmer = root.GetComponent<Image>();
            if (dimmer != null)
            {
                TutorialUiAssets.StyleDimmer(dimmer);
                dimmer.color = new Color(0.07f, 0.05f, 0.12f, 0.86f);
                dimmer.raycastTarget = true;
            }

            var card = root.Find("Card") as RectTransform;
            if (card == null)
            {
                var cardGo = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                card = cardGo.GetComponent<RectTransform>();
                card.SetParent(root, false);
            }

            card.anchorMin = card.anchorMax = card.pivot = new Vector2(0.5f, 0.5f);
            card.anchoredPosition = Vector2.zero;
            card.sizeDelta = new Vector2(920f, 1680f);
            var cardImage = card.GetComponent<Image>();
            TutorialUiAssets.StyleWindow(cardImage);
            cardImage.raycastTarget = true;

            var legacyBody = root.Find("Body");
            if (legacyBody != null) Object.Destroy(legacyBody.gameObject);
            var legacyBack = root.Find("How To Play Back");
            if (legacyBack != null) Object.Destroy(legacyBack.gameObject);

            var body = card.Find("Body") as RectTransform;
            if (body == null)
            {
                var bodyGo = new GameObject("Body", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                body = bodyGo.GetComponent<RectTransform>();
                body.SetParent(card, false);
            }

            body.anchorMin = new Vector2(0.07f, 0f);
            body.anchorMax = new Vector2(0.93f, 1f);
            body.pivot = new Vector2(0.5f, 0.5f);
            body.offsetMin = new Vector2(0f, 360f);
            body.offsetMax = new Vector2(0f, -64f);
            var tmp = body.GetComponent<TextMeshProUGUI>();
            tmp.font = TutorialUiAssets.GameFont != null ? TutorialUiAssets.GameFont : TutorialUiAssets.PlainFont;
            tmp.fontSize = 36f;
            tmp.lineSpacing = 8f;
            tmp.paragraphSpacing = 2f;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
            tmp.richText = true;
            tmp.raycastTarget = false;
            tmp.overflowMode = TextOverflowModes.Truncate;
            tmp.margin = new Vector4(36f, 20f, 36f, 12f);

            var back = card.Find("How To Play Back") as RectTransform;
            if (back == null)
            {
                CreateFallbackButton(card, "How To Play Back", "BACK", new Vector2(0f, 120f));
                back = card.Find("How To Play Back") as RectTransform;
            }

            if (back != null)
            {
                back.anchorMin = back.anchorMax = back.pivot = new Vector2(0.5f, 0f);
                back.anchoredPosition = new Vector2(0f, 120f);
                back.sizeDelta = new Vector2(560f, 180f);
                var backLabel = back.GetComponentInChildren<TextMeshProUGUI>(true);
                if (backLabel != null)
                {
                    ApplyGameFont(backLabel);
                    backLabel.enableAutoSizing = false;
                    backLabel.fontSize = 62f;
                    backLabel.fontSizeMax = 62f;
                }
            }
        }

        static void StretchFull(RectTransform rt)
        {
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        static Button CreateFallbackButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(560f, 180f);
            var image = go.GetComponent<Image>();
            TutorialUiAssets.StyleConfirmButton(image);
            go.GetComponent<Button>().targetGraphic = image;

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.SetParent(rt, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            ApplyGameFont(tmp);
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 40f;
            tmp.fontSizeMax = 64f;
            tmp.fontSize = 56f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return go.GetComponent<Button>();
        }
    }
}
