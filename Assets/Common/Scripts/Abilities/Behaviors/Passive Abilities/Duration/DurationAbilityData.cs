using UnityEngine;

namespace VXMonster.Core.Abilities
{
    [CreateAssetMenu(fileName = "Duration Ability Data", menuName = "VX Monster/Abilities/Passive/Duration")]
    public class DurationAbilityData : GenericAbilityData<DurationAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.Duration;
        }

        private void OnValidate()
        {
            type = AbilityType.Duration;
        }
    }

    [System.Serializable]
    public class DurationAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1)] float durationMultiplier = 1;
        public float DurationMultiplier => durationMultiplier;
    }
}