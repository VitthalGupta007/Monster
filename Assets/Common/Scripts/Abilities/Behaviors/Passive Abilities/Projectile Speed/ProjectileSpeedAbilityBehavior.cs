namespace OctoberStudio.Abilities
{
    public class ProjectileSpeedAbilityBehavior : AbilityBehavior<ProjectileSpeedAbilityData, ProjectileSpeedAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateProjectileSpeedMultiplier(AbilityLevel.ProjectileSpeedMultiplier);
        }

        public override void Clear()
        {
            base.Clear();

            PlayerBehavior.Player.RecalculateProjectileSpeedMultiplier(1);
        }
    }
}