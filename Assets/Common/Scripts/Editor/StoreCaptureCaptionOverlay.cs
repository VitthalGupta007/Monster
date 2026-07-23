#if UNITY_EDITOR
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VXMonster.EditorTools
{
    /// <summary>Bakes a top benefit caption into store screenshots during Play Mode capture.</summary>
    public static class StoreCaptureCaptionOverlay
    {
        const float BannerHeightFraction = 0.16f;
        static readonly Color BannerColor = new(0.29f, 0.18f, 0.43f, 0.95f); // ~#4A2D6E

        static GameObject _root;
        static bool _enabled = true;

        public static bool CaptionsEnabled
        {
            get => _enabled;
            set => _enabled = value;
        }

        public static void Show(string caption, int screenWidth, int screenHeight)
        {
            Hide();
            if (!_enabled || string.IsNullOrWhiteSpace(caption)) return;

            // Dedicated overlay canvas — parenting to a random in-scene Canvas often misses Screen Space Overlay / wrong sort order.
            _root = new GameObject("VX Store Caption Overlay");
            Object.DontDestroyOnLoad(_root);

            var canvas = _root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;
            canvas.pixelPerfect = false;

            var scaler = _root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(Mathf.Max(1, screenWidth), Mathf.Max(1, screenHeight));
            scaler.matchWidthOrHeight = screenHeight >= screenWidth ? 1f : 0f;

            _root.AddComponent<GraphicRaycaster>().enabled = false;

            var bannerGo = new GameObject("Banner");
            bannerGo.transform.SetParent(_root.transform, false);
            var rect = bannerGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            var bannerH = Mathf.Max(80f, screenHeight * BannerHeightFraction);
            rect.sizeDelta = new Vector2(0f, bannerH);

            var image = bannerGo.AddComponent<Image>();
            image.color = BannerColor;
            image.raycastTarget = false;

            var textGo = new GameObject("Caption");
            textGo.transform.SetParent(bannerGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 8f);
            textRect.offsetMax = new Vector2(-24f, -8f);

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = caption.ToUpperInvariant();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = screenWidth >= 2000 ? 52f : screenWidth >= 1500 ? 44f : 36f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;

            Canvas.ForceUpdateCanvases();
        }

        public static void Hide()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
                _root = null;
            }
        }
    }
}
#endif
