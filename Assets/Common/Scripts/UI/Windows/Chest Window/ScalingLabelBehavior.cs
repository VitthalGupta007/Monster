using VXMonster.Core.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VXMonster.Core.UI
{
    public class ScalingLabelBehavior : MonoBehaviour
    {
        [SerializeField] protected TMP_Text label;
        [SerializeField] protected Image icon;
        [SerializeField] protected AligmentType aligment;

        protected float spacing;

        protected virtual void Awake()
        {
            spacing = label.rectTransform.anchoredPosition.x - label.rectTransform.sizeDelta.x / 2 - icon.rectTransform.anchoredPosition.x - icon.rectTransform.sizeDelta.x / 2;
        }

        public virtual void SetAmount(int amount)
        {
            label.SetText(amount.ToString());
            RecalculatePositions();
        }

        public virtual void SetIcon(Sprite iconSprite)
        {
            icon.sprite = iconSprite;
        }

        protected virtual void RecalculatePositions()
        {
            label.SetSizeDeltaX(label.preferredWidth);

            var iconWidth = icon.rectTransform.sizeDelta.x;
            var textWidth = label.rectTransform.sizeDelta.x;
            var width = iconWidth + spacing + textWidth;

            switch (aligment)
            {
                case AligmentType.Center:

                    icon.SetAnchoredPositionX(-width / 2f + iconWidth / 2f);
                    label.SetAnchoredPositionX(width / 2f - textWidth / 2f);
                    break;

                case AligmentType.Left:

                    icon.SetAnchoredPositionX(iconWidth / 2f);
                    label.SetAnchoredPositionX(iconWidth + spacing + textWidth / 2f);

                    break;

                case AligmentType.Right:

                    icon.SetAnchoredPositionX(-textWidth - spacing - iconWidth / 2f);
                    label.SetAnchoredPositionX(-textWidth / 2f);

                    break;
            }
        }

        public enum AligmentType
        {
            Left, Center, Right
        }
    }
}