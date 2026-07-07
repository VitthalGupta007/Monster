using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Cooldown Ability Data", menuName = "October/Abilities/Passive/Cooldown")]
    public class CooldownAbilityData : GenericAbilityData<CooldownAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.Cooldown;
        }

        private void OnValidate()
        {
            type = AbilityType.Cooldown;
        }
    }

    [System.Serializable]
    public class CooldownAbilityLevel : AbilityLevel
    {
        [SerializeField, Range(0.1f, 1f)] float cooldownMultiplier = 1f;
        public float CooldownMultiplier => cooldownMultiplier;
    }
}