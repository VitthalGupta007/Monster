using UnityEngine;

namespace OctoberStudio.Drop
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