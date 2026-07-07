using UnityEngine;

namespace VXMonster.Core.Enemy
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