using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class RangedEnemyEventsHandler : MonoBehaviour
    {
        private RangedEnemyBehavior rangedEnemy;

        private void Awake()
        {
            rangedEnemy = GetComponentInParent<RangedEnemyBehavior>();
        }

        public void RangedAttackEvent()
        {
            rangedEnemy.Attack();
        }

        public void RangedAttackEndedEvent()
        {
            rangedEnemy.OnAttackAnimationEnded();
        }
    }
}