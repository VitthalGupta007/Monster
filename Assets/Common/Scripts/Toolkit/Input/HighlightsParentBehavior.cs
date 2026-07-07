using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using OctoberStudio.UI;
using UnityEngine;

namespace OctoberStudio.Input
{
    public class HighlightsParentBehavior : MonoBehaviour
    {
        private PoolComponent<RectTransform> buttonArrowPool;

        private RectTransform rightArrow;
        private RectTransform leftArrow;

        private HighlightableButtonUI highlightedButton;
        [SerializeField] GameObject buttonSelectionArrowPrefab;

        private void Awake()
        {
            buttonArrowPool = new PoolComponent<RectTransform>(buttonSelectionArrowPrefab, 2, transform, true);
        }

        public void EnableArrows()
        {
            if (highlightedButton != null)
            {
                leftArrow.localScale = Vector3.one;
                rightArrow.localScale = new Vector3(-1, 1, 1);
            }
        }

        public void DisableArrows()
        {
            if (highlightedButton != null)
            {
                leftArrow.localScale = Vector3.zero;
                rightArrow.localScale = Vector3.zero;
            }
        }

        private RectTransform GetButtonArrow()
        {
            return buttonArrowPool.GetEntity();
        }

        public void Highlight(HighlightableButtonUI button)
        {
            if (highlightedButton != null)
            {
                StopHighlighting(highlightedButton);
            }

            highlightedButton = button;

            leftArrow = GetButtonArrow();
            rightArrow = GetButtonArrow();

            leftArrow.SetParent(button.transform);
            rightArrow.SetParent(button.transform);

            leftArrow.ResetLocal();
            rightArrow.ResetLocal();

            if (GameController.InputManager.ActiveInput != InputType.UIJoystick)
            {
                leftArrow.localScale = Vector3.one;
                rightArrow.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                leftArrow.localScale = Vector3.zero;
                rightArrow.localScale = Vector3.zero;
            }

            leftArrow.anchorMin = new Vector2(0, 0.5f);
            leftArrow.anchorMax = new Vector2(0, 0.5f);

            rightArrow.anchorMin = new Vector2(1, 0.5f);
            rightArrow.anchorMax = new Vector2(1, 0.5f);

            leftArrow.anchoredPosition = Vector2.zero;
            rightArrow.anchoredPosition = Vector2.zero;

            leftArrow.SetParent(transform);
            rightArrow.SetParent(transform);

            button.IsHighlighted = true;
        }

        public void RefreshHighlight()
        {
            leftArrow.SetParent(highlightedButton.transform);
            rightArrow.SetParent(highlightedButton.transform);

            leftArrow.ResetLocal();
            rightArrow.ResetLocal();

            if (GameController.InputManager.ActiveInput != InputType.UIJoystick)
            {
                leftArrow.localScale = Vector3.one;
                rightArrow.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                leftArrow.localScale = Vector3.zero;
                rightArrow.localScale = Vector3.zero;
            }

            leftArrow.anchorMin = new Vector2(0, 0.5f);
            leftArrow.anchorMax = new Vector2(0, 0.5f);

            rightArrow.anchorMin = new Vector2(1, 0.5f);
            rightArrow.anchorMax = new Vector2(1, 0.5f);

            leftArrow.anchoredPosition = Vector2.zero;
            rightArrow.anchoredPosition = Vector2.zero;

            leftArrow.SetParent(transform);
            rightArrow.SetParent(transform);
        }

        public void StopHighlighting(HighlightableButtonUI button)
        {
            if (rightArrow != null) rightArrow.gameObject.SetActive(false);
            if (rightArrow != null) leftArrow.gameObject.SetActive(false);

            rightArrow = null;
            leftArrow = null;

            button.IsHighlighted = false;
            highlightedButton = null;
        }
    }
}