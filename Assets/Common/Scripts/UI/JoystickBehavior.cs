using VXMonster.Core.Extensions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VXMonster.Core.UI
{
    public class JoystickBehavior : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static JoystickBehavior Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            Instance = null;
        }

        [SerializeField] Canvas canvas;
        [SerializeField] CanvasGroup canvasGroup;

        [Header("Joystick")]
        [SerializeField] Image backgroundImage;
        [SerializeField] Image handleImage;

        [Space]
        [SerializeField] float handleRange = 1;
        [SerializeField] float deadZone = 0;

        private RectTransform baseRectTransform;
        private RectTransform backgroundRectTransform;
        private RectTransform handleRectTransform;

        private bool isBeingUsed;
        public bool IsBeingUsed => isBeingUsed;

        private Camera canvasCamera;

        private Vector2 value = Vector2.zero;
        public Vector2 Value => value;

        private Vector2 defaultAnchoredPosition;

        public event UnityAction OnBeingUsed;

        public bool IsEnabled { get; private set; }

        private void Awake()
        {
            Instance = this;

            baseRectTransform = GetComponent<RectTransform>();
            backgroundRectTransform = backgroundImage.rectTransform;
            handleRectTransform = handleImage.rectTransform;

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                canvasCamera = canvas.worldCamera;

            var center = new Vector2(0.5f, 0.5f);
            backgroundRectTransform.pivot = center;
            handleRectTransform.anchorMin = center;
            handleRectTransform.anchorMax = center;
            handleRectTransform.pivot = center;
            handleRectTransform.anchoredPosition = Vector2.zero;

            isBeingUsed = false;

            defaultAnchoredPosition = backgroundRectTransform.anchoredPosition;

            GameController.InputManager.RegisterJoystick(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRectTransform, eventData.position, canvasCamera, out var localPoint))
            {
                var pivotOffset = baseRectTransform.pivot * baseRectTransform.sizeDelta;
                backgroundRectTransform.anchoredPosition = localPoint - (backgroundRectTransform.anchorMax * baseRectTransform.sizeDelta) + pivotOffset;
            } else
            {
                backgroundRectTransform.anchoredPosition = Vector2.zero;
            }

            isBeingUsed = true;

            OnBeingUsed?.Invoke();

            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isBeingUsed) return;

            isBeingUsed = false;

            ResetJoystick();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isBeingUsed) return;

            var position = RectTransformUtility.WorldToScreenPoint(canvasCamera, backgroundRectTransform.position);
            var radius = backgroundRectTransform.sizeDelta / 2;

            value = (eventData.position - position) / (radius * canvas.scaleFactor);

            if (value.magnitude > deadZone)
            {
                if (value.magnitude > 1) value = value.normalized;
            }
            else
            {
                value = Vector2.zero;
            }

            handleRectTransform.anchoredPosition = value * radius * handleRange;
        }

        public void ResetJoystick()
        {
            isBeingUsed = false;

            backgroundRectTransform.anchoredPosition = defaultAnchoredPosition;

            value = Vector2.zero;
            handleRectTransform.anchoredPosition = Vector2.zero;
        }

        public void Enable()
        {
            IsEnabled = true;
            canvasGroup.alpha = 1;
        }

        public void Disable()
        {
            IsEnabled = false;
            canvasGroup.alpha = 0;

            isBeingUsed = false;

            ResetJoystick();
        }

        private void OnDestroy()
        {
            GameController.InputManager.RemoveJoystick();
        }

        public void Hide()
        {
            backgroundImage.color = backgroundImage.color.SetAlpha(0f);
            handleImage.color = backgroundImage.color.SetAlpha(0f);
        }

        public void Show()
        {
            backgroundImage.SetAlpha(1f);
            handleImage.SetAlpha(1f);
        }
    }
}