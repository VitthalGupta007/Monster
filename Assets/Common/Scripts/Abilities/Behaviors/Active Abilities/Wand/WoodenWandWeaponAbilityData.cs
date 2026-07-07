using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Wooden Wand Data", menuName = "October/Abilities/Active/Wooden Wand")]
    public class WoodenWandWeaponAbilityData : GenericAbilityData<WoodenWandWeaponAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.WoodenWand;
            isWeaponAbility = true;
        }

        private void OnValidate()
        {
            type = AbilityType.WoodenWand;
            isWeaponAbility = true;
        }
    }

    [System.Serializable]
    public class WoodenWandWeaponAbilityLevel : AbilityLevel
    {
        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Speed of the projectile")]
        [SerializeField] float projectileSpeed;
        public float ProjectileSpeed => projectileSpeed;

        [Tooltip("Damage of projectiles calculates like this: Player.Damage * Damage")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("Size of projectiles")]
        [SerializeField] float projectileSize;
        public float ProjectileSize => projectileSize;

        [Tooltip("Time projectiles stay alive")]
        [SerializeField] float projectileLifetime;
        public float ProjectileLifetime => projectileLifetime;
    }
}