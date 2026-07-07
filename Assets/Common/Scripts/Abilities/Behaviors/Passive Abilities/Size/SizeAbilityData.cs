using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Size Ability Data", menuName = "October/Abilities/Passive/Size")]
    public class SizeAbilityData : GenericAbilityData<SizeAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.Size;
        }

        private void OnValidate()
        {
            type = AbilityType.Size;
        }
    }

    [System.Serializable]
    public class SizeAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1)] float sizeMultiplier = 1;
        public float SizeMultiplier => sizeMultiplier;
    }
}