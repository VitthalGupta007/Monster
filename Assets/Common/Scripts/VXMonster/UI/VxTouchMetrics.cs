using UnityEngine;
using UnityEngine.UI;

namespace VXMonster.UI
{
    /// <summary>
    /// Converts Android Material touch-target rules (48dp / ~9mm, ≥8dp gaps) into uGUI canvas units.
    /// Primary path uses Screen.dpi + Canvas.scaleFactor (correct across Android densities).
    /// Fallback uses reference-width mapping when DPI is unavailable (some Editor hosts).
    /// </summary>
    public static class VxTouchMetrics
    {
        public const float MaterialMinDp = 48f;
        public const float MaterialGapDp = 8f;
        public const float MediumPhoneWidthDp = 360f;
        public const float FallbackReferenceWidth = 1080f;
        public const float AbsoluteMinCanvasUnits = 96f;

        public static float ReferenceWidth(Canvas canvas)
        {
            if (canvas == null) return FallbackReferenceWidth;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null &&
                scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize &&
                scaler.referenceResolution.x > 1f)
            {
                return scaler.referenceResolution.x;
            }

            var root = canvas.GetComponent<RectTransform>();
            if (root != null && root.rect.width > 1f)
            {
                return root.rect.width;
            }

            return FallbackReferenceWidth;
        }

        public static Canvas RootCanvas(RectTransform anyUnderCanvas)
        {
            if (anyUnderCanvas == null) return null;
            var canvas = anyUnderCanvas.GetComponentInParent<Canvas>();
            return canvas != null ? canvas.rootCanvas : null;
        }

        public static float CanvasWidth(RectTransform anyUnderCanvas)
        {
            var rootCanvas = RootCanvas(anyUnderCanvas);
            if (rootCanvas != null)
            {
                var root = rootCanvas.GetComponent<RectTransform>();
                if (root != null && root.rect.width > 1f)
                {
                    return root.rect.width;
                }

                return ReferenceWidth(rootCanvas);
            }

            return anyUnderCanvas != null && anyUnderCanvas.rect.width > 1f
                ? anyUnderCanvas.rect.width
                : FallbackReferenceWidth;
        }

        /// <summary>
        /// Minimum touch size in canvas units for the live device / canvas.
        /// </summary>
        public static float MinTouchSize(RectTransform anyUnderCanvas)
        {
            var fromFallback = CanvasWidth(anyUnderCanvas) * (MaterialMinDp / MediumPhoneWidthDp);
            var fromDpi = MinTouchSizeFromDpi(anyUnderCanvas);
            var value = fromDpi > 0f ? fromDpi : fromFallback;
            return Mathf.Max(AbsoluteMinCanvasUnits, value);
        }

        public static float MinGap(RectTransform anyUnderCanvas)
        {
            var fromFallback = CanvasWidth(anyUnderCanvas) * (MaterialGapDp / MediumPhoneWidthDp);
            var fromDpi = MinGapFromDpi(anyUnderCanvas);
            var value = fromDpi > 0f ? fromDpi : fromFallback;
            return Mathf.Max(8f, value);
        }

        /// <summary>
        /// 48dp → pixels via density (dpi/160), then → canvas units via scaleFactor.
        /// Returns 0 when DPI/scale cannot be read.
        /// </summary>
        public static float MinTouchSizeFromDpi(RectTransform anyUnderCanvas)
        {
            var rootCanvas = RootCanvas(anyUnderCanvas);
            if (rootCanvas == null) return 0f;
            if (Screen.dpi <= 1f) return 0f;
            if (rootCanvas.scaleFactor <= 0.0001f) return 0f;

            var pixels = MaterialMinDp * (Screen.dpi / 160f);
            return pixels / rootCanvas.scaleFactor;
        }

        public static float MinGapFromDpi(RectTransform anyUnderCanvas)
        {
            var rootCanvas = RootCanvas(anyUnderCanvas);
            if (rootCanvas == null) return 0f;
            if (Screen.dpi <= 1f) return 0f;
            if (rootCanvas.scaleFactor <= 0.0001f) return 0f;

            var pixels = MaterialGapDp * (Screen.dpi / 160f);
            return pixels / rootCanvas.scaleFactor;
        }

        public static float FitTouchHeight(RectTransform anyUnderCanvas, float availableForOneRow)
        {
            var ideal = MinTouchSize(anyUnderCanvas);
            if (availableForOneRow >= ideal) return ideal;
            return Mathf.Clamp(availableForOneRow, AbsoluteMinCanvasUnits, ideal);
        }
    }
}
