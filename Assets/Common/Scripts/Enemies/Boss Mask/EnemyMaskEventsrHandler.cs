using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class EnemyMaskEventsrHandler : MonoBehaviour
    {
        [SerializeField] EnemyMaskBehavior mask;

        public void WaveAttack()
        {
            if(mask != null) mask.WaveAttack();
        }
    }
}