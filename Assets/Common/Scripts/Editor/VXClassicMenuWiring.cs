#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core.UI;
using VXMonster.UI;

namespace VXMonster.EditorTools
{
    /// <summary>
    /// Builds classic purple MENU button + sheet into Lobby Window.prefab.
    /// </summary>
    public static class VXClassicMenuWiring
    {
        const string InfoIconPath = "Assets/Common/Sprites/UI/Main Menu/ui_info.png";

        static readonly Color PrimaryPurple = new Color(0.42f, 0.25f, 0.63f, 1f);
        static readonly Color CloseGray = new Color(0.35f, 0.35f, 0.4f, 1f);
        static readonly Color PanelDark = new Color(0.12f, 0.08f, 0.2f, 0.98f);

        const float RowHeight = 96f;
        const float RowWidth = 480f;
        const float PanelWidth = 560f;

        public static void WireClassicMetaMenuSheet(GameObject lobbyRoot, LobbyWindowBehavior lobby)
        {
            DestroyChild(lobbyRoot.transform, "VX Menu Button");
            DestroyChild(lobbyRoot.transform, "VX Meta Menu Sheet");

            var sprite = VXPrefabHookupWiring.LoadButtonSpriteForEditor();
            var font = VXPrefabHookupWiring.LoadFontForEditor();
            var infoIcon = AssetDatabase.LoadAssetAtPath<Sprite>(InfoIconPath);

            var menuBtn = CreateMenuButton(lobbyRoot.transform, sprite, font);
            var sheet = CreateMenuSheet(lobbyRoot.transform, sprite, font, infoIcon);

            var meta = lobbyRoot.GetComponent<VXLobbyMetaMenu>();
            if (meta == null) meta = lobbyRoot.AddComponent<VXLobbyMetaMenu>();

            var panel = sheet.transform.Find("Panel");
            var talentRow = panel.Find("Talent Row");
            var metaSo = new SerializedObject(meta);
            metaSo.FindProperty("lobbyWindow").objectReferenceValue = lobby;
            metaSo.FindProperty("menuButton").objectReferenceValue = menuBtn.GetComponent<Button>();
            metaSo.FindProperty("sheetRoot").objectReferenceValue = sheet;
            metaSo.FindProperty("infoPopup").objectReferenceValue = sheet.GetComponentInChildren<VxMetaMenuInfoPopup>(true);
            metaSo.FindProperty("talentRowButton").objectReferenceValue = talentRow?.GetComponent<Button>();
            metaSo.FindProperty("talentInfoButton").objectReferenceValue = talentRow?.Find("Info Button")?.GetComponent<Button>();
            metaSo.FindProperty("codexRowButton").objectReferenceValue = panel.Find("Codex Row")?.GetComponent<Button>();
            metaSo.FindProperty("shopRowButton").objectReferenceValue = panel.Find("Shop Row")?.GetComponent<Button>();
            metaSo.FindProperty("leaderboardsRowButton").objectReferenceValue =
                panel.Find("Play Games Leaderboard Row")?.GetComponent<Button>();
            metaSo.FindProperty("closeRowButton").objectReferenceValue = panel.Find("Close Row")?.GetComponent<Button>();
            metaSo.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject CreateMenuButton(Transform parent, Sprite sprite, TMP_FontAsset font)
        {
            var go = new GameObject("VX Menu Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-16f, -100f);
            rt.sizeDelta = new Vector2(168f, 68f);
            ApplyRowSprite(go.GetComponent<Image>(), sprite, PrimaryPurple);
            go.GetComponent<Button>().targetGraphic = go.GetComponent<Image>();
            AddCenterLabel(rt, "MENU", font, 32f);
            return go;
        }

        static GameObject CreateMenuSheet(Transform parent, Sprite sprite, TMP_FontAsset font, Sprite infoIcon)
        {
            var root = new GameObject("VX Meta Menu Sheet", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.SetParent(parent, false);
            Stretch(rootRt);
            root.GetComponent<Image>().color = new Color(0.05f, 0.03f, 0.1f, 0.92f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.SetParent(rootRt, false);
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(PanelWidth, 0f);
            panel.GetComponent<Image>().color = PanelDark;
            var vlg = panel.GetComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(40, 40, 36, 36);
            vlg.spacing = 16f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            panel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            AddLayoutTitle(panelRt, "Title", "MENU", font, 44f, 72f);
            CreateMenuRow(panelRt, "Talent Row", "TALENT", sprite, font, PrimaryPurple, withInfo: true, infoIcon);
            CreateMenuRow(panelRt, "Codex Row", "CODEX", sprite, font, PrimaryPurple);
            CreateMenuRow(panelRt, "Shop Row", "SHOP", sprite, font, PrimaryPurple);
            CreateMenuRow(panelRt, "Play Games Leaderboard Row", "LEADERBOARDS", sprite, font, PrimaryPurple);
            CreateMenuRow(panelRt, "Close Row", "CLOSE", sprite, font, CloseGray);

            CreateInfoPopup(rootRt, sprite, font);

            root.SetActive(false);
            return root;
        }

        static void CreateMenuRow(
            RectTransform panel,
            string name,
            string label,
            Sprite sprite,
            TMP_FontAsset font,
            Color tint,
            bool withInfo = false,
            Sprite infoIcon = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(panel, false);
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = RowHeight;
            le.minHeight = RowHeight;
            ApplyRowSprite(go.GetComponent<Image>(), sprite, tint);
            go.GetComponent<Button>().targetGraphic = go.GetComponent<Image>();
            AddCenterLabel(rt, label, font, 36f);

            if (withInfo && infoIcon != null)
            {
                var infoGo = new GameObject("Info Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                var infoRt = infoGo.GetComponent<RectTransform>();
                infoRt.SetParent(rt, false);
                infoRt.anchorMin = new Vector2(1f, 0.5f);
                infoRt.anchorMax = new Vector2(1f, 0.5f);
                infoRt.pivot = new Vector2(1f, 0.5f);
                infoRt.anchoredPosition = new Vector2(-14f, 0f);
                infoRt.sizeDelta = new Vector2(48f, 48f);
                infoGo.GetComponent<Image>().sprite = infoIcon;
                infoGo.GetComponent<Image>().preserveAspect = true;
                infoGo.GetComponent<Button>().targetGraphic = infoGo.GetComponent<Image>();
            }
        }

        static void CreateInfoPopup(RectTransform sheetRoot, Sprite sprite, TMP_FontAsset font)
        {
            var popupRoot = new GameObject("Info Popup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VxMetaMenuInfoPopup));
            var popupRt = popupRoot.GetComponent<RectTransform>();
            popupRt.SetParent(sheetRoot, false);
            Stretch(popupRt);
            var dim = popupRoot.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.55f);
            dim.raycastTarget = true;

            var card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(VerticalLayoutGroup));
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.SetParent(popupRt, false);
            cardRt.anchorMin = new Vector2(0.5f, 0.5f);
            cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(520f, 280f);
            card.GetComponent<Image>().color = PanelDark;

            var titleGo = AddLayoutTitle(cardRt, "Popup Title", "Talent Tree", font, 36f, 52f);
            titleGo.fontStyle = FontStyles.Bold;

            var bodyGo = new GameObject("Popup Body", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            bodyGo.transform.SetParent(cardRt, false);
            bodyGo.GetComponent<LayoutElement>().preferredHeight = 140f;
            var body = bodyGo.GetComponent<TextMeshProUGUI>();
            body.font = font;
            body.fontSize = 28f;
            body.alignment = TextAlignmentOptions.TopLeft;
            body.color = Color.white;
            body.textWrappingMode = TextWrappingModes.Normal;
            body.text = "Spend talent points earned from bosses on permanent upgrades between runs.";

            var okGo = new GameObject("Close Info", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            okGo.transform.SetParent(cardRt, false);
            okGo.GetComponent<LayoutElement>().preferredHeight = 64f;
            var okRt = okGo.GetComponent<RectTransform>();
            okRt.sizeDelta = new Vector2(360f, 64f);
            ApplyRowSprite(okGo.GetComponent<Image>(), sprite, PrimaryPurple);
            okGo.GetComponent<Button>().targetGraphic = okGo.GetComponent<Image>();
            AddCenterLabel(okRt, "OK", font, 30f);

            var popup = popupRoot.GetComponent<VxMetaMenuInfoPopup>();
            popup.Initialize(popupRoot, titleGo, body, okGo.GetComponent<Button>());
            popupRoot.SetActive(false);
        }

        static TMP_Text AddLayoutTitle(RectTransform parent, string name, string text, TMP_FontAsset font, float size, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().preferredHeight = height;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        static void AddCenterLabel(RectTransform parent, string text, TMP_FontAsset font, float size)
        {
            var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.SetParent(parent, false);
            Stretch(textRt);
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
        }

        static void ApplyRowSprite(Image image, Sprite sprite, Color tint)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = tint;
            image.pixelsPerUnitMultiplier = 2f;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void DestroyChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }
    }
}
#endif
