using UnityEngine;

namespace VXMonster.Core.Abilities
{
    [CreateAssetMenu(fileName = "Damage Ability Data", menuName = "VX Monster/Abilities/Passive/Damage")]
    public class DamageAbilityData : GenericAbilityData<DamageAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.Damage;
        }

        private void OnValidate()
        {
            type = AbilityType.Damage;
        }
    }

    [System.Serializable]
    public class DamageAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1f)] float damageMultiplier = 1f;
        public float DamageMultiplier => damageMultiplier;
    }
}