using UnityEngine;

namespace VXMonster.Core.Enemy
{
    public class QueenWaspEventsHandler : MonoBehaviour
    {
        [SerializeField] QueenWaspBehavior queenWasp;

        public void Shoot()
        {
            queenWasp.Shoot();
        }
    }
}