using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Lightning Amulet Ability Data", menuName = "October/Abilities/Active/Lightning Amulet")]
    public class LightningAmuletAbilityData : GenericAbilityData<LightningAmuletAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.LightningAmulet;
        }

        private void OnValidate()
        {
            type = AbilityType.LightningAmulet;
        }
    }

    [System.Serializable]
    public class LightningAmuletAbilityLevel : AbilityLevel
    {
        [Tooltip("This value is multiplied by the damage of the player")]
        [SerializeField, Min(0.1f)] float damage = 1f;
        public float Damage => damage;

        [Tooltip("This damage will be done to any enemy in radius")]
        [SerializeField, Min(0.1f)] float additionalDamage;
        public float AdditionalDamage => additionalDamage;

        [Tooltip("This is the radius in which every enemy will get the additional damage")]
        [SerializeField, Min(0.01f)] float additionalDamageRadius;
        public float AdditionalDamageRadius => additionalDamageRadius;

        [Tooltip("Amount of lightnings in one attack")]
        [SerializeField, Min(1)] int lightningsCount = 1;
        public int LightningsCount => lightningsCount;

        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Amount of time between lightnings")]
        [SerializeField] float durationBetweenHits;
        public float DurationBetweenHits => durationBetweenHits;
    }
}