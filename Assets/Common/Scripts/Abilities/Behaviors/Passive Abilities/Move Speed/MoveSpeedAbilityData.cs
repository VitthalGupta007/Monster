using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Move Speed Ability Data", menuName = "October/Abilities/Passive/Move Speed")]
    public class MoveSpeedAbilityData: GenericAbilityData<MoveSpeedAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.MoveSpeed;
        }

        private void OnValidate()
        {
            type = AbilityType.MoveSpeed;
        }
    }

    [System.Serializable]
    public class MoveSpeedAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1f)] float speedMultiplier = 1f;
        public float SpeedMultiplier => speedMultiplier;
    }
}