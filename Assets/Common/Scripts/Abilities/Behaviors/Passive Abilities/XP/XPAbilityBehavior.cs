namespace OctoberStudio.Abilities
{
    public class XPAbilityBehavior : AbilityBehavior<XPAbilityData, XPAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateXPMuliplier(AbilityLevel.XPMultiplier);
        }

        public override void Clear()
        {
            base.Clear();

            PlayerBehavior.Player.RecalculateXPMuliplier(1);
        }
    }
}