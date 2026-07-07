using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Meteor Ability Data", menuName = "October/Abilities/Evolution/Meteor")]
    public class MeteorAbilityData : GenericAbilityData<MeteorAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.Meteor;
        }

        private void OnValidate()
        {
            type = AbilityType.Meteor;
        }
    }

    [System.Serializable]
    public class MeteorAbilityLevel : AbilityLevel
    {
        [Tooltip("Amount of fireballs spawned at the same time")]
        [SerializeField] int projectilesCount;
        public int ProjectilesCount => projectilesCount;

        [Tooltip("Amount of time between firing meteors in one attack")]
        [SerializeField] float timeBetweenProjectiles;
        public float TimeBetweenProjectiles => timeBetweenProjectiles;

        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Damage of the fireball calculates as 'Damage = Player.Damage * Damage'")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("Inside this radius every enemy will be damaged")]
        [SerializeField] float explosionRadius;
        public float ExplosionRadius => explosionRadius;
    }
}