using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Lunar Projector Ability Data", menuName = "October/Abilities/Evolution/Lunar Projector")]
    public class LunarProjectorAbilityData : GenericAbilityData<LunarProjectorAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.LunarProjector;
        }

        private void OnValidate()
        {
            type = AbilityType.LunarProjector;
        }
    }

    [System.Serializable]
    public class LunarProjectorAbilityLevel : AbilityLevel
    {
        [Tooltip("Amount of rays spawned at the same time")]
        [SerializeField] int projectileCount;
        public int ProjectileCount => projectileCount;

        [Tooltip("Amount of time before rays disappear")]
        [SerializeField] float projectileLifetime;
        public float ProjectileLifetime => projectileLifetime;

        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Max distance the rays will travel to the player")]
        [SerializeField] float initialRadius;
        public float InitialRadius => initialRadius;

        [Tooltip("Rotational speed of the recoiler")]
        [SerializeField] float angularSpeed;
        public float AngularSpeed => angularSpeed;

        [Tooltip("Damage of the ray calculates as 'Damage = Player.Damage * damageMultiplier'")]
        [SerializeField] float damage;
        public float Damage => damage;
    }
}