#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core.UI;
using VXMonster.Gameplay;
using VXMonster.Platform.IAP;
using VXMonster.UI;

namespace VXMonster.EditorTools
{
    public static class VXPrefabHookupWiring
    {
        const string MainMenuPath = "Assets/Common/Prefabs/UI/Screens/Main Menu Screen.prefab";
        const string LobbyPath = "Assets/Common/Prefabs/UI/Upgrades/Lobby Window.prefab";
        const string SettingsPath = "Assets/Common/Prefabs/UI/Upgrades/Settings Window.prefab";
        const string StageFailedPath = "Assets/Common/Prefabs/UI/Screens/Stage Failed Screen.prefab";
        const string StageCompletePath = "Assets/Common/Prefabs/UI/Screens/Stage Complete Screen.prefab";
        const string GameScreenPath = "Assets/Common/Prefabs/UI/Screens/Game Screen.prefab";
        const string TalentNodePrefabPath = "Assets/Common/Prefabs/UI/VX/Talent Node Button.prefab";
        const string ModeButtonsPrefabPath = "Assets/Common/Prefabs/UI/VX/VX Mode Buttons.prefab";
        const string GreenButtonSpritePath = "Assets/Common/Sprites/UI/Generic/ui_button_green.png";
        const string YellowButtonSpritePath = "Assets/Common/Sprites/UI/Generic/ui_button_yellow.png";
        const string ButtonSpriteGuid = "4b9abba727e1c924cba2bcc0ee5bb8de";
        const string GrayButtonSpriteGuid = "3877251ea12389449ad7cb7bad961758";
        const string FontPath = "Assets/Common/Fonts/TMP Font Assets/NeverMindRounded-Bold/Shadow Outline NeverMindRounded-Bold.asset";

        static readonly Color PanelDark = new Color(0.1f, 0.07f, 0.16f, 0.92f);
        static readonly Color PrimaryPurple = new Color(0.42f, 0.25f, 0.63f, 1f);
        static readonly Color PrimaryGreen = new Color(0.3f, 0.69f, 0.31f, 1f);
        static readonly Color Danger = new Color(0.9f, 0.22f, 0.21f, 1f);

        [MenuItem("VX Monster/Wire All Prefab Hookup (Phase 1)")]
        public static void WireAll()
        {
            EnsureTalentNodePrefab();
            WireSettingsLegal();
            WireStageResults();
            WireGameHud();
            WireLobbyHub();
            WireMainMenuModals();
            AssetDatabase.SaveAssets();
            Debug.Log("[VX] Phase 1 prefab hookup complete. Open Main Menu + Game scenes to verify.");
        }

