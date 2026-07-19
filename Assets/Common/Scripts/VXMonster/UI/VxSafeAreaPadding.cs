using UnityEngine;

namespace VXMonster.UI
{
    /// <summary>
    /// Applies Screen.safeArea as offsetMin/offsetMax on a stretch RectTransform.
    /// Caches offsets to avoid layout feedback loops when dimensions change.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class VxSafeAreaPadding : MonoBehaviour
    {
        RectTransform rect;
        Rect lastSafeArea;
        Vector2Int lastScreenSize;
        Vector2 lastOffsetMin;
        Vector2 lastOffsetMax;

        private void OnEnable()
        {
            rect = transform as RectTransform;
            Apply(force: true);
        }

        private void Update()
        {
            if (Screen.safeArea != lastSafeArea || Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
            {
                Apply(force: true);
            }
        }

        public void Apply(bool force = false)
        {
            if (rect == null) rect = transform as RectTransform;
            if (rect == null) return;

            var area = Screen.safeArea;
            var min = new Vector2(area.xMin, area.yMin);
            var max = new Vector2(area.xMax - Screen.width, area.yMax - Screen.height);

            if (!force && area == lastSafeArea && lastScreenSize.x == Screen.width && lastScreenSize.y == Screen.height
                && rect.offsetMin == lastOffsetMin && rect.offsetMax == lastOffsetMax)
            {
                return;
            }

            lastSafeArea = area;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            lastOffsetMin = min;
            lastOffsetMax = max;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = min;
            rect.offsetMax = max;
        }
    }
}
