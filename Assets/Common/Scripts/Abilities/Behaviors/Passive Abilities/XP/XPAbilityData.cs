using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "XP Ability Data", menuName = "October/Abilities/Passive/XP")]
    public class XPAbilityData : GenericAbilityData<XPAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.XP;
        }

        private void OnValidate()
        {
            type = AbilityType.XP;
        }
    }

    [System.Serializable]
    public class XPAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1f)] float xpMultiplier = 1f;
        public float XPMultiplier => xpMultiplier;
    }
}