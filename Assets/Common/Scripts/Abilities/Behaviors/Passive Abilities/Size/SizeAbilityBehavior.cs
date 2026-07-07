namespace OctoberStudio.Abilities
{
    public class SizeAbilityBehavior : AbilityBehavior<SizeAbilityData, SizeAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateSizeMultiplier(AbilityLevel.SizeMultiplier);
        }

        public override void Clear()
        {
            base.Clear();

            PlayerBehavior.Player.RecalculateSizeMultiplier(1);
        }
    }
}