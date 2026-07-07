using VXMonster.Core.Abilities;
using UnityEngine;

namespace VXMonster.Core
{
    [CreateAssetMenu(fileName = "Damage Reduction Ability Data", menuName = "VX Monster/Abilities/Passive/Damage Reduction")]
    public class DamageReductionAbilityData : GenericAbilityData<DamageReductionAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.DamageReduction;
        }

        private void OnValidate()
        {
            type = AbilityType.DamageReduction;
        }
    }

    [System.Serializable]
    public class DamageReductionAbilityLevel : AbilityLevel
    {
        [SerializeField, Range(1, 99)] int damageReductionPercent = 10;
        public int DamageReductionPercent => damageReductionPercent;
    }
}