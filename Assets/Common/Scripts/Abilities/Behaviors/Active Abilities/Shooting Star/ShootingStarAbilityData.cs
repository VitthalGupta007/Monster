using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Shooting Star Ability Data", menuName = "October/Abilities/Active/Shooting Star")]
    public class ShootingStarAbilityData : GenericAbilityData<ShootingStartAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.ShootingStar;
        }

        private void OnValidate()
        {
            type = AbilityType.ShootingStar;
        }
    }

    [System.Serializable]
    public class ShootingStartAbilityLevel: AbilityLevel
    {
        [Tooltip("This value is multiplied by the damage of the player")]
        [SerializeField, Min(0.1f)] float damage = 1f;
        public float Damage => damage;

        [Tooltip("Time between spawns of the projectiles")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Amount of time the projectiles are alive")]
        [SerializeField] float projectileLifetime;
        public float ProjectileLifetime => projectileLifetime;

        [Tooltip("Amount of projectiles alive at the same time")]
        [SerializeField] int projectilesCount = 2;
        public int ProjectilesCount => projectilesCount;

        [Tooltip("Distance between the player and the projectiles")]
        [SerializeField] float radius = 2.5f;
        public float Radius => radius;

        [Tooltip("Rotation spped of projectiles")]
        [SerializeField] float angularSpeed = -200;
        public float AngularSpeed => angularSpeed;
    }
}