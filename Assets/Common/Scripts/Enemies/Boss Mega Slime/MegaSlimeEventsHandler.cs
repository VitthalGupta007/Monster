using UnityEngine;

namespace VXMonster.Core.Enemy
{
    public class MegaSlimeEventsHandler : MonoBehaviour
    {
        [SerializeField] EnemyMegaSlimeBehavior megaSlime;

        public void SwordsAttack()
        {
            megaSlime.SwordsAttack();
        }
    }
}