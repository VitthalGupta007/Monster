using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Fireball Ability Data", menuName = "October/Abilities/Active/Fireball")]
    public class FireballAbilityData : GenericAbilityData<FireballAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.Fireball;
        }

        private void OnValidate()
        {
            type = AbilityType.Fireball;
        }
    }

    [System.Serializable]
    public class FireballAbilityLevel : AbilityLevel
    {
        [Tooltip("Amount of fireballs spawned at the same time")]
        [SerializeField] int projectilesCount;
        public int ProjectilesCount => projectilesCount;

        [Tooltip("Amount of time between firing fireballs in one attack")]
        [SerializeField] float timeBetweenFireballs;
        public float TimeBetweenFireballs => timeBetweenFireballs;

        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Amount of time before fireball self destructs")]
        [SerializeField] float fireballLifetime;
        public float FireballLifetime => fireballLifetime;

        [Tooltip("Fireball speed")]
        [SerializeField] float speed;
        public float Speed => speed;

        [Tooltip("Damage of the fireball calculates as 'Damage = Player.Damage * Damage'")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("Scale of the fireball's transform")]
        [SerializeField] float projectileSize;
        public float ProjectileSize => projectileSize;

        [Tooltip("Inside this radius every enemy will be damaged")]
        [SerializeField] float explosionRadius;
        public float ExplosionRadius => explosionRadius;
    }
}