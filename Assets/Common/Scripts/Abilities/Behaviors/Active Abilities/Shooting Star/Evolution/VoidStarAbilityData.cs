using UnityEngine;

namespace OctoberStudio.Abilities
{
    [CreateAssetMenu(fileName = "Void Star Ability Data", menuName = "October/Abilities/Evolution/Void Star")]
    public class VoidStarAbilityData : GenericAbilityData<VoidStartAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.VoidStar;
        }

        private void OnValidate()
        {
            type = AbilityType.VoidStar;
        }
    }

    [System.Serializable]
    public class VoidStartAbilityLevel : AbilityLevel
    {
        [Tooltip("This value is multiplied by the damage of the player")]
        [SerializeField, Min(0.1f)] float damage = 1f;
        public float Damage => damage;

        [Tooltip("Amount of projectiles alive at the same time")]
        [SerializeField] int projectilesCount = 2;
        public int ProjectilesCount => projectilesCount;

        [Tooltip("Distance between the player and the projectiles")]
        [SerializeField] float radius = 2.5f;
        public float Radius => radius;

        [Tooltip("Rotation spped of projectiles")]
        [SerializeField] float angularSpeed = -200;
        public float AngularSpeed => angularSpeed;
    }
}