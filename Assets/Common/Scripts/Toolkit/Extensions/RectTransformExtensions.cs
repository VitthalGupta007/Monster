using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.Extensions
{
    public static class RectTransformExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectTransform SetAnchoredPositionX(this Graphic graphic, float x)
        {
            return graphic.rectTransform.SetAnchoredPositionX(x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectTransform SetAnchoredPositionY(this Graphic graphic, float y)
        {
            return graphic.rectTransform.SetAnchoredPositionY(y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectTransform SetAnchoredPositionX(this RectTransform rectTransform, float x)
        {
            rectTransform.anchoredPosition = rectTransform.anchoredPosition.SetX(x);
            return rectTransform;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectTransform SetAnchoredPositionY(this RectTransform rectTransform, float y)
        {
            rectTransform.anchoredPosition = rectTransform.anchoredPosition.SetY(y);
            return rectTransform;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectTransform SetSizeDeltaX(this RectTransform rectTransform, float x)
        {
            rectTransform.sizeDelta = rectTransform.sizeDelta.SetX(x);
            return rectTransform;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Graphic SetSizeDeltaX(this Graphic graphic, float x)
        {
            graphic.rectTransform.sizeDelta = graphic.rectTransform.sizeDelta.SetX(x);
            return graphic;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectTransform SetSizeDeltaY(this RectTransform rectTransform, float y)
        {
            rectTransform.sizeDelta = rectTransform.sizeDelta.SetY(y);
            return rectTransform;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Graphic SetSizeDeltaY(this Graphic graphic, float y)
        {
            graphic.rectTransform.sizeDelta = graphic.rectTransform.sizeDelta.SetY(y);
            return graphic;
        }
    }
}