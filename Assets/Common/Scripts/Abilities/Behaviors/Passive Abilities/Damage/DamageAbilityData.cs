using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Damage Ability Data", menuName = "October/Abilities/Passive/Damage")]
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