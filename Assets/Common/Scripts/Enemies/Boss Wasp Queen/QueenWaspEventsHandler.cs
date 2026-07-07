using UnityEngine;

namespace OctoberStudio.Enemy
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