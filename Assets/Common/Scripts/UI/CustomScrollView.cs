using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OctoberStudio.UI
{
    public class CustomScrollView : ScrollRect, IPointerEnterHandler, IPointerExitHandler
    {
        private bool swallowMouseWheelScrolls = true;
        private bool isMouseOver = false;

        private bool gamepad;

        public void OnPointerEnter(PointerEventData eventData)
        {
            isMouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isMouseOver = false;
        }

        private void Update()
        {
            // Detect the mouse wheel and generate a scroll. This fixes the issue where Unity will prevent our ScrollRect
            // from receiving any mouse wheel messages if the mouse is over a raycast target (such as a button).
            if (isMouseOver)
            {
                if (IsMouseWheelRolling())
                {
                    var delta = Mouse.current.scroll.value.y;

                    PointerEventData pointerData = new PointerEventData(EventSystem.current);
                    pointerData.scrollDelta = new Vector2(0f, delta);

                    gamepad = false;

                    swallowMouseWheelScrolls = false;
                    OnScroll(pointerData);
                    swallowMouseWheelScrolls = true;
                }

                if (IsGamepadScrolling())
                {
                    var delta = Gamepad.current.rightStick.value.y;

                    PointerEventData pointerData = new PointerEventData(EventSystem.current);
                    pointerData.scrollDelta = new Vector2(0f, delta);

                    gamepad = true;

                    swallowMouseWheelScrolls = false;
                    OnScroll(pointerData);
                    swallowMouseWheelScrolls = true;
                }
            }
        }

        public override void OnScroll(PointerEventData data)
        {
            if (IsMouseWheelRolling() && swallowMouseWheelScrolls)
            {
                // Eat the scroll so that we don't get a double scroll when the mouse is over an image
            }
            else
            {
                // Amplify the mousewheel so that it matches the scroll sensitivity.
                if (data.scrollDelta.y < -Mathf.Epsilon)
                    data.scrollDelta = new Vector2(0f, -scrollSensitivity);
                else if (data.scrollDelta.y > Mathf.Epsilon)
                    data.scrollDelta = new Vector2(0f, scrollSensitivity);

                if (gamepad) data.scrollDelta /= 5f;

                base.OnScroll(data);
            }
        }

        private static bool IsMouseWheelRolling()
        {
            return Mouse.current != null && Mouse.current.scroll.value.y != 0;
        }

        private static bool IsGamepadScrolling()
        {
            return Gamepad.current != null && Gamepad.current.rightStick.value.y != 0;
        }
    }
}