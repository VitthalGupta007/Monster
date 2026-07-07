using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class BlackHoleEventsHandler : MonoBehaviour
    {
        [SerializeField] VoidBlackHoleBehavior blackHole;

        public void Hidden()
        {
            blackHole.OnHidden();
        }
    }
}