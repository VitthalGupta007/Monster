using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Silver Stakes Data", menuName = "October/Abilities/Evolution/Silver Stakes")]
    public class SilverStakesAbilityData : GenericAbilityData<SilverStakesAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.SilverStakes;
        }

        private void OnValidate()
        {
            type = AbilityType.SilverStakes;
        }
    }

    [System.Serializable]
    public class SilverStakesAbilityLevel : AbilityLevel
    {
        [Tooltip("Speed of silver shards")]
        [SerializeField] float projectileSpeed;
        public float ProjectileSpeed => projectileSpeed;

        [Tooltip("Damage of silver shards calculates like this: Player.Damage * Damage")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("Size of silver shards")]
        [SerializeField] float projectileSize;
        public float ProjectileSize => projectileSize;

        [Tooltip("Amount of silver shards")]
        [SerializeField] int projectilesCount;
        public int ProjectilesCount => projectilesCount;
    }
}