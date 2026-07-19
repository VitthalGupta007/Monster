using UnityEngine;
using UnityEngine.UI;
using VXMonster.Core.UI;
using VXMonster.Platform;
using VXMonster.Platform.PlayGames;

namespace VXMonster.UI
{
    /// <summary>
    /// Corner MENU sheet — classic purple rows from Lobby Window.prefab.
    /// Wire via VX Monster → Wire Lobby Hub.
    /// </summary>
    [DefaultExecutionOrder(110)]
    public class VXLobbyMetaMenu : MonoBehaviour
    {
        [SerializeField] LobbyWindowBehavior lobbyWindow;
        [SerializeField] Button menuButton;
        [SerializeField] GameObject sheetRoot;
        [SerializeField] Button talentRowButton;
        [SerializeField] Button talentInfoButton;
        [SerializeField] Button codexRowButton;
        [SerializeField] Button shopRowButton;
        [SerializeField] Button leaderboardsRowButton;
        [SerializeField] Button closeRowButton;
        [SerializeField] VxMetaMenuInfoPopup infoPopup;

        private bool wired;

        private void Start()
        {
            if (lobbyWindow == null)
            {
                lobbyWindow = FindAnyObjectByType<LobbyWindowBehavior>();
            }

            if (lobbyWindow == null) return;

            Wire();
        }

        private void Wire()
        {
            if (wired) return;

            if (menuButton != null)
            {
                menuButton.onClick.RemoveListener(OpenSheet);
                menuButton.onClick.AddListener(OpenSheet);
            }

            WireRow(talentRowButton, () =>
            {
                CloseSheet();
                lobbyWindow.OpenTalentMenu();
            });

            WireRow(codexRowButton, () =>
            {
                CloseSheet();
                lobbyWindow.OpenCodexMenu();
            });

            WireRow(shopRowButton, () =>
            {
                CloseSheet();
                lobbyWindow.OpenShopMenu();
            });

            WireRow(closeRowButton, CloseSheet);

            if (talentInfoButton != null && infoPopup != null)
            {
                talentInfoButton.onClick.RemoveAllListeners();
                talentInfoButton.onClick.AddListener(() =>
                {
                    infoPopup.Show(
                        "Talent Tree",
                        "Spend talent points earned from bosses on permanent upgrades between runs.");
                });
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            WireLeaderboardsRow();
#else
            if (leaderboardsRowButton != null)
            {
                leaderboardsRowButton.gameObject.SetActive(false);
            }
#endif

            wired = true;
        }

        private void WireLeaderboardsRow()
        {
            if (leaderboardsRowButton == null) return;

            leaderboardsRowButton.gameObject.SetActive(true);
            leaderboardsRowButton.onClick.RemoveAllListeners();
            leaderboardsRowButton.onClick.AddListener(OnLeaderboardsClicked);
        }

        private static void WireRow(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void OnLeaderboardsClicked()
        {
            var playGames = PlatformServices.PlayGames;
            if (playGames == null) return;

            if (playGames.IsAuthenticated)
            {
                playGames.ShowLeaderboard();
                return;
            }

            playGames.SignIn(success =>
            {
                if (success) playGames.ShowLeaderboard();
            });
        }

        private void OpenSheet()
        {
            if (sheetRoot == null) return;
            sheetRoot.SetActive(true);
            sheetRoot.transform.SetAsLastSibling();
            infoPopup?.Hide();
        }

        private void CloseSheet()
        {
            infoPopup?.Hide();
            if (sheetRoot != null) sheetRoot.SetActive(false);
        }
    }
}
