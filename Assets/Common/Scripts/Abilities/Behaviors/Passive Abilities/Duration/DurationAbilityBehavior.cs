namespace OctoberStudio.Abilities
{
    public class DurationAbilityBehavior : AbilityBehavior<DurationAbilityData, DurationAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateDurationMultiplier(AbilityLevel.DurationMultiplier);
        }

        public override void Clear()
        {
            base.Clear();

            PlayerBehavior.Player.RecalculateDurationMultiplier(1);
        }
    }
}