        [MenuItem("VX Monster/Wire Settings Legal")]
        public static void WireSettingsLegal()
        {
            var root = PrefabUtility.LoadPrefabContents(SettingsPath);
            try
            {
                var settings = root.GetComponent<SettingsWindowBehavior>();
                if (settings == null)
                {
                    Debug.LogError("[VX] SettingsWindowBehavior missing.");
                    return;
                }

                var so = new SerializedObject(settings);
                var legalProp = so.FindProperty("legalTextWindow");
                LegalTextWindowBehavior legal = null;

                if (legalProp.objectReferenceValue != null)
                {
                    legal = legalProp.objectReferenceValue as LegalTextWindowBehavior;
                }
                else
                {
                    legal = CreateLegalTextWindow(root.transform);
                    legalProp.objectReferenceValue = legal;
                }

                // Layout (center-anchored): toggles at y=240/120/0.
                // Privacy + Terms share one row under Vibration, then Exit, then Back.
                // Never stack legal buttons on the original Back (y=-240, h=160).
                var privacy = so.FindProperty("privacyButton");
                if (privacy.objectReferenceValue == null)
                {
                    privacy.objectReferenceValue = CreateSettingsFooterButton(root.transform, "Privacy Button", "PRIVACY", -140f).GetComponent<Button>();
                }

                var terms = so.FindProperty("termsButton");
                if (terms.objectReferenceValue == null)
                {
                    terms.objectReferenceValue = CreateSettingsFooterButton(root.transform, "Terms Button", "TERMS", -140f).GetComponent<Button>();
                }

                if (privacy.objectReferenceValue is Button privacyBtn)
                {
                    var rt = privacyBtn.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(-170f, -140f);
                    rt.sizeDelta = new Vector2(300f, 72f);
                }

                if (terms.objectReferenceValue is Button termsBtn)
                {
                    var rt = termsBtn.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(170f, -140f);
                    rt.sizeDelta = new Vector2(300f, 72f);
                }

                var exit = so.FindProperty("exitButton");
                if (exit.objectReferenceValue is Button exitBtn)
                {
                    var rt = exitBtn.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(0f, -300f);
                    rt.sizeDelta = new Vector2(420f, 140f);
                }

                var back = so.FindProperty("backButton");
                if (back.objectReferenceValue is Button backBtn)
                {
                    var rt = backBtn.GetComponent<RectTransform>();
                    // Android hides Exit; keep Back above the home-indicator band.
                    rt.anchoredPosition = new Vector2(0f, -400f);
                    rt.sizeDelta = new Vector2(420f, 140f);
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, SettingsPath);
                Debug.Log("[VX] Settings legal buttons wired.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [MenuItem("VX Monster/Wire Stage Results")]
        public static void WireStageResults()
        {
            WireStageFailed();
            WireStageComplete();
        }

        static void WireStageFailed()
        {
            var root = PrefabUtility.LoadPrefabContents(StageFailedPath);
            try
            {
                var screen = root.GetComponent<StageFailedScreen>();
                var so = new SerializedObject(screen);
                var statsProp = so.FindProperty("statsText");
                var retryProp = so.FindProperty("retryButton");

                // Title → Stats (280, above character) → clear body → packed buttons from -120.
                if (statsProp.objectReferenceValue == null)
                {
                    var stats = CreateResultsLabel(root.transform, "Stats Text", new Vector2(0f, 280f));
                    statsProp.objectReferenceValue = stats;
                }

                if (statsProp.objectReferenceValue is TMP_Text existingStats)
                {
                    var rt = existingStats.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(0f, 280f);
                    rt.sizeDelta = new Vector2(860f, 80f);
                    existingStats.fontSize = 22f;
                    existingStats.enableAutoSizing = false;
                    existingStats.textWrappingMode = TextWrappingModes.Normal;
                    existingStats.overflowMode = TextOverflowModes.Overflow;
                    existingStats.alignment = TextAlignmentOptions.Center;
                    existingStats.lineSpacing = 0f;
                    existingStats.raycastTarget = false;
                }

                if (retryProp.objectReferenceValue == null)
                {
                    var retry = CreateResultsButton(root.transform, "Retry Button", "RETRY", new Vector2(0f, -120f), PrimaryGreen);
                    retryProp.objectReferenceValue = retry.GetComponent<Button>();
                }

                // Default packed stack when Ad Revive is visible: Retry / Ad / Exit.
                PlaceFailedActionButton(root.transform.Find("Retry Button") as RectTransform, new Vector2(0f, -120f), PrimaryGreen, "RETRY", 48f, false);
                PlaceFailedActionButton(root.transform.Find("Revive Button") as RectTransform, new Vector2(0f, -250f), new Color(0.28f, 1f, 0f, 1f), "REVIVE", 48f, false);
                PlaceFailedActionButton(root.transform.Find("Ad Revive Button") as RectTransform, new Vector2(0f, -250f), new Color(0.28f, 1f, 0f, 1f), "WATCH AD\nREVIVE", 36f, true);
                PlaceFailedActionButton(root.transform.Find("Exit Button") as RectTransform, new Vector2(0f, -380f), new Color(0.85f, 0.15f, 0.15f, 1f), "EXIT", 48f, false);

                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, StageFailedPath);
                Debug.Log("[VX] Stage Failed v2: dynamic pack + label fix.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void PlaceFailedActionButton(RectTransform rt, Vector2 anchoredPosition, Color tint, string label, float fontSize, bool allowWrap)
        {
            if (rt == null) return;

            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(400f, 110f);

            var image = rt.GetComponent<Image>();
            if (image != null)
            {
                image.color = tint;
                var spritePath = tint.r > 0.5f && tint.g < 0.4f
                    ? "Assets/Common/Sprites/UI/Generic/ui_button_red.png"
                    : "Assets/Common/Sprites/UI/Generic/ui_button_green.png";
                var rounded = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (rounded == null)
                {
                    rounded = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath("a4088e99fa611aa4b9d9c04d5c43b541"));
                }

                if (rounded != null)
                {
                    image.sprite = rounded;
                    image.type = Image.Type.Sliced;
                }
            }

            var labels = rt.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (labels == null || labels.Length == 0) return;

            // Keep a single clean label; destroy accidental duplicates that cause glyph overlap.
            for (var i = 1; i < labels.Length; i++)
            {
                Object.DestroyImmediate(labels[i].gameObject);
            }

            var tmp = labels[0];
            var textRt = tmp.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(12f, 8f);
            textRt.offsetMax = new Vector2(-12f, -8f);
            textRt.anchoredPosition = Vector2.zero;
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.enableAutoSizing = false;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = allowWrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.characterSpacing = 0f;
            tmp.lineSpacing = allowWrap ? -10f : 0f;
            tmp.raycastTarget = false;
            tmp.color = Color.white;
        }

        static void WireStageComplete()
        {
            var root = PrefabUtility.LoadPrefabContents(StageCompletePath);
            try
            {
                var screen = root.GetComponent<StageCompleteScreen>();
                var so = new SerializedObject(screen);
                var statsProp = so.FindProperty("statsText");

                if (statsProp.objectReferenceValue == null)
                {
                    var stats = CreateResultsLabel(root.transform, "Stats Text", new Vector2(0f, 80f));
                    statsProp.objectReferenceValue = stats;
                }

                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, StageCompletePath);
                Debug.Log("[VX] Stage Complete stats wired.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [MenuItem("VX Monster/Wire Game HUD")]
        public static void WireGameHud()
        {
            var root = PrefabUtility.LoadPrefabContents(GameScreenPath);
            try
            {
                var safeArea = root.transform.Find("Safe Area");
                var parent = safeArea != null ? safeArea : root.transform;

                var hudRoot = parent.Find("VX HUD");
                if (hudRoot == null)
                {
                    var go = new GameObject("VX HUD", typeof(RectTransform));
                    hudRoot = go.transform;
                    var rt = go.GetComponent<RectTransform>();
                    rt.SetParent(parent, false);
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }

                var combo = GetOrAdd<ComboHudBehavior>(hudRoot.gameObject);
                var comboSo = new SerializedObject(combo);
                if (comboSo.FindProperty("comboLabel").objectReferenceValue == null)
                {
                    var label = CreateHudLabel(hudRoot, "Combo Label", new Vector2(-360f, -80f), TextAlignmentOptions.TopLeft);
                    comboSo.FindProperty("comboLabel").objectReferenceValue = label;
                }
                comboSo.ApplyModifiedPropertiesWithoutUndo();

                var badge = GetOrAdd<DifficultyBadgeHudBehavior>(hudRoot.gameObject);
                var badgeSo = new SerializedObject(badge);
                var badgeProp = badgeSo.FindProperty("badgeLabel");
                TMP_Text badgeLabel = badgeProp.objectReferenceValue as TMP_Text;
                if (badgeLabel == null)
                {
                    badgeLabel = CreateHudLabel(hudRoot, "Difficulty Badge", new Vector2(0f, -16f), TextAlignmentOptions.Center, 22f);
                    badgeProp.objectReferenceValue = badgeLabel;
                }

                // Always pin to top-center — old mid-screen offset was unreadable on Android.
                var badgeRt = badgeLabel.GetComponent<RectTransform>();
                badgeRt.anchorMin = new Vector2(0.5f, 1f);
                badgeRt.anchorMax = new Vector2(0.5f, 1f);
                badgeRt.pivot = new Vector2(0.5f, 1f);
                badgeRt.anchoredPosition = new Vector2(0f, -16f);
                badgeRt.sizeDelta = new Vector2(420f, 36f);
                badgeLabel.fontSize = 22f;
                badgeLabel.enableAutoSizing = false;
                badgeLabel.alignment = TextAlignmentOptions.Center;
                badgeLabel.textWrappingMode = TextWrappingModes.NoWrap;
                badgeLabel.overflowMode = TextOverflowModes.Ellipsis;
                badgeSo.ApplyModifiedPropertiesWithoutUndo();

                var relicHudGo = hudRoot.Find("Relic HUD");
                if (relicHudGo == null)
                {
                    relicHudGo = new GameObject("Relic HUD", typeof(RectTransform)).transform;
                    var relicRt = relicHudGo.GetComponent<RectTransform>();
                    relicRt.SetParent(hudRoot, false);
                    relicRt.anchorMin = new Vector2(0.5f, 0f);
                    relicRt.anchorMax = new Vector2(0.5f, 0f);
                    relicRt.pivot = new Vector2(0.5f, 0f);
                    relicRt.anchoredPosition = new Vector2(0f, 48f);
                    relicRt.sizeDelta = new Vector2(420f, 72f);
                }

                var relic = GetOrAdd<RelicHudBehavior>(relicHudGo.gameObject);
                var relicSo = new SerializedObject(relic);
                var icons = relicSo.FindProperty("slotIcons");
                if (icons.arraySize < 3)
                {
                    icons.ClearArray();
                    for (var i = 0; i < 3; i++)
                    {
                        var slot = CreateRelicSlot(relicHudGo, $"Relic Slot {i + 1}", i);
                        icons.InsertArrayElementAtIndex(i);
                        icons.GetArrayElementAtIndex(i).objectReferenceValue = slot;
                    }
                }

                if (relicSo.FindProperty("emptySlotSprite").objectReferenceValue == null)
                {
                    relicSo.FindProperty("emptySlotSprite").objectReferenceValue = LoadButtonSpriteGray();
                }

                relicSo.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, GameScreenPath);
                Debug.Log("[VX] Game HUD wired.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [MenuItem("VX Monster/Wire Lobby Hub")]
        public static void WireLobbyHub()
        {
            var root = PrefabUtility.LoadPrefabContents(LobbyPath);
            try
            {
                var lobby = root.GetComponent<LobbyWindowBehavior>();
                var so = new SerializedObject(lobby);

                // Meta destinations live in VXLobbyMetaMenu (corner MENU sheet), not on the mode band.
                // Keep serialized refs for Init() listeners, but hide surface buttons so they cannot overlap modes.
                HideHubButtonIfPresent(root.transform, "Talent Button");
                HideHubButtonIfPresent(root.transform, "Codex Button");
                HideHubButtonIfPresent(root.transform, "Shop Button");

                // Ensure serialized refs still exist for LobbyWindowBehavior.Init wiring.
                EnsureHubButton(so, root.transform, "talentButton", "Talent Button", "TALENT", new Vector2(4000f, 4000f), startInactive: true);
                EnsureHubButton(so, root.transform, "codexButton", "Codex Button", "CODEX", new Vector2(4000f, 4000f), startInactive: true);
                EnsureHubButton(so, root.transform, "shopButton", "Shop Button", "SHOP", new Vector2(4000f, 4000f), startInactive: true);

                so.ApplyModifiedPropertiesWithoutUndo();

                WireLobbyModeButtonsInto(root, lobby);

                if (root.GetComponent<VXLobbyMetaMenu>() == null)
                {
                    var meta = root.AddComponent<VXLobbyMetaMenu>();
                    var metaSo = new SerializedObject(meta);
                    metaSo.FindProperty("lobbyWindow").objectReferenceValue = lobby;
                    metaSo.ApplyModifiedPropertiesWithoutUndo();
                }

                // Top-anchored under logo (logo top -240, h 204 → bottom -444). Center Y breaks on short iPhones.
                PlaceOrCreateHudLabel(root.transform, "Daily Best Label", new Vector2(0f, -468f), TextAlignmentOptions.Center, 20f, 32f, topAnchored: true);
                PlaceOrCreateHudLabel(root.transform, "Endless Best Label", new Vector2(0f, -500f), TextAlignmentOptions.Center, 20f, 32f, topAnchored: true);
                PlaceOrCreateHudLabel(root.transform, "Streak Label", new Vector2(0f, -532f), TextAlignmentOptions.Center, 20f, 32f, topAnchored: true);
                PlaceOrCreateHudLabel(root.transform, "Daily Modifiers Label", new Vector2(0f, -564f), TextAlignmentOptions.Center, 18f, 32f, topAnchored: true);

                var pb = root.GetComponent<LocalPersonalBestBehavior>();
                if (pb == null) pb = root.AddComponent<LocalPersonalBestBehavior>();
                var pbSo = new SerializedObject(pb);
                pbSo.FindProperty("dailyBestLabel").objectReferenceValue =
                    root.transform.Find("Daily Best Label")?.GetComponent<TMP_Text>();
                pbSo.FindProperty("endlessBestLabel").objectReferenceValue =
                    root.transform.Find("Endless Best Label")?.GetComponent<TMP_Text>();
                pbSo.FindProperty("streakLabel").objectReferenceValue =
                    root.transform.Find("Streak Label")?.GetComponent<TMP_Text>();
                pbSo.ApplyModifiedPropertiesWithoutUndo();

                var daily = root.GetComponent<DailyModifierPreviewBehavior>();
                if (daily == null) daily = root.AddComponent<DailyModifierPreviewBehavior>();
                var dailySo = new SerializedObject(daily);
                dailySo.FindProperty("modifiersLabel").objectReferenceValue =
                    root.transform.Find("Daily Modifiers Label")?.GetComponent<TMP_Text>();
                dailySo.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, LobbyPath);
                Debug.Log("[VX] Lobby hub buttons + PB + daily modifiers wired.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [MenuItem("VX Monster/Wire Main Menu Modals")]
        public static void WireMainMenuModals()
        {
            var root = PrefabUtility.LoadPrefabContents(MainMenuPath);
            try
            {
                var menu = root.GetComponent<MainMenuScreenBehavior>();
                var so = new SerializedObject(menu);

                var modalsRoot = root.transform.Find("VX Modals");
                if (modalsRoot == null)
                {
                    var go = new GameObject("VX Modals", typeof(RectTransform));
                    modalsRoot = go.transform;
                    var rt = go.GetComponent<RectTransform>();
                    rt.SetParent(root.transform, false);
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                }

                if (so.FindProperty("talentTreeWindow").objectReferenceValue == null)
                {
                    so.FindProperty("talentTreeWindow").objectReferenceValue = CreateTalentTreeModal(modalsRoot);
                }

                if (so.FindProperty("codexWindow").objectReferenceValue == null)
                {
                    so.FindProperty("codexWindow").objectReferenceValue = CreateCodexModal(modalsRoot);
                }

                if (so.FindProperty("shopWindow").objectReferenceValue == null)
                {
                    so.FindProperty("shopWindow").objectReferenceValue = CreateShopModal(modalsRoot);
                }

                if (so.FindProperty("difficultyModal").objectReferenceValue == null)
                {
                    so.FindProperty("difficultyModal").objectReferenceValue = CreateDifficultyModal(modalsRoot);
                }

                so.ApplyModifiedPropertiesWithoutUndo();

                var tutorial = root.GetComponentInChildren<TutorialOverlayBehavior>(true);
                if (tutorial == null)
                {
                    CreateTutorialOverlay(root.transform);
                }

                PrefabUtility.SaveAsPrefabAsset(root, MainMenuPath);
                Debug.Log("[VX] Main Menu modals + tutorial wired.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        static void EnsureTalentNodePrefab()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Common/Prefabs/UI/VX"))
            {
                AssetDatabase.CreateFolder("Assets/Common/Prefabs/UI", "VX");
            }

            if (File.Exists(TalentNodePrefabPath)) return;

            var temp = new GameObject("Talent Node Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = temp.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(900f, 72f);
            var image = temp.GetComponent<Image>();
            image.color = PrimaryPurple;
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(temp.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(16f, 0f);
            labelRt.offsetMax = new Vector2(-16f, 0f);
            var tmp = labelGo.GetComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.fontSize = 28f;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = Color.white;

            PrefabUtility.SaveAsPrefabAsset(temp, TalentNodePrefabPath);
            Object.DestroyImmediate(temp);
        }

        static LegalTextWindowBehavior CreateLegalTextWindow(Transform parent)
        {
            var window = CreateModalShell(parent, "Legal Text Window", false);
            var body = CreateScrollBody(window.transform, "Legal Body");
            var back = CreateModalButton(window.transform, "Back Button", "BACK", new Vector2(0f, -720f), PrimaryPurple);
            var behavior = window.AddComponent<LegalTextWindowBehavior>();
            var so = new SerializedObject(behavior);
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("scrollRect").objectReferenceValue = body.GetComponentInParent<ScrollRect>();
            so.FindProperty("backButton").objectReferenceValue = back.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
            return behavior;
        }

        static TalentTreeWindowBehavior CreateTalentTreeModal(Transform parent)
        {
            var window = CreateModalShell(parent, "VX Talent Tree", true);
            var points = CreateHudLabel(window.transform, "Points Label", new Vector2(0f, 760f), TextAlignmentOptions.Center, 32f);
            var back = CreateModalButton(window.transform, "Back Button", "BACK", new Vector2(0f, -760f), PrimaryPurple);
            var containerGo = new GameObject("Nodes Container", typeof(RectTransform), typeof(VerticalLayoutGroup));
            var containerRt = containerGo.GetComponent<RectTransform>();
            containerRt.SetParent(window.transform, false);
            containerRt.anchorMin = new Vector2(0.5f, 0.5f);
            containerRt.anchorMax = new Vector2(0.5f, 0.5f);
            containerRt.sizeDelta = new Vector2(920f, 900f);
            containerRt.anchoredPosition = new Vector2(0f, -40f);
            var layout = containerGo.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var behavior = window.AddComponent<TalentTreeWindowBehavior>();
            var nodePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TalentNodePrefabPath);
            var so = new SerializedObject(behavior);
            so.FindProperty("pointsLabel").objectReferenceValue = points;
            so.FindProperty("nodesContainer").objectReferenceValue = containerRt;
            so.FindProperty("nodeButtonPrefab").objectReferenceValue = nodePrefab;
            so.FindProperty("backButton").objectReferenceValue = back.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
            return behavior;
        }

        static CodexWindowBehavior CreateCodexModal(Transform parent)
        {
            var window = CreateModalShell(parent, "VX Codex", true);
            var body = CreateScrollBody(window.transform, "Codex Body");
            var back = CreateModalButton(window.transform, "Back Button", "BACK", new Vector2(0f, -760f), PrimaryPurple);
            var behavior = window.AddComponent<CodexWindowBehavior>();
            var so = new SerializedObject(behavior);
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("backButton").objectReferenceValue = back.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
            return behavior;
        }

        static ShopWindowBehavior CreateShopModal(Transform parent)
        {
            var window = CreateModalShell(parent, "VX Shop", true);
            var status = CreateHudLabel(window.transform, "Status Label", new Vector2(0f, -700f), TextAlignmentOptions.Center, 24f);
            var back = CreateModalButton(window.transform, "Back Button", "BACK", new Vector2(0f, -760f), PrimaryPurple);
            var restore = CreateModalButton(window.transform, "Restore Button", "RESTORE", new Vector2(0f, 700f), PrimaryPurple);

            var rowsRoot = new GameObject("Product Rows", typeof(RectTransform), typeof(VerticalLayoutGroup));
            var rowsRt = rowsRoot.GetComponent<RectTransform>();
            rowsRt.SetParent(window.transform, false);
            rowsRt.anchorMin = new Vector2(0.5f, 0.5f);
            rowsRt.anchorMax = new Vector2(0.5f, 0.5f);
            rowsRt.sizeDelta = new Vector2(900f, 520f);
            rowsRt.anchoredPosition = new Vector2(0f, 40f);
            var layout = rowsRoot.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.UpperCenter;

            var behavior = window.AddComponent<ShopWindowBehavior>();
            var so = new SerializedObject(behavior);
            so.FindProperty("statusLabel").objectReferenceValue = status;
            so.FindProperty("backButton").objectReferenceValue = back.GetComponent<Button>();
            so.FindProperty("restoreButton").objectReferenceValue = restore.GetComponent<Button>();

            var rows = so.FindProperty("productRows");
            rows.ClearArray();
            AddShopRow(rows, rowsRt, IAPProductIds.RemoveAds, "Remove Ads", 0);
            AddShopRow(rows, rowsRt, IAPProductIds.StarterBundle, "Starter Bundle", 1);
            AddShopRow(rows, rowsRt, IAPProductIds.GoldSmall, "Gold Small", 2);
            AddShopRow(rows, rowsRt, IAPProductIds.GoldMedium, "Gold Medium", 3);
            AddShopRow(rows, rowsRt, IAPProductIds.GoldLarge, "Gold Large", 4);
            so.ApplyModifiedPropertiesWithoutUndo();
            return behavior;
        }

        static void AddShopRow(SerializedProperty rows, Transform parent, string productId, string title, int index)
        {
            var rowGo = new GameObject(title, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var rowRt = rowGo.GetComponent<RectTransform>();
            rowRt.SetParent(parent, false);
            rowRt.sizeDelta = new Vector2(880f, 72f);
            var rowLayout = rowGo.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 16f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = false;
            rowLayout.childForceExpandWidth = false;

            var titleLabel = CreateHudLabel(rowRt, "Title", new Vector2(0f, 0f), TextAlignmentOptions.MidlineLeft, 26f);
            var titleRt = titleLabel.rectTransform;
            titleRt.sizeDelta = new Vector2(360f, 56f);
            titleLabel.text = title;

            var priceLabel = CreateHudLabel(rowRt, "Price", new Vector2(0f, 0f), TextAlignmentOptions.MidlineLeft, 24f);
            priceLabel.rectTransform.sizeDelta = new Vector2(180f, 56f);
            priceLabel.text = "...";

            var buy = CreateModalButton(rowRt, "Buy", "BUY", Vector2.zero, PrimaryGreen);
            buy.GetComponent<RectTransform>().sizeDelta = new Vector2(180f, 64f);

            rows.InsertArrayElementAtIndex(index);
            var element = rows.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("productId").stringValue = productId;
            element.FindPropertyRelative("buyButton").objectReferenceValue = buy.GetComponent<Button>();
            element.FindPropertyRelative("priceLabel").objectReferenceValue = priceLabel;
            element.FindPropertyRelative("titleLabel").objectReferenceValue = titleLabel;
        }

        static DifficultyModalWindowBehavior CreateDifficultyModal(Transform parent)
        {
            var window = CreateModalShell(parent, "VX Difficulty Modal", true);
            window.AddComponent<CanvasGroup>();
            var preview = CreateHudLabel(window.transform, "Preview Label", new Vector2(0f, 120f), TextAlignmentOptions.Center, 28f);
            var close = CreateModalButton(window.transform, "Close Button", "CLOSE", new Vector2(0f, -760f), PrimaryPurple);
            var easy = CreateModalButton(window.transform, "Easy Button", "EASY", new Vector2(0f, 520f), PrimaryGreen);
            var normal = CreateModalButton(window.transform, "Normal Button", "NORMAL", new Vector2(0f, 420f), PrimaryPurple);
            var hard = CreateModalButton(window.transform, "Hard Button", "HARD", new Vector2(0f, 320f), new Color(0.85f, 0.55f, 0.1f, 1f));
            var nightmare = CreateModalButton(window.transform, "Nightmare Button", "NIGHTMARE", new Vector2(0f, 220f), Danger);

            var behavior = window.AddComponent<DifficultyModalWindowBehavior>();
            var so = new SerializedObject(behavior);
            so.FindProperty("canvasGroup").objectReferenceValue = window.GetComponent<CanvasGroup>();
            so.FindProperty("closeButton").objectReferenceValue = close.GetComponent<Button>();
            so.FindProperty("easyButton").objectReferenceValue = easy.GetComponent<Button>();
            so.FindProperty("normalButton").objectReferenceValue = normal.GetComponent<Button>();
            so.FindProperty("hardButton").objectReferenceValue = hard.GetComponent<Button>();
            so.FindProperty("nightmareButton").objectReferenceValue = nightmare.GetComponent<Button>();
            so.FindProperty("previewLabel").objectReferenceValue = preview;
            so.ApplyModifiedPropertiesWithoutUndo();
            return behavior;
        }

        static void CreateTutorialOverlay(Transform parent)
        {
            var overlay = CreateModalShell(parent, "VX Tutorial Overlay", true);
            overlay.AddComponent<CanvasGroup>();
            var body = CreateHudLabel(overlay.transform, "Tutorial Body", new Vector2(0f, 80f), TextAlignmentOptions.Center, 30f);
            body.rectTransform.sizeDelta = new Vector2(900f, 420f);
            var next = CreateModalButton(overlay.transform, "Next Button", "NEXT", new Vector2(-180f, -320f), PrimaryGreen);
            var skip = CreateModalButton(overlay.transform, "Skip Button", "SKIP", new Vector2(180f, -320f), PrimaryPurple);
            var behavior = overlay.AddComponent<TutorialOverlayBehavior>();
            var so = new SerializedObject(behavior);
            so.FindProperty("canvasGroup").objectReferenceValue = overlay.GetComponent<CanvasGroup>();
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.FindProperty("nextButton").objectReferenceValue = next.GetComponent<Button>();
            so.FindProperty("skipButton").objectReferenceValue = skip.GetComponent<Button>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject CreateModalShell(Transform parent, string name, bool startInactive)
        {
            var existing = parent.Find(name);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = PanelDark;
            go.SetActive(false);
            return go;
        }

        static TMP_Text CreateScrollBody(Transform parent, string name)
        {
            var scrollGo = new GameObject(name, typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            var scrollRt = scrollGo.GetComponent<RectTransform>();
            scrollRt.SetParent(parent, false);
            scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
            scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRt.sizeDelta = new Vector2(920f, 1100f);
            scrollRt.anchoredPosition = new Vector2(0f, -40f);
            scrollGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.25f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            var viewportRt = viewport.GetComponent<RectTransform>();
            viewportRt.SetParent(scrollRt, false);
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.offsetMin = new Vector2(12f, 12f);
            viewportRt.offsetMax = new Vector2(-12f, -12f);
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.05f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter));
            var contentRt = content.GetComponent<RectTransform>();
            contentRt.SetParent(viewportRt, false);
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 900f);
            var fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(contentRt, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(16f, 16f);
            textRt.offsetMax = new Vector2(-16f, -16f);
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.font = LoadFont();
            tmp.fontSize = 28f;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = viewportRt;
            scroll.content = contentRt;
            scroll.horizontal = false;
            scroll.vertical = true;
            return tmp;
        }

        static GameObject CreateSettingsFooterButton(Transform parent, string name, string label, float y)
        {
            return CreateModalButton(parent, name, label, new Vector2(0f, y), PrimaryPurple);
        }

        static void HideHubButtonIfPresent(Transform root, string objectName)
        {
            var existing = root.Find(objectName);
            if (existing != null) existing.gameObject.SetActive(false);
        }

        static void EnsureHubButton(SerializedObject lobbySo, Transform root, string propertyName, string objectName, string label, Vector2 anchoredPosition, bool startInactive = false)
        {
            var prop = lobbySo.FindProperty(propertyName);
            Button button = prop.objectReferenceValue as Button;
            if (button == null)
            {
                var existing = root.Find(objectName);
                if (existing != null) Object.DestroyImmediate(existing.gameObject);
                button = CreateHubButton(root, objectName, label, anchoredPosition).GetComponent<Button>();
                prop.objectReferenceValue = button;
            }

            var rt = button.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(180f, 56f);
            button.gameObject.SetActive(!startInactive);
        }

        static GameObject CreateHubButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var go = CreateModalButton(parent, name, label, anchoredPosition, PrimaryPurple);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(180f, 56f);
            return go;
        }

        static GameObject CreateModalButton(Transform parent, string name, string label, Vector2 anchoredPosition, Color tint)
        {
            var sprite = LoadButtonSprite();
            var font = LoadFont();
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(320f, 72f);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = tint;
            image.pixelsPerUnitMultiplier = 2f;
            go.GetComponent<Button>().targetGraphic = image;

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.SetParent(rt, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;
            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.font = font;
            tmp.fontSize = 30f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return go;
        }

        static TMP_Text CreateResultsLabel(Transform parent, string name, Vector2 position)
        {
            return CreateHudLabel(parent, name, position, TextAlignmentOptions.Center, 28f);
        }

        static GameObject CreateResultsButton(Transform parent, string name, string label, Vector2 position, Color tint)
        {
            return CreateModalButton(parent, name, label, position, tint);
        }

        static TMP_Text CreateHudLabel(Transform parent, string name, Vector2 anchoredPosition, TextAlignmentOptions align, float fontSize = 28f)
        {
            var existing = parent.Find(name);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = new Vector2(900f, 48f);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.font = LoadFont();
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            return tmp;
        }

        static void PlaceOrCreateHudLabel(Transform parent, string name, Vector2 anchoredPosition, TextAlignmentOptions align, float fontSize, float height = 36f, bool topAnchored = false)
        {
            var existing = parent.Find(name);
            if (existing != null)
            {
                var rt = existing.GetComponent<RectTransform>();
                if (rt != null)
                {
                    if (topAnchored)
                    {
                        rt.anchorMin = new Vector2(0.5f, 1f);
                        rt.anchorMax = new Vector2(0.5f, 1f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                    }

                    rt.anchoredPosition = anchoredPosition;
                    rt.sizeDelta = new Vector2(900f, height);
                }

                var tmp = existing.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.fontSize = fontSize;
                    tmp.alignment = align;
                    tmp.enableAutoSizing = false;
                    tmp.overflowMode = TextOverflowModes.Ellipsis;
                    tmp.textWrappingMode = TextWrappingModes.NoWrap;
                }

                return;
            }

            var created = CreateHudLabel(parent, name, anchoredPosition, align, fontSize);
            var createdRt = created.GetComponent<RectTransform>();
            if (createdRt != null)
            {
                if (topAnchored)
                {
                    createdRt.anchorMin = new Vector2(0.5f, 1f);
                    createdRt.anchorMax = new Vector2(0.5f, 1f);
                    createdRt.pivot = new Vector2(0.5f, 0.5f);
                    createdRt.anchoredPosition = anchoredPosition;
                }

                createdRt.sizeDelta = new Vector2(900f, height);
            }

            created.enableAutoSizing = false;
            created.overflowMode = TextOverflowModes.Ellipsis;
            created.textWrappingMode = TextWrappingModes.NoWrap;
        }

        static Image CreateRelicSlot(Transform parent, string name, int index)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-120f + index * 120f, 0f);
            rt.sizeDelta = new Vector2(64f, 64f);
            var image = go.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0.35f);
            return image;
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        [MenuItem("VX Monster/Wire Lobby Mode Buttons Prefab")]
        public static void WireLobbyModeButtonsMenu()
        {
            var root = PrefabUtility.LoadPrefabContents(LobbyPath);
            try
            {
                var lobby = root.GetComponent<LobbyWindowBehavior>();
                WireLobbyModeButtonsInto(root, lobby);
                PrefabUtility.SaveAsPrefabAsset(root, LobbyPath);
                Debug.Log("[VX] Lobby mode buttons prefab created and wired. Drag them in Lobby Window to reposition.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /// <summary>
        /// Builds editable mode buttons under Lobby Window and saves a reusable prefab copy.
        /// Simple Image+Button+Label only (no hit-split components). Uses Play/Upgrade sprites.
        /// </summary>
        static void WireLobbyModeButtonsInto(GameObject lobbyRoot, LobbyWindowBehavior lobby)
        {
            const string rootName = "VX Mode Buttons";

            for (var i = lobbyRoot.transform.childCount - 1; i >= 0; i--)
            {
                var child = lobbyRoot.transform.GetChild(i);
                if (child.name == rootName || child.name.StartsWith(rootName + "_"))
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }

            // Restore production lobby anchors (pre-touch-target experiment).
            var attempts = FindDeepChild(lobbyRoot.transform, "Attempts Text (TMP)") as RectTransform;
            if (attempts != null)
            {
                attempts.anchoredPosition = new Vector2(0f, -289.5f);
                attempts.sizeDelta = new Vector2(500f, 50f);
            }

            SetAnchoredY(FindDeepChild(lobbyRoot.transform, "Play Button Background"), -550f);
            SetAnchoredY(FindDeepChild(lobbyRoot.transform, "Upgrade Button Background"), -780f);
            SetAnchoredY(FindDeepChild(lobbyRoot.transform, "Characters Button Background"), -780f);
            // Do NOT move Upgrade/Characters Button children — they are bottom-anchored
            // inside the backgrounds; world Y would push icons off the frames.

            var infoBtn = FindDeepChild(lobbyRoot.transform, "Info Button") as RectTransform;
            if (infoBtn != null)
            {
                infoBtn.anchoredPosition = new Vector2(400f, -400f);
            }

            var folder = Path.GetDirectoryName(ModeButtonsPrefabPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(folder) && !AssetDatabase.IsValidFolder(folder))
            {
                Directory.CreateDirectory(folder);
                AssetDatabase.Refresh();
            }

            var modeRoot = new GameObject(rootName, typeof(RectTransform));
            var modeRt = modeRoot.GetComponent<RectTransform>();
            modeRt.SetParent(lobbyRoot.transform, false);
            modeRt.anchorMin = new Vector2(0.5f, 0.5f);
            modeRt.anchorMax = new Vector2(0.5f, 0.5f);
            modeRt.pivot = new Vector2(0.5f, 0.5f);
            modeRt.anchoredPosition = Vector2.zero;
            modeRt.sizeDelta = Vector2.zero;

            // Fits between Attempts (~-315) and Play top (~-448) without overlap.
            const float rowH = 64f;
            const float modeW = 180f;
            const float diffW = 500f;
            const float modeY = -410f;
            const float diffY = -340f;
            const float step = 195f;

            var yellow = AssetDatabase.LoadAssetAtPath<Sprite>(YellowButtonSpritePath) ?? LoadButtonSprite();
            var green = AssetDatabase.LoadAssetAtPath<Sprite>(GreenButtonSpritePath) ?? yellow;

            var difficulty = CreateLobbyModeButton(modeRt, "Difficulty Button", "DIFF · NORMAL", new Vector2(0f, diffY), new Vector2(diffW, rowH), yellow, Color.white);
            var daily = CreateLobbyModeButton(modeRt, "Daily Challenge", "DAILY", new Vector2(-step, modeY), new Vector2(modeW, rowH), green, Color.white);
            var practice = CreateLobbyModeButton(modeRt, "Practice", "PRACTICE", new Vector2(0f, modeY), new Vector2(modeW, rowH), yellow, Color.white);
            var endless = CreateLobbyModeButton(modeRt, "Endless", "ENDLESS", new Vector2(step, modeY), new Vector2(modeW, rowH), yellow, Color.white);

            PrefabUtility.SaveAsPrefabAsset(modeRoot, ModeButtonsPrefabPath);

            var mode = lobbyRoot.GetComponent<VXLobbyModePanel>();
            if (mode == null) mode = lobbyRoot.AddComponent<VXLobbyModePanel>();
            var modeSo = new SerializedObject(mode);
            modeSo.FindProperty("lobbyWindow").objectReferenceValue = lobby;
            modeSo.FindProperty("difficultyButton").objectReferenceValue = difficulty.GetComponent<Button>();
            modeSo.FindProperty("dailyButton").objectReferenceValue = daily.GetComponent<Button>();
            modeSo.FindProperty("practiceButton").objectReferenceValue = practice.GetComponent<Button>();
            modeSo.FindProperty("endlessButton").objectReferenceValue = endless.GetComponent<Button>();
            modeSo.FindProperty("difficultyLabel").objectReferenceValue = difficulty.GetComponentInChildren<TMP_Text>();
            modeSo.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetAnchoredY(Transform t, float y)
        {
            var rt = t as RectTransform;
            if (rt == null) return;
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, y);
        }

        static GameObject CreateLobbyModeButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchoredPosition,
            Vector2 size,
            Sprite sprite,
            Color tint)
        {
            var font = LoadFont();
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = tint;
            image.pixelsPerUnitMultiplier = 1.85f;
            image.raycastTarget = true;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.SetParent(rt, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(8f, 4f);
            textRt.offsetMax = new Vector2(-8f, -4f);

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.font = font;
            tmp.fontSize = 26f;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 16f;
            tmp.fontSizeMax = 26f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return go;
        }

        static Transform FindDeepChild(Transform parent, string childName)
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

        static Sprite LoadButtonSprite()
        {
            var yellow = AssetDatabase.LoadAssetAtPath<Sprite>(YellowButtonSpritePath);
            if (yellow != null) return yellow;
            return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(ButtonSpriteGuid));
        }

        static Sprite LoadButtonSpriteGray()
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(GrayButtonSpriteGuid));
        }

        static TMP_FontAsset LoadFont()
        {
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        }
    }
}
#endif
