namespace OctoberStudio.Abilities
{
    public class CooldownAbilityBehavior : AbilityBehavior<CooldownAbilityData, CooldownAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateCooldownMuliplier(AbilityLevel.CooldownMultiplier);
        }

        public override void Clear()
        {
            base.Clear();

            PlayerBehavior.Player.RecalculateCooldownMuliplier(1);
        }
    }
}