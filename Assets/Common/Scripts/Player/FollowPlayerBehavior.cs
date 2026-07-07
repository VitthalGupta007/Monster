using UnityEngine;

namespace VXMonster.Core
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