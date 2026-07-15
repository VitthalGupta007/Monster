using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core.UI;

namespace VXMonster.UI
{
    /// <summary>
    /// Vampire Survivors-style meta access: one Menu entry opens Talent / Codex / Shop.
    /// Keeps those destinations off the stage-select / mode-button band so nothing overlaps.
    /// </summary>
    [DefaultExecutionOrder(110)]
    public class VXLobbyMetaMenu : MonoBehaviour
    {
        private const string MenuButtonName = "VX Menu Button";
        private const string SheetName = "VX Meta Menu Sheet";

        // Top-right: flush to edge. Gold sits left of this (see Main Menu Screen Gold overrides).
        private static readonly Vector2 MenuButtonPos = new Vector2(-16f, -100f);

        [SerializeField] LobbyWindowBehavior lobbyWindow;
        [SerializeField] Sprite buttonSprite;
        [SerializeField] TMP_FontAsset labelFont;

        private GameObject sheetRoot;
        private bool built;

        private void Start()
        {
            Build();
        }

        private void Build()
        {
            if (built) return;

            if (lobbyWindow == null)
            {
                lobbyWindow = FindAnyObjectByType<LobbyWindowBehavior>();
            }

            if (lobbyWindow == null) return;

            var lobbyRect = lobbyWindow.GetComponent<RectTransform>();
            if (lobbyRect == null) return;

            HideSurfaceHubButtons(lobbyRect);
            ResolveVisuals(lobbyRect);

            var menuSize = ResolveMenuButtonSize(lobbyRect);
            var existingMenu = lobbyRect.Find(MenuButtonName) as RectTransform;
            if (existingMenu == null)
            {
                CreateCornerMenuButton(lobbyRect, menuSize);
            }
            else
            {
                // Re-apply production corner layout if an older button already exists.
                existingMenu.anchorMin = new Vector2(1f, 1f);
                existingMenu.anchorMax = new Vector2(1f, 1f);
                existingMenu.pivot = new Vector2(1f, 0.5f);
                existingMenu.anchoredPosition = MenuButtonPos;
                existingMenu.sizeDelta = menuSize;
                existingMenu.SetAsLastSibling();
            }

            if (lobbyRect.Find(SheetName) == null)
            {
                sheetRoot = CreateSheet(lobbyRect);
            }
            else
            {
                sheetRoot = lobbyRect.Find(SheetName).gameObject;
            }

            built = true;
        }

        private void HideSurfaceHubButtons(Transform lobby)
        {
            // Prefab hub buttons were placed on the mode band and collide with VXLobbyModePanel.
            HideChild(lobby, "Talent Button");
            HideChild(lobby, "Codex Button");
            HideChild(lobby, "Shop Button");
        }

        private static void HideChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }

        private void ResolveVisuals(Transform lobby)
        {
            if (buttonSprite == null)
            {
                var play = FindDeepChild(lobby, "Play Button");
                if (play != null && play.TryGetComponent<Image>(out var img))
                {
                    buttonSprite = img.sprite;
                }
            }

            if (labelFont == null)
            {
                var playLabel = FindDeepChild(lobby, "Text (TMP)");
                if (playLabel != null && playLabel.TryGetComponent<TextMeshProUGUI>(out var tmp))
                {
                    labelFont = tmp.font;
                }
            }
        }

        private static Vector2 ResolveMenuButtonSize(RectTransform lobbyRect)
        {
            return new Vector2(160f, 64f);
        }

        private void CreateCornerMenuButton(RectTransform lobbyRect, Vector2 menuSize)
        {
            var go = new GameObject(MenuButtonName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(lobbyRect, false);
            rt.SetAsLastSibling();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = MenuButtonPos;
            rt.sizeDelta = menuSize;

            var image = go.GetComponent<Image>();
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.42f, 0.25f, 0.63f, 1f);
            image.pixelsPerUnitMultiplier = 2f;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(OpenSheet);

            CreateLabel(rt, "MENU", Mathf.Clamp(menuSize.y * 0.35f, 26f, 34f));
        }

