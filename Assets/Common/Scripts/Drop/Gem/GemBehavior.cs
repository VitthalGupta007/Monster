using VXMonster.Core.Drop;
using UnityEngine;

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
            StageController.ExperienceManager.AddXP(XP);
        }
    }
}