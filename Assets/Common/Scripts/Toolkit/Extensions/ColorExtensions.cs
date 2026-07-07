using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace OctoberStudio.Extensions
{
    public static class ColorExtensions
    {
        public static Color SetAlpha(this Color color, float aValue)
        {
            color.a = aValue;

            return color;
        }

        public static Graphic SetAlpha(this Graphic graphic, float a)
        {
            graphic.color = graphic.color.SetAlpha(a);

            return graphic;
        }

        public static void SetBorderColor(this IStyle style, Color color)
        {
            style.borderBottomColor = color;
            style.borderLeftColor = color;
            style.borderRightColor = color;
            style.borderTopColor = color;
        }
    }
}