using VXMonster.Core.Drop;
using UnityEngine;
using VXMonster.Gameplay;

namespace VXMonster.Core
{
    public class GemBehavior : DropBehavior
    {
        [SerializeField] int xp;
        public float XP => xp;

        public override void OnPickedUp()
        {
            base.OnPickedUp();

            gameObject.SetActive(false);

            if (StageController.ExperienceManager == null)
                return;

            var xpAmount = XP * RunXpScaling.GetCurrentGemXpMultiplier();
            StageController.ExperienceManager.AddXP(xpAmount);
        }
    }
}
