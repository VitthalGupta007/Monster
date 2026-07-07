using OctoberStudio.Abilities;
using UnityEngine;

namespace OctoberStudio
{
    [CreateAssetMenu(fileName = "Damage Reduction Ability Data", menuName = "October/Abilities/Passive/Damage Reduction")]
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