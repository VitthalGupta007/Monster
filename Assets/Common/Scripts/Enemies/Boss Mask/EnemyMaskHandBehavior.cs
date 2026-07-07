using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class EnemyMaskHandBehavior : SimpleEnemyProjectileBehavior
    {
        private static readonly int HIDE_TRIGGER = Animator.StringToHash("Hide");

        [SerializeField] Animator animator;

        private bool IsHiding { get; set; }

        private Vector3 Destination { get; set; }
        private float Duration { get; set; }

        private bool IsAppearing { get; set; }

        public void Init(Vector3 destination, float speed)
        {
            Destination = destination;

            Init(transform.position, (destination - transform.position).normalized);
            Duration = Vector3.Distance(transform.position, destination) / speed;

            IsAppearing = true;
        }

        public void OnAppeared()
        {
            if (IsAppearing)
            {
                IsAppearing = false;
                transform.DoPosition(Destination, Duration).SetEasing(EasingType.SineInOut).SetOnFinish(Hide);
            }
        }

        public void Hide()
        {
            animator.SetTrigger(HIDE_TRIGGER);

            IsHiding = true;
        }

        public void OnHidden()
        {
            if (IsHiding)
            {
                IsHiding = false;
                Disable();
            }
        }
    }
}