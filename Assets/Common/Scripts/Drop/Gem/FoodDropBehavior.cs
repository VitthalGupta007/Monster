using UnityEngine;

namespace VXMonster.Core.Drop
{
    public class FoodDropBehavior : DropBehavior
    {
        [SerializeField] float hp;

        public override void OnPickedUp()
        {
            base.OnPickedUp();

            PlayerBehavior.Player.Heal(hp);

            gameObject.SetActive(false);
        }
    }
}