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
    /// Adds Daily Challenge, Endless, and Difficulty controls to the lobby at runtime.
    /// Layout coordinates match Lobby Window.prefab (Attempts y=-289, Play y=-550).
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class VXLobbyModePanel : MonoBehaviour
    {
        private const string RootName = "VX Mode Buttons";

        private const float RowHeight = 42f;
        private const float RowGap = 8f;
        private const float HorizontalPadding = 40f;
        private const float ButtonGap = 10f;
        private const float DifficultyButtonWidth = 220f;
        private const float LabelFontSize = 26f;
        private const float LabelFontSizeMin = 18f;

        // Midpoint between Attempts Text bottom (~-359) and Play button top (~-457).
        private const float PanelCenterY = -408f;

        [SerializeField] LobbyWindowBehavior lobbyWindow;
        [SerializeField] Sprite buttonSprite;
        [SerializeField] Sprite greenButtonSprite;
        [SerializeField] TMP_FontAsset labelFont;

        private DailyChallengeSave dailySave;
        private TextMeshProUGUI labelStyleReference;
        private TextMeshProUGUI difficultyLabel;
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

            var existing = lobbyRect.Find(RootName);
            if (existing != null)
            {
                Destroy(existing.gameObject);
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

            var panelHeight = RowHeight * 2f + RowGap;
            var modeRowY = PanelCenterY - (panelHeight * 0.5f) + RowHeight * 0.5f;
            var difficultyRowY = modeRowY + RowHeight + RowGap;

            var lobbyWidth = lobbyRect.rect.width > 0f ? lobbyRect.rect.width : 720f;
            var maxRowWidth = Mathf.Min(lobbyWidth - HorizontalPadding, 620f);
            var modeButtonWidth = Mathf.Floor((maxRowWidth - ButtonGap * 2f) / 3f);
            modeButtonWidth = Mathf.Clamp(modeButtonWidth, 120f, 190f);
            var halfStep = modeButtonWidth + ButtonGap;

            var root = new GameObject(RootName, typeof(RectTransform));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(lobbyRect, false);
            rootRect.SetAsLastSibling();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = Vector2.zero;

            var diffGo = CreateModeButton(
                rootRect,
                "Difficulty Button",
                new Vector2(0f, difficultyRowY),
                new Vector2(DifficultyButtonWidth, RowHeight),
                OnDifficultyClicked,
                buttonSprite,
                VXDifficultySelection.Selected.DisplayLabel());
            difficultyLabel = diffGo.GetComponentInChildren<TextMeshProUGUI>();

            CreateModeButton(
                rootRect,
                "Daily Challenge",
                new Vector2(-halfStep, modeRowY),
                new Vector2(modeButtonWidth, RowHeight),
                OnDailyScoredClicked,
                greenButtonSprite ?? buttonSprite,
                "Daily");

            CreateModeButton(
                rootRect,
                "Practice",
                new Vector2(0f, modeRowY),
                new Vector2(modeButtonWidth, RowHeight),
                OnDailyPracticeClicked,
                buttonSprite,
                "Practice");

            CreateModeButton(
                rootRect,
                "Endless",
                new Vector2(halfStep, modeRowY),
                new Vector2(modeButtonWidth, RowHeight),
                OnEndlessClicked,
                buttonSprite,
                "Endless");

            uiBuilt = true;
        }

        private void RefreshDifficultyLabel()
        {
            if (difficultyLabel != null)
            {
                difficultyLabel.text = VXDifficultySelection.Selected.DisplayLabel();
            }
        }

        private void OnDifficultyClicked()
        {
            VXDifficultySelection.CycleNext();
            RefreshDifficultyLabel();
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
            lobbyWindow.StartEndlessRun(VXDifficultySelection.Selected);
        }

        private GameObject CreateModeButton(
            RectTransform parent,
            string objectName,
            Vector2 anchoredPosition,
            Vector2 size,
            UnityEngine.Events.UnityAction onClick,
            Sprite sprite,
            string label)
        {
            var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

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
            textRect.offsetMin = new Vector2(6f, 2f);
            textRect.offsetMax = new Vector2(-6f, -2f);

            ApplyLabelStyle(textGo.GetComponent<TextMeshProUGUI>(), label);
            return go;
        }

        private void ApplyLabelStyle(TextMeshProUGUI tmp, string text)
        {
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = LabelFontSizeMin;
            tmp.fontSizeMax = LabelFontSize;
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
