using VXMonster.Core;
using VXMonster.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.UI
{
    /// <summary>
    /// Adds Daily Challenge and Endless mode buttons to the lobby at runtime.
    /// Layout is derived from Lobby Window.prefab rects (Attempts, Play, bottom icons).
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class VXLobbyModePanel : MonoBehaviour
    {
        private const float ButtonWidth = 210f;
        private const float ButtonHeight = 56f;
        private const float ButtonGap = 14f;
        private const float LabelFontSize = 34f;

        [SerializeField] LobbyWindowBehavior lobbyWindow;
        [SerializeField] Sprite buttonSprite;
        [SerializeField] Sprite greenButtonSprite;
        [SerializeField] TMP_FontAsset labelFont;

        private DailyChallengeSave dailySave;
        private TextMeshProUGUI labelStyleReference;
        private bool uiBuilt;

        private void Start()
        {
            BuildModeButtons();
        }

        private void BuildModeButtons()
        {
            if (uiBuilt) return;

            if (lobbyWindow == null)
            {
                lobbyWindow = FindAnyObjectByType<LobbyWindowBehavior>();
            }

            if (lobbyWindow == null) return;

            var lobbyRect = lobbyWindow.GetComponent<RectTransform>();
            if (lobbyRect == null) return;

            if (lobbyRect.Find("VX Mode Buttons") != null)
            {
                uiBuilt = true;
                return;
            }

            if (GameController.SaveManager != null)
            {
                dailySave = GameController.SaveManager.GetSave<DailyChallengeSave>("VX Daily Challenge");
            }

            var playTransform = FindDeepChild(lobbyWindow.transform, "Play Button");
            if (playTransform == null) return;

            var playButton = playTransform.GetComponent<Button>();
            if (playButton == null) return;

            ResolveVisualReferences(playTransform, playButton);

            var playBackground = FindDeepChild(lobbyWindow.transform, "Play Button Background") as RectTransform;
            if (playBackground == null) return;

            var rowCenterY = CalculateRowCenterY(lobbyWindow.transform, playBackground);
            var totalWidth = ButtonWidth * 3f + ButtonGap * 2f;
            var halfStep = ButtonWidth + ButtonGap;

            var root = new GameObject("VX Mode Buttons", typeof(RectTransform));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(lobbyRect, false);
            rootRect.SetAsLastSibling();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = new Vector2(0f, rowCenterY + 10f);
            rootRect.sizeDelta = new Vector2(totalWidth, ButtonHeight);

            CreateModeButton(rootRect, "Daily Rank", new Vector2(-halfStep, 0f), OnDailyScoredClicked, greenButtonSprite ?? buttonSprite);
            CreateModeButton(rootRect, "Practice", new Vector2(0f, 0f), OnDailyPracticeClicked, buttonSprite);
            CreateModeButton(rootRect, "Endless", new Vector2(halfStep, 0f), OnEndlessClicked, buttonSprite);

            uiBuilt = true;
        }

        private static float CalculateRowCenterY(Transform lobbyRoot, RectTransform playBackground)
        {
            const float padding = 12f;

            var playTop = playBackground.anchoredPosition.y + playBackground.rect.height * 0.5f;
            var gapBottom = playTop + padding;

            var attempts = FindDeepChild(lobbyRoot, "Attempts Text (TMP)") as RectTransform;
            if (attempts != null && attempts.gameObject.activeInHierarchy)
            {
                var attemptsBottom = attempts.anchoredPosition.y - attempts.rect.height * 0.5f;
                var gapTop = attemptsBottom - padding;
                var gapSize = gapTop - gapBottom;

                if (gapSize >= ButtonHeight)
                {
                    return gapBottom + gapSize * 0.5f;
                }
            }

            return gapBottom + ButtonHeight * 0.5f;
        }

        private void ResolveVisualReferences(Transform playTransform, Button playButton)
        {
            if (buttonSprite == null)
            {
                buttonSprite = playButton.image.sprite;
            }

            if (greenButtonSprite == null)
            {
                var upgradeButton = FindDeepChild(lobbyWindow.transform, "Upgrade Button");
                if (upgradeButton != null && upgradeButton.TryGetComponent<Image>(out var upgradeImage))
                {
                    greenButtonSprite = upgradeImage.sprite;
                }
            }

            var playLabelTransform = playTransform.Find("Text (TMP)");
            if (playLabelTransform != null)
            {
                labelStyleReference = playLabelTransform.GetComponent<TextMeshProUGUI>();
                if (labelFont == null && labelStyleReference != null)
                {
                    labelFont = labelStyleReference.font;
                }
            }
        }

        private void OnDailyScoredClicked()
        {
            if (dailySave != null && dailySave.HasScoredToday(GameSessionManager.GetUtcDateKey()))
            {
                lobbyWindow.StartDailyChallenge(false);
                return;
            }

            lobbyWindow.StartDailyChallenge(true);
        }

        private void OnDailyPracticeClicked()
        {
            lobbyWindow.StartDailyChallenge(false);
        }

        private void OnEndlessClicked()
        {
            lobbyWindow.StartEndlessRun(DifficultyTier.Normal);
        }

        private void CreateModeButton(RectTransform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick, Sprite sprite)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1.85f;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.SetParent(rect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 4f);
            textRect.offsetMax = new Vector2(-8f, -4f);

            ApplyLabelStyle(textGo.GetComponent<TextMeshProUGUI>(), label);
        }

        private void ApplyLabelStyle(TextMeshProUGUI tmp, string text)
        {
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableAutoSizing = false;
            tmp.fontSize = LabelFontSize;
            tmp.raycastTarget = false;

            if (labelStyleReference != null)
            {
                tmp.font = labelStyleReference.font;
                tmp.fontSharedMaterial = labelStyleReference.fontSharedMaterial;
                tmp.fontStyle = labelStyleReference.fontStyle;
                tmp.color = labelStyleReference.color;
                tmp.characterSpacing = labelStyleReference.characterSpacing;
                tmp.wordSpacing = labelStyleReference.wordSpacing;
                return;
            }

            if (labelFont != null)
            {
                tmp.font = labelFont;
                tmp.fontSharedMaterial = labelFont.material;
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = Color.white;
            }
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
