using UnityEngine;

namespace OctoberStudio
{
    public class FollowPlayerBehavior : MonoBehaviour
    {
        [SerializeField] protected Vector3 offset;

        protected virtual void Update()
        {
            if(PlayerBehavior.Player != null)
            {
                transform.position = PlayerBehavior.Player.transform.position + offset;
            }
        }
    }
}