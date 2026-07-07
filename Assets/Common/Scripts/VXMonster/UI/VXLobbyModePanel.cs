using OctoberStudio;
using OctoberStudio.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.UI
{
    /// <summary>
    /// Adds Daily Challenge and Endless mode buttons to the lobby at runtime.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class VXLobbyModePanel : MonoBehaviour
    {
        [SerializeField] LobbyWindowBehavior lobbyWindow;
        [SerializeField] Sprite buttonSprite;
        [SerializeField] Sprite greenButtonSprite;

        private DailyChallengeSave dailySave;

        private void Start()
        {
            if (lobbyWindow == null)
            {
                lobbyWindow = FindAnyObjectByType<LobbyWindowBehavior>();
            }

            if (lobbyWindow == null) return;

            if (GameController.SaveManager != null)
            {
                dailySave = GameController.SaveManager.GetSave<DailyChallengeSave>("VX Daily Challenge");
            }

            var playTransform = FindDeepChild(lobbyWindow.transform, "Play Button");
            if (playTransform == null) return;

            var playButton = playTransform.GetComponent<Button>();
            var playRect = playTransform.GetComponent<RectTransform>();
            if (playButton == null || playRect == null) return;

            if (buttonSprite == null)
            {
                buttonSprite = playButton.image.sprite;
            }

            var root = new GameObject("VX Mode Buttons", typeof(RectTransform));
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(playRect.parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0f);
            rootRect.anchorMax = new Vector2(0.5f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.anchoredPosition = new Vector2(0f, playRect.anchoredPosition.y + 170f);
            rootRect.sizeDelta = new Vector2(420f, 150f);

            CreateModeButton(rootRect, "Daily (Scored)", new Vector2(-140f, 75f), OnDailyScoredClicked, greenButtonSprite ?? buttonSprite);
            CreateModeButton(rootRect, "Daily Practice", new Vector2(140f, 75f), OnDailyPracticeClicked, buttonSprite);
            CreateModeButton(rootRect, "Endless", new Vector2(0f, 0f), OnEndlessClicked, buttonSprite);
        }

        private void OnDailyScoredClicked()
        {
            if (dailySave != null && dailySave.HasScoredToday(GameSessionManager.GetUtcDateKey()))
            {
                Debug.Log("[VX] Daily scored attempt already used today — starting practice instead.");
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
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(260f, 70f);

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
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 26f;
            tmp.color = Color.white;
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
