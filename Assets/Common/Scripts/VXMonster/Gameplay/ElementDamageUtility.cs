using VXMonster.Core;
using UnityEngine;

namespace VXMonster.Gameplay
{
    public static class ElementDamageUtility
    {
        public static void DealDamage(EnemyBehavior enemy, float damage, ElementType element, float duration = 3f)
        {
            if (enemy == null || !enemy.IsAlive) return;
            enemy.TakeDamage(damage, element, duration);
        }
    }
}
