using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Spiky Trap Ability Data", menuName = "October/Abilities/Evolution/Spiky Trap")]
    public class SpikyTrapAbilityData : GenericAbilityData<SpikyTrapAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.SpikyTrap;
        }

        private void OnValidate()
        {
            type = AbilityType.SpikyTrap;
        }
    }

    [System.Serializable]
    public class SpikyTrapAbilityLevel : AbilityLevel
    {
        [Tooltip("Amount of mines that are spawned at the time")]
        [SerializeField, Min(1f)] int minesCount = 1;
        public int MinesCount => minesCount;

        [Tooltip("Amount of spikes that shppt after an explosion")]
        [SerializeField, Min(1f)] int spikesCount = 6;
        public int SpikesCount => spikesCount;

        [Tooltip("Amount of time between spawning mines")]
        [SerializeField, Min(0.01f)] float abilityCooldown = 7;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Distance between the player and the mine spawn position")]
        [SerializeField, Min(0)] float mineSpawnRadius;
        public float MineSpawnRadius => mineSpawnRadius;

        [Tooltip("Amount of time before mine selfdestructs")]
        [SerializeField, Min(0.01f)] float mineLifetime = 10;
        public float MineLifetime => mineLifetime;

        [Tooltip("Damage of the mine calculates as 'Damage = Player.Damage * Damage'")]
        [SerializeField, Min(0.01f)] float damage = 10;
        public float Damage => damage;

        [Tooltip("Damage of the spike calculates as 'Damage = Player.Damage * SpikeDamage'")]
        [SerializeField, Min(0.01f)] float spikeDamage = 10;
        public float SpikeDamage => spikeDamage;

        [Tooltip("Scale of the mine's transform")]
        [SerializeField, Min(0.1f)] float mineSize = 1;
        public float MineSize => mineSize;

        [Tooltip("Max distance between the mine and the enemy that detonated the mine")]
        [SerializeField, Min(0.1f)] float mineTriggerRadius = 1;
        public float MineTriggerRadius => mineTriggerRadius;

        [Tooltip("Max distance between the mine and the enemy for the latter to be damaged")]
        [SerializeField, Min(0.1f)] float mineDamageRadius = 1;
        public float MineDamageRadius => mineDamageRadius;
    }
}