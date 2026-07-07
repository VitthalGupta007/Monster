using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Mace Ball Ability Data", menuName = "October/Abilities/Evolution/Mace Ball")]
    public class MaceBallAbilityData : GenericAbilityData<MaceBallAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.MaceBall;
        }

        private void OnValidate()
        {
            type = AbilityType.MaceBall;
        }
    }

    [System.Serializable]
    public class MaceBallAbilityLevel : AbilityLevel
    {
        [Tooltip("The speed of the mace ball")]
        [SerializeField] float speed;
        public float Speed => speed;

        [Tooltip("The speed of the rotation of the mace ball")]
        [SerializeField] float angularSpeed = 180;
        public float AngularSpeed => angularSpeed;

        [Tooltip("The amount of damage the roling stone deals is calculated like Player.Damage * Damage")]
        [SerializeField] float damage;
        public float DamageMultiplier => damage;

        [Tooltip("The size of the mace ball")]
        [SerializeField] float size;
        public float Size => size;
    }
}
