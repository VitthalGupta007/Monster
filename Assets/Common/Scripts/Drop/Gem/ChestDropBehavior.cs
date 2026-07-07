namespace OctoberStudio.Drop
{
    public class ChestDropBehavior : DropBehavior
    {
        public override void OnPickedUp()
        {
            base.OnPickedUp();

            gameObject.SetActive(false);

            StageController.AbilityManager.ShowChest();
        }
    }
}