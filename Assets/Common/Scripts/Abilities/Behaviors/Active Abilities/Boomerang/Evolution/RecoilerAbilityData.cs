using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Recoiler Ability Data", menuName = "October/Abilities/Evolution/Recoiler")]
    public class RecoilerAbilityData : GenericAbilityData<RecoilerAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.Recoiler;
        }

        private void OnValidate()
        {
            type = AbilityType.Recoiler;
        }
    }

    [System.Serializable]
    public class RecoilerAbilityLevel : AbilityLevel
    {
        [Tooltip("Amount of time before recoiler disappears")]
        [SerializeField] float projectileLifetime;
        public float ProjectileLifetime => projectileLifetime;

        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Max distance the recoiler will travel from the player")]
        [SerializeField] float projectileTravelDistance;
        public float ProjectileTravelDistance => projectileTravelDistance;

        [Tooltip("Rotational speed of the recoiler")]
        [SerializeField] float angularSpeed;
        public float AngularSpeed => angularSpeed;

        [Tooltip("Damage of the recoiler calculates as 'Damage = Player.Damage * Damage'")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("Size of the recoiler")]
        [SerializeField] float projectileSize;
        public float ProjectileSize => projectileSize;
    }
}