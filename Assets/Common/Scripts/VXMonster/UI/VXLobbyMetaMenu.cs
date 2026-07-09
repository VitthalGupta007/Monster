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

        // Top-right corner (Settings gear is top-left at ~100,-100).
        private static readonly Vector2 MenuButtonPos = new Vector2(-100f, -100f);
        private static readonly Vector2 MenuButtonSize = new Vector2(160f, 64f);

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

            if (lobbyRect.Find(MenuButtonName) == null)
            {
                CreateCornerMenuButton(lobbyRect);
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

        private void CreateCornerMenuButton(RectTransform lobbyRect)
        {
            var go = new GameObject(MenuButtonName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(lobbyRect, false);
            rt.SetAsLastSibling();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = MenuButtonPos;
            rt.sizeDelta = MenuButtonSize;

            var image = go.GetComponent<Image>();
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.42f, 0.25f, 0.63f, 1f);
            image.pixelsPerUnitMultiplier = 2f;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(OpenSheet);

            CreateLabel(rt, "MENU", 28f);
        }

        private GameObject CreateSheet(RectTransform lobbyRect)
        {
            var root = new GameObject(SheetName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.SetParent(lobbyRect, false);
            rootRt.SetAsLastSibling();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;
            root.GetComponent<Image>().color = new Color(0.05f, 0.03f, 0.1f, 0.92f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.SetParent(rootRt, false);
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(560f, 720f);
            panelRt.anchoredPosition = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0.12f, 0.08f, 0.2f, 0.98f);

            CreateLabel(panelRt, "MENU", 40f, new Vector2(0f, 280f), new Vector2(480f, 64f));

            CreateSheetRow(panelRt, "Talent Row", "TALENT", new Vector2(0f, 140f), () =>
            {
                CloseSheet();
                lobbyWindow.OpenTalentMenu();
            });
            CreateSheetRow(panelRt, "Codex Row", "CODEX", new Vector2(0f, 40f), () =>
            {
                CloseSheet();
                lobbyWindow.OpenCodexMenu();
            });
            CreateSheetRow(panelRt, "Shop Row", "SHOP", new Vector2(0f, -60f), () =>
            {
                CloseSheet();
                lobbyWindow.OpenShopMenu();
            });
            CreateSheetRow(panelRt, "Close Row", "CLOSE", new Vector2(0f, -220f), CloseSheet,
                new Color(0.35f, 0.35f, 0.4f, 1f));

            root.SetActive(false);
            return root;
        }

        private void CreateSheetRow(RectTransform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick, Color? tint = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(420f, 80f);
            rt.anchoredPosition = pos;

            var image = go.GetComponent<Image>();
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;
            image.color = tint ?? new Color(0.42f, 0.25f, 0.63f, 1f);
            image.pixelsPerUnitMultiplier = 2f;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            CreateLabel(rt, label, 32f);
        }

        private void CreateLabel(RectTransform parent, string text, float size, Vector2? pos = null, Vector2? sizeDelta = null)
        {
            var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.SetParent(parent, false);
            if (pos.HasValue)
            {
                textRt.anchorMin = new Vector2(0.5f, 0.5f);
                textRt.anchorMax = new Vector2(0.5f, 0.5f);
                textRt.anchoredPosition = pos.Value;
                textRt.sizeDelta = sizeDelta ?? new Vector2(400f, 64f);
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
