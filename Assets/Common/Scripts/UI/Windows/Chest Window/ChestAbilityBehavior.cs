using OctoberStudio.Abilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoberStudio.UI.Chest
{
    public class ChestAbilityBehavior : MonoBehaviour
    {
        [SerializeField] Image abilityIcon;
        [SerializeField] TMP_Text abilityDescription;

        public void Init(AbilityData ability)
        {
            abilityIcon.sprite = ability.Icon;
            abilityDescription.text = ability.Title;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}