using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class TextIndicatorBehavior : MonoBehaviour
    {
        [SerializeField] protected RectTransform rectTransform;
        [SerializeField] protected TMP_Text textComponent;

        public void SetText(string text)
        {
            textComponent.text = text;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAnchors(Vector2 viewportPosition)
        {
            rectTransform.anchorMin = viewportPosition;
            rectTransform.anchorMax = viewportPosition;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(Vector2 position)
        {
            rectTransform.anchoredPosition = position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetScale(Vector3 scale)
        {
            rectTransform.localScale = scale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAnimationParameters(float4 parameters)
        {
            rectTransform.localScale = new Vector3(parameters.x, parameters.x, parameters.x);

            rectTransform.anchoredPosition = new Vector2(0, parameters.y);

            var anchor = parameters.zw;

            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
        }
    }
}