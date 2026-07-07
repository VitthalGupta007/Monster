using UnityEngine;
using UnityEngine.Serialization;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Solar Magnifier Ability Data", menuName = "October/Abilities/Active/Solar Magnifier")]
    public class SolarMagnifierAbilityData : GenericAbilityData<SolarMagnifierAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.SolarMagnifier;
        }

        private void OnValidate()
        {
            type = AbilityType.SolarMagnifier;
        }
    }

    [System.Serializable]
    public class SolarMagnifierAbilityLevel: AbilityLevel
    {
        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Amount of time before sun ray disappears")]
        [SerializeField] float projectileLifetime;
        public float ProjectileLifetime => projectileLifetime;

        [Tooltip("Amount of time between firing sun rays in one attack")]
        [SerializeField] float timeBetweenProjectiles;
        public float TimeBetweenProjectiles => timeBetweenProjectiles;

        [Tooltip("Amount of sun rays spawned at the same time")]
        [SerializeField] int projectileCount;
        public int ProjectileCount => projectileCount;

        [Tooltip("Damage of the sun ray calculates as 'Damage = Player.Damage * DamagePerSecond'")]
        [FormerlySerializedAs("damage")]
        [SerializeField] float damagePerSecond;
        public float DamagePerSecond => damagePerSecond;

        [Tooltip("Enemy will take damage in this time per second")]
        [SerializeField] float damageRate;
        public float DamageRate => damageRate;

        [Tooltip("All enemies in additionalDamageRadius will take additionalDamagePerSecond")]
        [SerializeField] float additionalDamagePerSecond;
        public float AdditionalDamagePerSecond => additionalDamagePerSecond;

        [Tooltip("The radius enemies have to be in to get additional damage")]
        [SerializeField] float additionalDamageRadius;
        public float AdditionalDamageRadius => additionalDamageRadius;

        [Tooltip("Spead of the sun ray")]
        [SerializeField] float projectileSpeed;
        public float ProjectileSpeed => projectileSpeed;
    }
}