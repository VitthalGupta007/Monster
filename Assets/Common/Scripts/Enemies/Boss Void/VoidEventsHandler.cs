using UnityEngine;

namespace VXMonster.Core.Enemy
{
    public class VoidEventsHandler : MonoBehaviour
    {
        [SerializeField] VoidBehavior boss;

        public void Teleport()
        {
            boss.Teleport();
        }
    }
}