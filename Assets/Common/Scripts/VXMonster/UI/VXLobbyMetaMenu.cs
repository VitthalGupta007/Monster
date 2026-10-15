using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VXMonster.Core;
using VXMonster.Core.UI;
using VXMonster.Gameplay;
using VXMonster.Platform;
using VXMonster.Platform.PlayGames;
using VXMonster.Save;
using VXMonster.Tutorial;

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
        [SerializeField] Button achievementsRowButton;
        [SerializeField] Button loginClaimRowButton;
        [SerializeField] Button closeRowButton;
        [SerializeField] VxMetaMenuInfoPopup infoPopup;
        [SerializeField] TMP_Text loginClaimLabel;
        [SerializeField] VxLoginClaimPopupBehavior loginClaimPopup;

        private bool wired;

        private void Start()
        {
            if (lobbyWindow == null)
            {
                lobbyWindow = FindAnyObjectByType<LobbyWindowBehavior>();
            }

            if (lobbyWindow == null) return;

            Wire();
            StartCoroutine(ShowDailyClaimPopupWhenReady());
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

            EnsureExtraRows();

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

#if UNITY_ANDROID
            WireLeaderboardsRow();
            WireAchievementsRow();
#else
            if (leaderboardsRowButton != null)
            {
                leaderboardsRowButton.gameObject.SetActive(false);
            }

            if (achievementsRowButton != null)
            {
                achievementsRowButton.gameObject.SetActive(false);
            }
#endif

            WireLoginClaimRow();
            EnsureLoginClaimPopup();
            wired = true;
        }

        private void EnsureLoginClaimPopup()
        {
            if (loginClaimPopup != null) return;

            var canvas = GetComponentInParent<Canvas>();
            var parent = canvas != null ? canvas.transform : transform;
            loginClaimPopup = VxLoginClaimPopupBehavior.Ensure(parent);
            loginClaimPopup?.SetOnClaimed(RefreshLoginClaimLabel);
        }

        private IEnumerator ShowDailyClaimPopupWhenReady()
        {
            // Wait one frame so continue-run popup can activate if a save exists.
            yield return null;
            yield return new WaitForSecondsRealtime(0.15f);

            const float timeoutSeconds = 8f;
            var elapsed = 0f;
            while (lobbyWindow != null && lobbyWindow.IsContinuePopupActive && elapsed < timeoutSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            EnsureLoginClaimPopup();
            loginClaimPopup?.TryShowIfUnclaimed();
        }

        private void EnsureExtraRows()
        {
            if (sheetRoot == null) return;

            if (achievementsRowButton == null && leaderboardsRowButton != null)
            {
                achievementsRowButton = CloneRow(leaderboardsRowButton, "Achievements Row", "ACHIEVEMENTS");
            }

            if (loginClaimRowButton == null && shopRowButton != null)
            {
                loginClaimRowButton = CloneRow(shopRowButton, "Login Claim Row", "DAILY CLAIM");
                loginClaimLabel = loginClaimRowButton != null
                    ? loginClaimRowButton.GetComponentInChildren<TMP_Text>()
                    : null;
            }
        }

        private Button CloneRow(Button template, string name, string label)
        {
            if (template == null) return null;
            var clone = Object.Instantiate(template.gameObject, template.transform.parent);
            clone.name = name;
            var rect = clone.GetComponent<RectTransform>();
            var templateRect = template.GetComponent<RectTransform>();
            if (rect != null && templateRect != null)
            {
                rect.anchoredPosition = templateRect.anchoredPosition + new Vector2(0f, -72f);
            }

            // Keep CLOSE as the last row in the sheet.
            if (closeRowButton != null)
            {
                clone.transform.SetSiblingIndex(closeRowButton.transform.GetSiblingIndex());
            }

            var tmp = clone.GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = label;
            return clone.GetComponent<Button>();
        }

        private void WireLeaderboardsRow()
        {
            if (leaderboardsRowButton == null) return;

            leaderboardsRowButton.gameObject.SetActive(true);
            leaderboardsRowButton.onClick.RemoveAllListeners();
            leaderboardsRowButton.onClick.AddListener(OnLeaderboardsClicked);
        }

        private void WireAchievementsRow()
        {
            if (achievementsRowButton == null) return;

            achievementsRowButton.gameObject.SetActive(true);
            achievementsRowButton.onClick.RemoveAllListeners();
            achievementsRowButton.onClick.AddListener(OnAchievementsClicked);
        }

        private void WireLoginClaimRow()
        {
            if (loginClaimRowButton == null) return;

            loginClaimRowButton.gameObject.SetActive(true);
            loginClaimRowButton.onClick.RemoveAllListeners();
            loginClaimRowButton.onClick.AddListener(OnLoginClaimClicked);
            RefreshLoginClaimLabel();
        }

        private void RefreshLoginClaimLabel()
        {
            if (loginClaimLabel == null || GameController.SaveManager == null) return;

            var save = GameController.SaveManager.GetSave<LoginClaimSave>(LoginClaimSave.SaveKey);
            var dateKey = GameSessionManager.GetUtcDateKey();
            if (save != null && save.HasClaimedToday(dateKey))
            {
                loginClaimLabel.text = $"CLAIMED (streak {save.LoginStreak})";
                if (loginClaimRowButton != null) loginClaimRowButton.interactable = false;
            }
            else
            {
                var gold = LoginClaimUtility.PreviewGold(save, dateKey);
                loginClaimLabel.text = $"DAILY CLAIM (+{gold}g)";
                if (loginClaimRowButton != null) loginClaimRowButton.interactable = true;
            }
        }

        private void OnLoginClaimClicked()
        {
            if (!LoginClaimUtility.TryClaim(out var gold, out var streak))
            {
                VxToastBehavior.Show("Already claimed today.");
                RefreshLoginClaimLabel();
                return;
            }

            VxToastBehavior.Show($"Login claim: +{gold} gold (streak {streak})");
            RefreshLoginClaimLabel();
            infoPopup?.Hide();
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

        private void OnAchievementsClicked()
        {
            var playGames = PlatformServices.PlayGames;
            if (playGames == null) return;

            if (playGames.IsAuthenticated)
            {
                playGames.ShowAchievements();
                return;
            }

            playGames.SignIn(success =>
            {
                if (success) playGames.ShowAchievements();
            });
        }

        private void OpenSheet()
        {
            if (sheetRoot == null) return;
            sheetRoot.SetActive(true);
            sheetRoot.transform.SetAsLastSibling();
            infoPopup?.Hide();
            RefreshLoginClaimLabel();
        }

        private void CloseSheet()
        {
            infoPopup?.Hide();
            if (sheetRoot != null) sheetRoot.SetActive(false);
        }
    }
}
