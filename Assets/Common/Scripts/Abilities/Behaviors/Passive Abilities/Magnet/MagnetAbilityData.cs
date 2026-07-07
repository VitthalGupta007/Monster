using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Magnet Ability Data", menuName = "October/Abilities/Passive/Magnet")]
    public class MagnetAbilityData : GenericAbilityData<MagnetAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.Magnet;
        }

        private void OnValidate()
        {
            type = AbilityType.Magnet;
        }
    }

    [System.Serializable]
    public class MagnetAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1f)] float radiusMultiplier = 1f;
        public float RadiusMultiplier => radiusMultiplier;
    }
}