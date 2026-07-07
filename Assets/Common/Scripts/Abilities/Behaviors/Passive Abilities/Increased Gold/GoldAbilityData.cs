using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Gold Ability Data", menuName = "October/Abilities/Passive/Gold")]
    public class GoldAbilityData : GenericAbilityData<GoldAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.IncreasedGold;
        }

        private void OnValidate()
        {
            type = AbilityType.IncreasedGold;
        }
    }

    [System.Serializable]
    public class GoldAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1)] float goldMultiplier = 1;
        public float GoldMultiplier => goldMultiplier;
    }
}