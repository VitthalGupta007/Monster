namespace OctoberStudio.Abilities
{
    public class DamageReductionAbilityBehavior : AbilityBehavior<DamageReductionAbilityData, DamageReductionAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateDamageReduction(AbilityLevel.DamageReductionPercent);
        }

        public override void Clear()
        {
            base.Clear();

            PlayerBehavior.Player.RecalculateDamageReduction(0);
        }
    }
}