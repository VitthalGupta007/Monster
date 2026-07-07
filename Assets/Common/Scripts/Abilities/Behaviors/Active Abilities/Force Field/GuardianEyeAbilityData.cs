using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Guardian Eye Ability Data", menuName = "October/Abilities/Active/Guardian Eye")]
    public class GuardianEyeAbilityData : GenericAbilityData<GuardianEyeAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.GuardianEye;
        }

        private void OnValidate()
        {
            type = AbilityType.GuardianEye;
        }
    }

    [System.Serializable]
    public class GuardianEyeAbilityLevel : AbilityLevel
    {
        [Tooltip("The size of the forcefield")]
        [SerializeField, Min(0.1f)] float fieldRadius = 0.5f;
        public float FieldRadius => fieldRadius;

        [Tooltip("The damage of the forceField. Multiplier by Player damage.")]
        [SerializeField, Min(0.1f)] float damage = 1f;
        public float Damage => damage;

        [Tooltip("Enemy will take damage every 'damageCooldown' seconds")]
        [SerializeField, Min(0f)] float damageCooldown;
        public float DamageCooldown => damageCooldown;
    }
}