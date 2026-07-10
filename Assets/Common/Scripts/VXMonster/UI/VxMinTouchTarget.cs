using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VXMonster.UI
{
    /// <summary>
    /// Production Android touch helper: keeps the designer visual size, expands the Button
    /// hit rect toward Material 48dp, and caps expansion so sibling hit boxes do not overlap.
    /// Safe to place on prefab buttons — does not move anchoredPosition.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Button))]
    public class VxMinTouchTarget : MonoBehaviour
    {
        private const string VisualName = "Visual";

        [SerializeField] Vector2 visualSize;
        [SerializeField] bool lockVisualSize = true;

        private RectTransform rect;
        private RectTransform visualRect;
        private Button button;
        private bool applying;

        private void Awake()
        {
            Cache();
            Apply();
        }

        private void OnEnable()
        {
            Cache();
            Apply();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled || applying || !Application.isPlaying) return;
            Apply();
        }

        public void Apply()
        {
            if (applying) return;
            Cache();
            if (rect == null || button == null) return;

            applying = true;
            try
            {
                EnsureVisualHierarchy();

                var minTouch = VxTouchMetrics.MinTouchSize(rect);
                var gap = VxTouchMetrics.MinGap(rect);

                if (visualRect != null && lockVisualSize && visualSize.x > 0f && visualSize.y > 0f)
                {
                    visualRect.sizeDelta = visualSize;
                }

                var targetW = Mathf.Max(visualSize.x, minTouch);
                var targetH = Mathf.Max(visualSize.y, minTouch);
                CapAgainstSiblings(ref targetW, ref targetH, gap);

                // Never shrink below the authored visual.
                targetW = Mathf.Max(targetW, visualSize.x);
                targetH = Mathf.Max(targetH, visualSize.y);

                if (Mathf.Abs(rect.sizeDelta.x - targetW) > 0.1f ||
                    Mathf.Abs(rect.sizeDelta.y - targetH) > 0.1f)
                {
                    rect.sizeDelta = new Vector2(targetW, targetH);
                }
            }
            finally
            {
                applying = false;
            }
        }

        private void Cache()
        {
            rect = transform as RectTransform;
            button = GetComponent<Button>();
            var visual = transform.Find(VisualName);
            visualRect = visual as RectTransform;

            if (visualSize.x < 1f || visualSize.y < 1f)
            {
                if (visualRect != null && visualRect.sizeDelta.x > 1f && visualRect.sizeDelta.y > 1f)
                {
                    visualSize = visualRect.sizeDelta;
                }
                else if (rect != null && rect.sizeDelta.x > 1f && rect.sizeDelta.y > 1f)
                {
                    visualSize = rect.sizeDelta;
                }
            }
        }

        private void EnsureVisualHierarchy()
        {
            var rootImage = GetComponent<Image>();
            var hit = GetComponent<VxInvisibleHitGraphic>();

            if (visualRect == null)
            {
                if (rootImage == null)
                {
                    // Already split or misconfigured — still ensure a hit graphic exists.
                }
                else
                {
                    // First-time split: keep sprite on a child so the parent can grow as hit area only.
                    var visualGo = new GameObject(VisualName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    visualRect = visualGo.GetComponent<RectTransform>();
                    visualRect.SetParent(rect, false);
                    visualRect.SetAsFirstSibling();
                    visualRect.anchorMin = new Vector2(0.5f, 0.5f);
                    visualRect.anchorMax = new Vector2(0.5f, 0.5f);
                    visualRect.pivot = new Vector2(0.5f, 0.5f);
                    visualRect.anchoredPosition = Vector2.zero;
                    if (visualSize.x < 1f || visualSize.y < 1f)
                    {
                        visualSize = rect.sizeDelta;
                    }

                    visualRect.sizeDelta = visualSize;

                    var visualImage = visualGo.GetComponent<Image>();
                    visualImage.sprite = rootImage.sprite;
                    visualImage.type = rootImage.type;
                    visualImage.color = rootImage.color;
                    visualImage.pixelsPerUnitMultiplier = rootImage.pixelsPerUnitMultiplier;
                    visualImage.raycastTarget = false;

                    if (button != null)
                    {
                        button.targetGraphic = visualImage;
                    }

                    // Must remove Image immediately — only one Graphic per GameObject.
                    DestroyImageComponent(rootImage);
                    rootImage = null;
                }
            }
            else
            {
                var visualImage = visualRect.GetComponent<Image>();
                if (visualImage != null)
                {
                    visualImage.raycastTarget = false;
                    if (button != null && (button.targetGraphic == null || button.targetGraphic == hit))
                    {
                        button.targetGraphic = visualImage;
                    }
                }
            }

            // Labels must not steal raycasts from the expanded hit box.
            var labels = GetComponentsInChildren<TMP_Text>(true);
            for (var i = 0; i < labels.Length; i++)
            {
                labels[i].raycastTarget = false;
            }

            // If an Image is still on this object (deferred destroy race), strip it before adding hit graphic.
            rootImage = GetComponent<Image>();
            if (rootImage != null)
            {
                DestroyImageComponent(rootImage);
            }

            hit = GetComponent<VxInvisibleHitGraphic>();
            if (hit == null)
            {
                hit = gameObject.AddComponent<VxInvisibleHitGraphic>();
            }

            if (hit == null)
            {
                Debug.LogError($"[VxMinTouchTarget] Failed to add hit graphic on '{name}'.", this);
                return;
            }

            hit.raycastTarget = true;
            hit.color = new Color(1f, 1f, 1f, 0f);
        }

        private void CapAgainstSiblings(ref float targetW, ref float targetH, float gap)
        {
            if (rect.parent == null) return;

            var myCenter = rect.anchoredPosition;
            for (var i = 0; i < rect.parent.childCount; i++)
            {
                var sibling = rect.parent.GetChild(i) as RectTransform;
                if (sibling == null || sibling == rect) continue;
                if (!sibling.gameObject.activeInHierarchy) continue;
                if (sibling.GetComponent<Button>() == null && sibling.GetComponent<VxMinTouchTarget>() == null)
                {
                    continue;
                }

                var otherCenter = sibling.anchoredPosition;
                var dx = Mathf.Abs(otherCenter.x - myCenter.x);
                var dy = Mathf.Abs(otherCenter.y - myCenter.y);

                // Same row: both sides expand → widths must fit in (dx - gap).
                if (dx > 1f && dy <= Mathf.Max(visualSize.y, 1f))
                {
                    targetW = Mathf.Min(targetW, Mathf.Max(visualSize.x, dx - gap));
                }

                // Same column / stacked: heights must fit in (dy - gap).
                if (dy > 1f && dx <= Mathf.Max(visualSize.x, 1f) * 0.5f)
                {
                    targetH = Mathf.Min(targetH, Mathf.Max(visualSize.y, dy - gap));
                }
            }
        }

        private static void DestroyImageComponent(Image image)
        {
            if (image == null) return;
            // Destroy() is end-of-frame; Graphic swap must be immediate or AddComponent fails.
            Object.DestroyImmediate(image);
        }

        public void SetVisualSize(Vector2 size)
        {
            visualSize = size;
            lockVisualSize = true;
        }
    }
}
