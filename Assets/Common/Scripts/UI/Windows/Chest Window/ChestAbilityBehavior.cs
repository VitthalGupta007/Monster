using VXMonster.Core.Abilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VXMonster.Core.UI.Chest
{
    public class ChestAbilityBehavior : MonoBehaviour
    {
        [SerializeField] Image abilityIcon;
        [SerializeField] TMP_Text abilityDescription;

        public void Init(AbilityData ability)
        {
            abilityIcon.sprite = ability.Icon;
            if (abilityDescription != null)
            {
                var desc = string.IsNullOrWhiteSpace(ability.Description) ? ability.Title : $"{ability.Title}\n{ability.Description}";
                abilityDescription.text = desc;
                abilityDescription.textWrappingMode = TextWrappingModes.Normal;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}