using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Heal Endgame Ability Data", menuName = "October/Abilities/Passive/Heal Endgame")]
    public class HealEndgameAbilityData : GenericAbilityData<HealEndgameAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.HealEndgame;
        }

        private void OnValidate()
        {
            type = AbilityType.HealEndgame;
        }
    }

    [System.Serializable]
    public class HealEndgameAbilityLevel : AbilityLevel
    {
        [Tooltip("Heal the player by the persentage of their max hp")]
        [SerializeField, Range(0, 100)] int healPersentage = 40;
        public int HealPersentage => healPersentage;
    }
}