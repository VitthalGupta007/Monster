#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core.Abilities.UI;
using VXMonster.UI;

namespace VXMonster.EditorTools
{
    public static class VXFirstSessionUiWiring
    {
        const string AbilitiesPopupPath = "Assets/Common/Prefabs/UI/Abilities/Abilities Popup.prefab";
        const string LoadingScenePath = "Assets/Common/Scenes/Loading Screen.unity";
        const string ButtonSpriteGuid = "4b9abba727e1c924cba2bcc0ee5bb8de";
        const string FontPath = "Assets/Common/Fonts/TMP Font Assets/NeverMindRounded-Bold/Shadow Outline NeverMindRounded-Bold.asset";

        [MenuItem("VX Monster/Wire First Session UI (Reroll+Banish + Loading)")]
        public static void WireAll()
        {
            WireAbilitiesPopup();
            WireLoadingScreen();
            AssetDatabase.SaveAssets();
            Debug.Log("[VX] First-session UI wiring complete.");
        }

        [MenuItem("VX Monster/Wire Abilities Popup Reroll+Banish")]
        public static void WireAbilitiesPopup()
        {
            var root = PrefabUtility.LoadPrefabContents(AbilitiesPopupPath);
            try
            {
                var behavior = root.GetComponent<AbilitiesWindowBehavior>();
                if (behavior == null)
                {
                    Debug.LogError("[VX] AbilitiesWindowBehavior missing on prefab.");
                    return;
                }

                var so = new SerializedObject(behavior);
                if (so.FindProperty("rerollButton").objectReferenceValue != null &&
                    so.FindProperty("banishButton").objectReferenceValue != null)
                {
                    Debug.Log("[VX] Abilities Popup already has reroll/banish wired.");
                    return;
                }

                var existing = root.transform.Find("Action Buttons");
                if (existing != null) Object.DestroyImmediate(existing.gameObject);

                var buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(ButtonSpriteGuid));
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

                var row = new GameObject("Action Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                var rowRt = row.GetComponent<RectTransform>();
                rowRt.SetParent(root.transform, false);
                rowRt.anchorMin = new Vector2(0.5f, 0f);
                rowRt.anchorMax = new Vector2(0.5f, 0f);
                rowRt.pivot = new Vector2(0.5f, 0f);
                rowRt.anchoredPosition = new Vector2(0f, 36f);
                rowRt.sizeDelta = new Vector2(860f, 96f);

                var layout = row.GetComponent<HorizontalLayoutGroup>();
                layout.spacing = 40f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                var reroll = CreateActionButton(rowRt, "Reroll Button", "REROLL", buttonSprite, font, new Color(0.42f, 0.25f, 0.63f, 1f));
                var banish = CreateActionButton(rowRt, "Banish Button", "BANISH", buttonSprite, font, new Color(0.9f, 0.22f, 0.21f, 1f));

                so.FindProperty("rerollButton").objectReferenceValue = reroll.GetComponent<Button>();
                so.FindProperty("banishButton").objectReferenceValue = banish.GetComponent<Button>();
                so.FindProperty("rerollLabel").objectReferenceValue = reroll.GetComponentInChildren<TMP_Text>(true);
                so.FindProperty("banishLabel").objectReferenceValue = banish.GetComponentInChildren<TMP_Text>(true);
                so.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(root, AbilitiesPopupPath);
                Debug.Log("[VX] Wired reroll/banish on Abilities Popup.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [MenuItem("VX Monster/Wire Loading Screen Progress")]
        public static void WireLoadingScreen()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(LoadingScenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
            var canvas = GameObject.Find("Loaind Canvas");
            if (canvas == null)
            {
                Debug.LogError("[VX] Loaind Canvas not found in Loading Screen.");
                return;
            }

            var behavior = canvas.GetComponent<LoadingScreenBehavior>();
            if (behavior == null) behavior = canvas.AddComponent<LoadingScreenBehavior>();

            var so = new SerializedObject(behavior);
            var progressProp = so.FindProperty("progressBar");
            var statusProp = so.FindProperty("statusLabel");
            var percentProp = so.FindProperty("percentLabel");

            var loadingText = canvas.transform.Find("Loading Text");
            TMP_Text statusLabel = loadingText != null ? loadingText.GetComponent<TMP_Text>() : null;

            Slider progressBar = null;
            var existingBar = canvas.transform.Find("Progress Bar");
            if (existingBar != null) progressBar = existingBar.GetComponent<Slider>();
            if (progressBar == null)
            {
                progressBar = CreateProgressSlider(canvas.transform);
            }

            TMP_Text percentLabel = null;
            var existingPercent = canvas.transform.Find("Percent Label");
            if (existingPercent != null) percentLabel = existingPercent.GetComponent<TMP_Text>();
            if (percentLabel == null)
            {
                percentLabel = CreatePercentLabel(canvas.transform);
            }

            if (statusLabel != null) statusProp.objectReferenceValue = statusLabel;
            progressProp.objectReferenceValue = progressBar;
            percentProp.objectReferenceValue = percentLabel;
            so.ApplyModifiedPropertiesWithoutUndo();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            Debug.Log("[VX] Wired Loading Screen progress UI.");
        }

        static GameObject CreateActionButton(Transform parent, string name, string label, Sprite sprite, TMP_FontAsset font, Color tint)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(360f, 88f);

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = tint;
            image.pixelsPerUnitMultiplier = 2f;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;

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
            tmp.fontSize = 32f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;

            return go;
        }

        static Slider CreateProgressSlider(Transform parent)
        {
            var root = new GameObject("Progress Bar", typeof(RectTransform), typeof(Slider));
            var rt = root.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -120f);
            rt.sizeDelta = new Vector2(720f, 36f);

            var bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var bgRt = bg.GetComponent<RectTransform>();
            bgRt.SetParent(rt, false);
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImage = bg.GetComponent<Image>();
            bgImage.color = new Color(0.1f, 0.07f, 0.16f, 0.92f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            var fillAreaRt = fillArea.GetComponent<RectTransform>();
            fillAreaRt.SetParent(rt, false);
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.offsetMin = new Vector2(4f, 4f);
            fillAreaRt.offsetMax = new Vector2(-4f, -4f);

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var fillRt = fill.GetComponent<RectTransform>();
            fillRt.SetParent(fillAreaRt, false);
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = new Color(1f, 0.76f, 0.03f, 1f);

            var slider = root.GetComponent<Slider>();
            slider.fillRect = fillRt;
            slider.targetGraphic = fillImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.value = 0f;
            return slider;
        }

        static TMP_Text CreatePercentLabel(Transform parent)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            var go = new GameObject("Percent Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, -170f);
            rt.sizeDelta = new Vector2(200f, 48f);

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = "0%";
            tmp.font = font;
            tmp.fontSize = 28f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return tmp;
        }
    }
}
#endif
