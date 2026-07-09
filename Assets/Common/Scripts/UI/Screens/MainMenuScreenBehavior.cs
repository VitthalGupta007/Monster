using System;
using VXMonster.Core.Audio;
using VXMonster.Core.Easing;
using VXMonster.Core.Upgrades.UI;
using UnityEngine;
using UnityEngine.Events;

namespace VXMonster.Core.UI
{
    public class MainMenuScreenBehavior : MonoBehaviour
    {
        private Canvas canvas;

        [SerializeField] LobbyWindowBehavior lobbyWindow;
        [SerializeField] UpgradesWindowBehavior upgradesWindow;
        [SerializeField] SettingsWindowBehavior settingsWindow;
        [SerializeField] CharactersWindowBehavior charactersWindow;
        [SerializeField] VXMonster.UI.TalentTreeWindowBehavior talentTreeWindow;
        [SerializeField] VXMonster.UI.CodexWindowBehavior codexWindow;
        [SerializeField] VXMonster.UI.ShopWindowBehavior shopWindow;
        [SerializeField] VXMonster.UI.DifficultyModalWindowBehavior difficultyModal;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            // Each window must init independently — a Characters failure must not skip Talent/Codex/Shop.
            SafeInit("Lobby", () => lobbyWindow.Init(ShowUpgrades, ShowSettings, ShowCharacters, ShowTalentTree, ShowCodex, ShowShop));
            SafeInit("Upgrades", () => upgradesWindow.Init(HideUpgrades));
            SafeInit("Settings", () => settingsWindow.Init(HideSettings));
            SafeInit("Characters", () => charactersWindow.Init(HideCharacters));
            SafeInit("TalentTree", () => talentTreeWindow?.Init(HideTalentTree));
            SafeInit("Codex", () => codexWindow?.Init(HideCodex));
            SafeInit("Shop", () => shopWindow?.Init(HideShop));
        }

        private static void SafeInit(string label, Action init)
        {
            try
            {
                init?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MainMenu] {label} Init failed: {ex}");
            }
        }

        private void ShowUpgrades()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            lobbyWindow.Close();
            upgradesWindow.Open();
        }

        private void HideUpgrades()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            upgradesWindow.Close();
            lobbyWindow.Open();
        }

        private void ShowCharacters()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            lobbyWindow.Close();
            charactersWindow.Open();
        }

        private void HideCharacters()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            charactersWindow.Close();
            lobbyWindow.Open();
        }

        private void ShowSettings()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            lobbyWindow.Close();
            settingsWindow.Open();
        }

        private void HideSettings()
        {
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);

            settingsWindow.Close();
            lobbyWindow.Open();
        }

        private void ShowTalentTree()
        {
            if (talentTreeWindow == null) return;
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            lobbyWindow.Close();
            talentTreeWindow.Open();
        }

        public void HideTalentTree()
        {
            if (talentTreeWindow == null) return;
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            talentTreeWindow.Close();
            lobbyWindow.Open();
        }

        private void ShowCodex()
        {
            if (codexWindow == null) return;
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            lobbyWindow.Close();
            codexWindow.Open();
        }

        public void HideCodex()
        {
            if (codexWindow == null) return;
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            codexWindow.Close();
            lobbyWindow.Open();
        }

        private void ShowShop()
        {
            if (shopWindow == null) return;
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            lobbyWindow.Close();
            shopWindow.Open();
        }

        public void HideShop()
        {
            if (shopWindow == null) return;
            GameController.AudioManager.PlaySound(AudioManager.BUTTON_CLICK_HASH);
            shopWindow.Close();
            lobbyWindow.Open();
        }

        public void ShowDifficultyModal(System.Action onClosed = null)
        {
            if (difficultyModal == null) return;
            difficultyModal.Open(onClosed);
        }

        private void OnDestroy()
        {
            charactersWindow.Clear();
            upgradesWindow.Clear();
        }
    }
}