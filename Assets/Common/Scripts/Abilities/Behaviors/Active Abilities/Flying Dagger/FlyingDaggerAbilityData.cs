using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Flying Dagger Ability Data", menuName = "October/Abilities/Active/Flying Dagger")]
    public class FlyingDaggerAbilityData : GenericAbilityData<FlyingDaggerAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.FlyingDagger;
        }

        private void OnValidate()
        {
            type = AbilityType.FlyingDagger;
        }
    }

    [System.Serializable]
    public class FlyingDaggerAbilityLevel : AbilityLevel
    {
        [Tooltip("Amount of daggers spawned at the same time")]
        [SerializeField] int projectileCount;
        public int ProjectileCount => projectileCount;

        [Tooltip("Amount of time before dagger disappears")]
        [SerializeField] float projectileLifetime;
        public float ProjectileLifetime => projectileLifetime;

        [Tooltip("Amount of time between throwig daggers in one attack")]
        [SerializeField] float timeBetweenProjectiles;
        public float TimeBetweenProjectiles => timeBetweenProjectiles;

        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Force that will be applied to the dagger's rigidbody")]
        [SerializeField] float throwingForce;
        public float ThrowingForce => throwingForce;

        [Tooltip("How much will the throwing force deviate from the Vector2.up vector")]
        [SerializeField] float spread;
        public float Spread => spread;

        [Tooltip("Damage of the boomerang calculates as 'Damage = Player.Damage * Damage'")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("Size of the boomerang")]
        [SerializeField] float projectileSize;
        public float ProjectileSize => projectileSize;
    }
}