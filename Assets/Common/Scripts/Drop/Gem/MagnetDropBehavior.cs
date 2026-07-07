namespace OctoberStudio.Drop
{
    public class MagnetDropBehavior : DropBehavior
    {
        public override void OnPickedUp()
        {
            base.OnPickedUp();

            StageController.DropManager.PickUpAllDrop();

            gameObject.SetActive(false);
        }
    }
}