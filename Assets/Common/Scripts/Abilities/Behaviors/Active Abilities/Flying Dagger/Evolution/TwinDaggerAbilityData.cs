using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Twin Daggers Ability Data", menuName = "October/Abilities/Evolution/Twin Daggers")]
    public class TwinDaggerAbilityData : GenericAbilityData<TwinDaggerAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.TwinDagger;
        }

        private void OnValidate()
        {
            type = AbilityType.TwinDagger;
        }
    }

    [System.Serializable]
    public class TwinDaggerAbilityLevel : AbilityLevel
    {
        [Tooltip("Amount of daggers spawned at the same time")]
        [SerializeField] int projectileCount;
        public int ProjectileCount => projectileCount;

        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Damage of the boomerang calculates as 'Damage = Player.Damage * Damage'")]
        [SerializeField] float damage;
        public float Damage => damage;
    }
}