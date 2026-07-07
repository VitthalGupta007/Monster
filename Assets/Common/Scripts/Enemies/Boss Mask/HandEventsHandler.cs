using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class HandEventsHandler : MonoBehaviour
    {
        [SerializeField] EnemyMaskHandBehavior hand;

        public void HandAppeared()
        {
            if(hand != null) hand.OnAppeared();
        }

        public void HandHidden()
        {
            if (hand != null) hand.OnHidden();
        }
    }
}