using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Boomerang Ability Data", menuName = "October/Abilities/Active/Boomerang")]
    public class BoomerangAbilityData : GenericAbilityData<BoomerangAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.Boomerang;
        }

        private void OnValidate()
        {
            type = AbilityType.Boomerang;
        }
    }

    [System.Serializable]
    public class BoomerangAbilityLevel : AbilityLevel
    {
        [Tooltip("Amount of boomerang spawned at the same time")]
        [SerializeField] int projectileCount;
        public int ProjectileCount => projectileCount;

        [Tooltip("Amount of time before boomerang disappears")]
        [SerializeField] float projectileLifetime;
        public float ProjectileLifetime => projectileLifetime;

        [Tooltip("Amount of time between firing boomerang in one attack")]
        [SerializeField] float timeBetweenProjectiles;
        public float TimeBetweenProjectiles => timeBetweenProjectiles;

        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Max distance the boomerang will travel from the player")]
        [SerializeField] float projectileTravelDistance;
        public float ProjectileTravelDistance => projectileTravelDistance;

        [Tooltip("Damage of the boomerang calculates as 'Damage = Player.Damage * damage'")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("Size of the boomerang")]
        [SerializeField] float projectileSize;
        public float ProjectileSize => projectileSize;
    }
}