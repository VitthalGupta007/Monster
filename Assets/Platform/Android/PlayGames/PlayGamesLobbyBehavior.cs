using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core.UI;
using VXMonster.Platform;
using VXMonster.Platform.PlayGames;

namespace VXMonster.UI
{
    /// <summary>
    /// Lobby affordances for Google Play Games sign-in and leaderboards.
    /// Injects rows into the meta menu sheet without duplicating Talent/Codex/Shop entries.
    /// </summary>
    [DefaultExecutionOrder(115)]
    public class PlayGamesLobbyBehavior : MonoBehaviour
    {
        private const string SheetName = "VX Meta Menu Sheet";
        private const string PanelName = "Panel";
        private const string CloseRowName = "Close Row";

        private TextMeshProUGUI signInLabel;
        private bool injected;

        private void Start()
        {
            StartCoroutine(InjectWhenReady());
        }

        private System.Collections.IEnumerator InjectWhenReady()
        {
            for (var i = 0; i < 30 && !injected; i++)
            {
                if (TryInjectRows())
                {
                    injected = true;
                    yield break;
                }

                yield return null;
            }
        }

        private void Update()
        {
            RefreshSignInLabel();
        }

        private bool TryInjectRows()
        {
            var lobby = FindAnyObjectByType<LobbyWindowBehavior>();
            if (lobby == null) return false;

            var sheet = lobby.transform.Find(SheetName);
            if (sheet == null) return false;

            var panel = sheet.Find(PanelName) as RectTransform;
            if (panel == null) return false;

            if (panel.Find("Play Games Sign In Row") != null)
            {
                signInLabel = panel.Find("Play Games Sign In Row/Label")?.GetComponent<TextMeshProUGUI>();
                return true;
            }

            var closeRow = panel.Find(CloseRowName);
            var insertIndex = closeRow != null ? closeRow.GetSiblingIndex() : panel.childCount;

            var buttonSprite = ResolveButtonSprite(panel);
            var labelFont = ResolveLabelFont(panel);

            var signInRow = CreateSheetRow(panel, "Play Games Sign In Row", "SIGN IN TO PLAY", buttonSprite, labelFont, OnSignInClicked);
            signInRow.SetSiblingIndex(insertIndex++);
            signInLabel = signInRow.Find("Label")?.GetComponent<TextMeshProUGUI>();

            var leaderboardRow = CreateSheetRow(panel, "Play Games Leaderboard Row", "LEADERBOARDS", buttonSprite, labelFont, OnLeaderboardsClicked);
            leaderboardRow.SetSiblingIndex(insertIndex);

            RefreshSignInLabel();
            return true;
        }

        private void RefreshSignInLabel()
        {
            if (signInLabel == null || PlayGames == null) return;

            signInLabel.text = PlayGames.IsAuthenticated ? "ACHIEVEMENTS" : "SIGN IN TO PLAY";
        }

        private void OnSignInClicked()
        {
            if (PlayGames == null) return;

            if (PlayGames.IsAuthenticated)
            {
                PlayGames.ShowAchievements();
                return;
            }

            PlayGames.SignIn(_ => RefreshSignInLabel());
        }

        private void OnLeaderboardsClicked()
        {
            PlayGames?.ShowLeaderboard();
        }

        private static IPlayGamesService PlayGames => PlatformServices.PlayGames;

        private static Sprite ResolveButtonSprite(RectTransform panel)
        {
            var closeRow = panel.Find(CloseRowName);
            if (closeRow != null && closeRow.TryGetComponent<Image>(out var image))
            {
                return image.sprite;
            }

            return null;
        }

        private static TMP_FontAsset ResolveLabelFont(RectTransform panel)
        {
            var closeRow = panel.Find(CloseRowName);
            if (closeRow != null)
            {
                var label = closeRow.Find("Label");
                if (label != null && label.TryGetComponent<TextMeshProUGUI>(out var tmp))
                {
                    return tmp.font;
                }
            }

            return null;
        }

        private static RectTransform CreateSheetRow(
            RectTransform parent,
            string name,
            string label,
            Sprite buttonSprite,
            TMP_FontAsset labelFont,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(480f, 80f);

            var layout = go.GetComponent<LayoutElement>();
            layout.preferredHeight = 80f;
            layout.minHeight = 80f;
            layout.flexibleWidth = 1f;

            var image = go.GetComponent<Image>();
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(0.42f, 0.25f, 0.63f, 1f);
            image.pixelsPerUnitMultiplier = 2f;

            var button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(onClick);

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.SetParent(rt, false);
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 32f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            if (labelFont != null) tmp.font = labelFont;

            return rt;
        }
    }
}
