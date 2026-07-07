using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Time Gazer Ability Data", menuName = "October/Abilities/Evolution/Time Gazer")]
    public class TimeGazerAbilityData : GenericAbilityData<TimeGazerAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.TimeGazer;
        }

        private void OnValidate()
        {
            type = AbilityType.TimeGazer;
        }
    }

    [System.Serializable]
    public class TimeGazerAbilityLevel : AbilityLevel
    {
        [Tooltip("The size of the timezone")]
        [SerializeField, Min(0.1f)] float fieldRadius = 0.5f;
        public float FieldRadius => fieldRadius;

        [Tooltip("Enemies inside timezone will move slower")]
        [SerializeField, Min(0.1f)] float slowDownMultiplier = 0.5f;
        public float SlowDownMultiplier => slowDownMultiplier;

        [Tooltip("The damage of the timezone. Multiplier by Player damage.")]
        [SerializeField, Min(0.1f)] float damage = 1f;
        public float Damage => damage;

        [Tooltip("Enemy will take damage every 'damageCooldown' seconds")]
        [SerializeField, Min(0f)] float damageCooldown;
        public float DamageCooldown => damageCooldown;
    }
}