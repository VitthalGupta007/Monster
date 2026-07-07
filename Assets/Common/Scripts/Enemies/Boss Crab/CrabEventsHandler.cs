using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class CrabEventsHandler : MonoBehaviour
    {
        [SerializeField] CrabBehavior crab;

        public void ClawHit()
        {
            crab.PlayClawHitParticle();
        }
    }
}