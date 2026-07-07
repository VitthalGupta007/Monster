using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Thunder Ring Ability Data", menuName = "October/Abilities/Evolution/Thunder Ring")]
    public class ThunderRingAbilityData : GenericAbilityData<ThunderRingAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.ThunderRing;
        }

        private void OnValidate()
        {
            type = AbilityType.ThunderRing;
        }
    }

    [System.Serializable]
    public class ThunderRingAbilityLevel : AbilityLevel
    {
        [Tooltip("This value is multiplied by the damage of the player")]
        [SerializeField, Min(0.1f)] float damage = 1f;
        public float Damage => damage;

        [Tooltip("The damage multiplier of the secondary projhectiles")]
        [SerializeField, Min(0.1f)] float ballDamage = 1f;
        public float BallDamage => ballDamage;

        [Tooltip("Amount of lightnings in one attack")]
        [SerializeField, Min(1)] int lightningsCount = 1;
        public int LightningsCount => lightningsCount;

        [Tooltip("Amount of additional projectiles")]
        [SerializeField, Min(1)] int ballLightningCount = 1;
        public int BallLightningCount => ballLightningCount;

        [Tooltip("Amount of time between attacks")]
        [SerializeField] float abilityCooldown;
        public float AbilityCooldown => abilityCooldown;

        [Tooltip("Amount of time between lightnings")]
        [SerializeField] float durationBetweenHits;
        public float DurationBetweenHits => durationBetweenHits;
    }
}