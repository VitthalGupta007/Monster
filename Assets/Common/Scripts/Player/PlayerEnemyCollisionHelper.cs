using UnityEngine;

namespace OctoberStudio
{
    public class PlayerEnemyCollisionHelper : MonoBehaviour
    {
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerBehavior.Player.CheckTriggerEnter2D(collision);
        }

        protected virtual void OnTriggerExit2D(Collider2D collision)
        {
            PlayerBehavior.Player.CheckTriggerExit2D(collision);
        }
    }
}