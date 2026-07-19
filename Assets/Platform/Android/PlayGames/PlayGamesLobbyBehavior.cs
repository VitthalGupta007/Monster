using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core.UI;
using VXMonster.Platform;
using VXMonster.Platform.PlayGames;

namespace VXMonster.UI
{
    /// <summary>
    /// Injects a single Leaderboards row into the meta menu sheet.
    /// Layout stays dynamic — the panel grows for the extra row.
    /// If the player is not signed in, Leaderboards triggers Play Games sign-in first.
    /// </summary>
    [DefaultExecutionOrder(115)]
    public class PlayGamesLobbyBehavior : MonoBehaviour
    {
        private const string SheetName = "VX Meta Menu Sheet";
        private const string PanelName = "Panel";
        private const string CloseRowName = "Close Row";
        private const string LeaderboardRowName = "Play Games Leaderboard Row";
        private const string LegacySignInRowName = "Play Games Sign In Row";

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

        private bool TryInjectRows()
        {
            var lobby = FindAnyObjectByType<LobbyWindowBehavior>();
            if (lobby == null) return false;

            var sheet = lobby.transform.Find(SheetName);
            if (sheet == null) return false;

            var panel = sheet.Find(PanelName) as RectTransform;
            if (panel == null) return false;

            var legacySignIn = panel.Find(LegacySignInRowName);
            if (legacySignIn != null)
            {
                Destroy(legacySignIn.gameObject);
            }

            if (panel.Find(LeaderboardRowName) != null)
            {
                FitPanelToContents(panel);
                return true;
            }

            var closeRow = panel.Find(CloseRowName);
            var insertIndex = closeRow != null ? closeRow.GetSiblingIndex() : panel.childCount;

            var buttonSprite = ResolveButtonSprite(panel);
            var labelFont = ResolveLabelFont(panel);

            var leaderboardRow = CreateSheetRow(
                panel,
                LeaderboardRowName,
                "LEADERBOARDS",
                buttonSprite,
                labelFont,
                OnLeaderboardsClicked);
            leaderboardRow.SetSiblingIndex(insertIndex);

            FitPanelToContents(panel);
            return true;
        }

        private static void FitPanelToContents(RectTransform panel)
        {
            var fitter = panel.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

            var size = panel.sizeDelta;
            if (size.x < 400f)
            {
                size.x = 560f;
                panel.sizeDelta = size;
            }
        }

        private void OnLeaderboardsClicked()
        {
            if (PlayGames == null) return;

            if (PlayGames.IsAuthenticated)
            {
                PlayGames.ShowLeaderboard();
                return;
            }

            PlayGames.SignIn(success =>
            {
                if (success) PlayGames.ShowLeaderboard();
            });
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
            const float rowHeight = 96f;
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.sizeDelta = new Vector2(480f, rowHeight);

            var layout = go.GetComponent<LayoutElement>();
            layout.preferredHeight = rowHeight;
            layout.minHeight = rowHeight;
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
            tmp.fontSize = 36f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.raycastTarget = false;
            if (labelFont != null) tmp.font = labelFont;

            return rt;
        }
    }
}
