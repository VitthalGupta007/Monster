using UnityEngine;

namespace VXMonster.Core.Abilities
{
    [CreateAssetMenu(fileName = "Restore HP Ability Data", menuName = "VX Monster/Abilities/Passive/Restore HP")]
    public class RestoreHPAbilityData : GenericAbilityData<RestoreHPAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.RestoreHP;
        }

        private void OnValidate()
        {
            type = AbilityType.RestoreHP;
        }
    }

    [System.Serializable]
    public class RestoreHPAbilityLevel : AbilityLevel
    {
        [SerializeField, Range(0f, 20f)] int restoredHPPercent = 1;
        public int RestoredHPPercent => restoredHPPercent;

        [SerializeField] float cooldown;
        public float Cooldown => cooldown;
    }
}