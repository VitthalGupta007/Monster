using UnityEngine;

namespace OctoberStudio.Enemy
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