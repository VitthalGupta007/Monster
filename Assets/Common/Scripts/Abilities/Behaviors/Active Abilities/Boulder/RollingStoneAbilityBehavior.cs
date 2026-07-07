using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class RollingStoneAbilityBehavior : AbilityBehavior<RollingStoneAbilityData, RollingStoneAbilityLevel>
    {
        private static readonly int ROLLING_STONE_LAUNCH_HASH = "Rolling Stone Launch".GetHashCode();

        [SerializeField] BoulderProjectileBehavior boulderProjectileBehavior;

        public override void Init(AbilityData data, int stageId)
        {
            base.Init(data, stageId);

            boulderProjectileBehavior.transform.position = PlayerBehavior.CenterPosition;
            boulderProjectileBehavior.Direction = (Vector2.up + Vector2.right).normalized;

            GameController.AudioManager.PlaySound(ROLLING_STONE_LAUNCH_HASH);
        }

        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            boulderProjectileBehavior.SetData(AbilityLevel.Size, AbilityLevel.Damage, AbilityLevel.Speed, AbilityLevel.AngularSpeed);
        }

        public override void Clear()
        {
            base.Clear();
        }
    }
}