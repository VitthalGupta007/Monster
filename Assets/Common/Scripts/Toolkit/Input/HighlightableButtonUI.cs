using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VXMonster.Core.UI
{
    [RequireComponent(typeof(Button))]
    public class HighlightableButtonUI : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public bool IsHighlighted { get; set; }

        protected Button button;

        private Vector3 savedPosition = Vector3.zero;

        protected virtual void Awake()
        {
            button = GetComponent<Button>();
        }

        private void Update()
        {
            if (IsHighlighted)
            {
                if(transform.position != savedPosition)
                {
                    savedPosition = transform.position;

                    GameController.InputManager.Highlights.RefreshHighlight();
                }
            }
        }

        public virtual void Highlight()
        {
            if (button.enabled) GameController.InputManager.Highlights.Highlight(this);

            savedPosition = transform.position;
        }

        public virtual void StopHighlighting()
        {
            if (IsHighlighted) GameController.InputManager.Highlights.StopHighlighting(this);
        }

        private void OnDisable()
        {
            if (IsHighlighted)
            {
                GameController.InputManager.Highlights.StopHighlighting(this);
            }
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            Highlight();
        }

        public virtual void OnDeselect(BaseEventData eventData)
        {
            StopHighlighting();
        }
    }
}