        private GameObject CreateSheet(RectTransform lobbyRect)
        {
            var touch = 80f;
            var gap = 16f;
            var rowHeight = 80f;
            var rowWidth = 480f;
            var panelWidth = rowWidth + 80f;

            var root = new GameObject(SheetName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.SetParent(lobbyRect, false);
            rootRt.SetAsLastSibling();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0.05f, 0.03f, 0.1f, 0.92f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.SetParent(rootRt, false);
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(panelWidth, 0f);
            panelRt.anchoredPosition = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.12f, 0.08f, 0.2f, 0.98f);

            var panelLayout = panel.GetComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(40, 40, 36, 36);
            panelLayout.spacing = gap;
            panelLayout.childAlignment = TextAnchor.UpperCenter;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = false;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            // Grow with injected rows (e.g. Leaderboards) so CLOSE stays inside the purple panel.
            var fitter = panel.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateLabel(panelRt, "MENU", 40f, null, new Vector2(rowWidth, Mathf.Max(64f, touch * 0.55f)), useLayout: true);

            CreateSheetRow(panelRt, "Talent Row", "TALENT", rowWidth, rowHeight, () =>
            {
                CloseSheet();
                lobbyWindow.OpenTalentMenu();
            });
            CreateSheetRow(panelRt, "Codex Row", "CODEX", rowWidth, rowHeight, () =>
            {
                CloseSheet();
                lobbyWindow.OpenCodexMenu();
            });
            CreateSheetRow(panelRt, "Shop Row", "SHOP", rowWidth, rowHeight, () =>
            {
                CloseSheet();
                lobbyWindow.OpenShopMenu();
            });
            CreateSheetRow(panelRt, "Close Row", "CLOSE", rowWidth, rowHeight, CloseSheet,
                new Color(0.35f, 0.35f, 0.4f, 1f));

            root.SetActive(false);
            return root;
        }

        private void CreateSheetRow(
            RectTransform parent,
            string name,
            string label,
            float width,
            float height,
            UnityEngine.Events.UnityAction onClick,
            Color? tint = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(width, height);

            var layout = go.GetComponent<LayoutElement>();
            layout.preferredHeight = height;
            layout.minHeight = height;
            layout.flexibleWidth = 1f;

            var image = go.GetComponent<Image>();
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;
            image.color = tint ?? new Color(0.42f, 0.25f, 0.63f, 1f);
            image.pixelsPerUnitMultiplier = 2f;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            CreateLabel(rt, label, Mathf.Clamp(height * 0.32f, 28f, 36f));
        }

        private void CreateLabel(
            RectTransform parent,
            string text,
            float size,
            Vector2? pos = null,
            Vector2? sizeDelta = null,
            bool useLayout = false)
        {
            var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            if (useLayout)
            {
                textGo.AddComponent<LayoutElement>();
            }

            var textRt = textGo.GetComponent<RectTransform>();
            textRt.SetParent(parent, false);
            if (pos.HasValue)
            {
                textRt.anchorMin = new Vector2(0.5f, 0.5f);
                textRt.anchorMax = new Vector2(0.5f, 0.5f);
                textRt.anchoredPosition = pos.Value;
                textRt.sizeDelta = sizeDelta ?? new Vector2(400f, 64f);
            }
            else if (useLayout)
            {
                textRt.sizeDelta = sizeDelta ?? new Vector2(400f, 64f);
                var layout = textGo.GetComponent<LayoutElement>();
                layout.preferredHeight = textRt.sizeDelta.y;
                layout.minHeight = textRt.sizeDelta.y;
                layout.flexibleWidth = 1f;
            }
            else
            {
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;
            }

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            if (labelFont != null) tmp.font = labelFont;
        }

        private void OpenSheet()
        {
            if (sheetRoot == null) return;
            sheetRoot.SetActive(true);
            sheetRoot.transform.SetAsLastSibling();
        }

        private void CloseSheet()
        {
            if (sheetRoot != null) sheetRoot.SetActive(false);
        }

        private static Transform FindDeepChild(Transform parent, string childName)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (child.name == childName) return child;
                var nested = FindDeepChild(child, childName);
                if (nested != null) return nested;
            }

            return null;
        }
    }
}
