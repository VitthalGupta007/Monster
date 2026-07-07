namespace OctoberStudio.Abilities
{
    public class MagnetAbilityBehavior : AbilityBehavior<MagnetAbilityData, MagnetAbilityLevel>
    {
        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            PlayerBehavior.Player.RecalculateMagnetRadius(AbilityLevel.RadiusMultiplier);
        }

        public override void Clear()
        {
            base.Clear();

            PlayerBehavior.Player.RecalculateMagnetRadius(1);
        }
    }
}