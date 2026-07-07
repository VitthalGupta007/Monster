using OctoberStudio.Audio;
using OctoberStudio.Easing;
using OctoberStudio.Upgrades.UI;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.UI
{
    public class MainMenuScreenBehavior : MonoBehaviour
    {
        private Canvas canvas;

        [SerializeField] LobbyWindowBehavior lobbyWindow;
        [SerializeField] UpgradesWindowBehavior upgradesWindow;
        [SerializeField] SettingsWindowBehavior settingsWindow;
        [SerializeField] CharactersWindowBehavior charactersWindow;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            lobbyWindow.Init(ShowUpgrades, ShowSettings, ShowCharacters);
            upgradesWindow.Init(HideUpgrades);
            settingsWindow.Init(HideSettings);
            charactersWindow.Init(HideCharacters);
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

        private void OnDestroy()
        {
            charactersWindow.Clear();
            upgradesWindow.Clear();
        }
    }
}