using UnityEngine;

namespace OctoberStudio.Drop
{
    public class BombDropBehavior : DropBehavior
    {
        [SerializeField] float damageMultiplier = 100;

        public override void OnPickedUp()
        {
            base.OnPickedUp();
            
            StageController.EnemiesSpawner.DealDamageToAllEnemies(PlayerBehavior.Player.Damage * damageMultiplier);

            gameObject.SetActive(false);
        }
    }
}

