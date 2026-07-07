using UnityEngine;

namespace OctoberStudio.Abilities
{

    [CreateAssetMenu(fileName = "Projectile Speed Ability Data", menuName = "October/Abilities/Passive/Projectile Speed")]
    public class ProjectileSpeedAbilityData : GenericAbilityData<ProjectileSpeedAbilityLevel>
    {
        private void Awake()
        {
            type = AbilityType.ProjectileSpeed;
        }

        private void OnValidate()
        {
            type = AbilityType.ProjectileSpeed;
        }
    }

    [System.Serializable]
    public class ProjectileSpeedAbilityLevel : AbilityLevel
    {
        [SerializeField, Min(1f)] float projectileSpeedMultiplier = 1f;
        public float ProjectileSpeedMultiplier => projectileSpeedMultiplier;
    }
}