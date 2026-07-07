using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Ice Shard Data", menuName = "October/Abilities/Active/Ice Shard")]
    public class IceShardAbilityData : GenericAbilityData<IceShardAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.IceShard;
        }

        private void OnValidate()
        {
            type = AbilityType.IceShard;
        }
    }

    [System.Serializable]
    public class IceShardAbilityLevel : AbilityLevel
    {
        [Tooltip("Ice shards spawn every 'AbilityCooldown' seconds")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Speed of ice shards")]
        [SerializeField] float projectileSpeed;
        public float ProjectileSpeed => projectileSpeed;

        [Tooltip("Damage of ice shards calculates like this: Player.Damage * Damage")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("Size of ice shards")]
        [SerializeField] float projectileSize;
        public float ProjectileSize => projectileSize;

        [Tooltip("Amount of ice shards")]
        [SerializeField] int projectilesCount;
        public int ProjectilesCount => projectilesCount;

        [Tooltip("Time ice shards stay alive")]
        [SerializeField] float projectileLifetime;
        public float ProjectileLifetime => projectileLifetime;
    }
}