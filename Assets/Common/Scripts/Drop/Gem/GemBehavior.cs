using OctoberStudio.Drop;
using UnityEngine;

namespace OctoberStudio
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