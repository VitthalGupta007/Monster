using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VXMonster.Core;
using VXMonster.Save;

namespace VXMonster.UI
{
    public class CodexWindowBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text bodyText;
        [SerializeField] Button backButton;

        private CodexSave codexSave;

        private void Awake()
        {
            if (GameController.SaveManager != null)
            {
                codexSave = GameController.SaveManager.GetSave<CodexSave>("VX Codex");
            }
        }

        public void Init(UnityAction onBackClicked)
        {
            if (backButton != null) backButton.onClick.AddListener(onBackClicked);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        private void Refresh()
        {
            if (bodyText == null || codexSave == null)
            {
                if (bodyText != null) bodyText.text = "Codex data loading...";
                return;
            }

            var relics = codexSave.DiscoveredRelicIds;
            var evolutions = codexSave.DiscoveredEvolutionIds;

            var text =
                "RELICS\n" +
                (relics.Count > 0 ? string.Join("\n", relics) : "None discovered yet.") +
                "\n\nEVOLUTIONS\n" +
                (evolutions.Count > 0 ? string.Join("\n", evolutions) : "None discovered yet.") +
                "\n\nCOMBOS\n" +
                "First combo discovered: " + (codexSave.HasDiscoveredFirstCombo ? "Yes" : "Not yet");

            bodyText.text = text;
        }
    }
}
