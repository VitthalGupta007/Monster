using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Rolling Stone Ability Data", menuName = "October/Abilities/Active/Rolling Stone")]
    public class RollingStoneAbilityData : GenericAbilityData<RollingStoneAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.RollingStone;
        }

        private void OnValidate()
        {
            type = AbilityType.RollingStone;
        }
    }

    [System.Serializable]
    public class RollingStoneAbilityLevel : AbilityLevel
    {
        [Tooltip("The speed of the rolling stone")]
        [SerializeField] float speed;
        public float Speed => speed;

        [Tooltip("The speed of the rotation of the rolling stone")]
        [SerializeField] float angularSpeed = 180;
        public float AngularSpeed => angularSpeed;

        [Tooltip("The amount of damage the roling stone deals is calculated like Player.Damage * Damage")]
        [SerializeField] float damage;
        public float Damage => damage;

        [Tooltip("The size of the rolling stone")]
        [SerializeField] float size;
        public float Size => size;
    }
}
