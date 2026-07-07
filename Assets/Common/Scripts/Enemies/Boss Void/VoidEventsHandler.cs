using UnityEngine;

namespace OctoberStudio.Enemy
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