using TMPro;
using UnityEngine;
using VXMonster.Core;
using VXMonster.Save;

namespace VXMonster.UI
{
    public class CodexWindowBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text bodyText;

        private CodexSave codexSave;

        private void Awake()
        {
            if (GameController.SaveManager != null)
            {
                codexSave = GameController.SaveManager.GetSave<CodexSave>("VX Codex");
            }
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

            var text = "Discovered:\n";
            var relics = codexSave.DiscoveredRelicIds;
            text += relics.Count > 0 ? string.Join(", ", relics) : "(none yet)";
            text += "\n\nEvolutions:\n";
            var evolutions = codexSave.DiscoveredEvolutionIds;
            text += evolutions.Count > 0 ? string.Join(", ", evolutions) : "(none yet)";
            text += "\n\nFirst combo discovered: " + (codexSave.HasDiscoveredFirstCombo ? "Yes" : "No");

            bodyText.text = text;
        }
    }
}
