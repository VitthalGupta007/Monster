using UnityEngine;

namespace VXMonster.Core.Abilities
{
    [CreateAssetMenu(fileName = "Max HP Ability Data", menuName = "VX Monster/Abilities/Passive/Max HP")]
    public class MaxHPAbilityData : GenericAbilityData<MaxHPAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.MaxHP;
        }

        private void OnValidate()
        {
            type = AbilityType.MaxHP;
        }
    }

    [System.Serializable]
    public class MaxHPAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1f)] float maxHPMultiplier = 1f;
        public float MaxHPMultiplier => maxHPMultiplier;
    }
}