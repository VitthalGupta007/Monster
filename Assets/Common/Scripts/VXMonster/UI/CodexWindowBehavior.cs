using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VXMonster.Core;
using VXMonster.Core.Abilities;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.UI
{
    public class CodexWindowBehavior : MonoBehaviour
    {
        [SerializeField] TMP_Text bodyText;
        [SerializeField] Button backButton;
        [SerializeField] AbilitiesDatabase abilitiesDatabase;

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

            var sb = new StringBuilder();
            sb.AppendLine("RELICS");
            var relics = codexSave.DiscoveredRelicIds;
            if (relics.Count == 0)
            {
                sb.AppendLine("None discovered yet.");
            }
            else
            {
                var manager = RelicsManager.Instance;
                for (var i = 0; i < relics.Count; i++)
                {
                    var relic = manager != null ? manager.GetRelicById(relics[i]) : null;
                    if (relic != null)
                    {
                        sb.AppendLine($"• {relic.DisplayName}");
                        sb.AppendLine($"  {PlayerFacingCopy.FormatRelic(relic)}");
                    }
                    else
                    {
                        sb.AppendLine($"• {relics[i]}");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("EVOLUTIONS");
            var evolutions = codexSave.DiscoveredEvolutionIds;
            if (evolutions.Count == 0)
            {
                sb.AppendLine("None discovered yet.");
            }
            else
            {
                for (var i = 0; i < evolutions.Count; i++)
                {
                    sb.AppendLine("• " + ResolveEvolutionTitle(evolutions[i]));
                }
            }

            sb.AppendLine();
            sb.AppendLine("COMBOS");
            sb.AppendLine(codexSave.HasDiscoveredFirstCombo
                ? "First combo discovered: Yes (+15 gold on first discovery)"
                : "First combo discovered: Not yet (+15 gold when you trigger one)");
            sb.AppendLine();
            sb.AppendLine("Discovery rewards: Relic +25 gold · Evolution +40 gold · Scholar's Eye +15 on relics");

            bodyText.text = sb.ToString();
            bodyText.textWrappingMode = TextWrappingModes.Normal;
        }

        private string ResolveEvolutionTitle(string evolutionId)
        {
            if (string.IsNullOrEmpty(evolutionId)) return "Unknown";

            if (System.Enum.TryParse(evolutionId, out AbilityType abilityType))
            {
                var data = abilitiesDatabase != null ? abilitiesDatabase.GetAbility(abilityType) : null;
                if (data == null && StageController.AbilityManager != null)
                {
                    data = StageController.AbilityManager.GetAbilityData(abilityType);
                }

                if (data != null && !string.IsNullOrEmpty(data.Title)) return data.Title;
                return abilityType.ToString();
            }

            return evolutionId;
        }
    }
}
