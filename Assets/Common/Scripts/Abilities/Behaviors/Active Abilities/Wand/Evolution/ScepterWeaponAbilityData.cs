using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Scepter Data", menuName = "October/Abilities/Evolution/Scepter")]
    public class ScepterWeaponAbilityData : GenericAbilityData<ScepterWeaponAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.AncientScepter;
            isWeaponAbility = true;
        }

        private void OnValidate()
        {
            type = AbilityType.AncientScepter;
            isWeaponAbility = true;
        }
    }

    [System.Serializable]
    public class ScepterWeaponAbilityLevel : AbilityLevel
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