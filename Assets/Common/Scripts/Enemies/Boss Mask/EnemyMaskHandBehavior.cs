using VXMonster.Core.Easing;
using UnityEngine;

namespace VXMonster.Core.Enemy
{
    public class EnemyMaskHandBehavior : SimpleEnemyProjectileBehavior
    {
        private static readonly int HIDE_TRIGGER = Animator.StringToHash("Hide");

        [SerializeField] Animator animator;
        [SerializeField] Collider2D handCollider;

        private bool IsHiding { get; set; }

        private Vector3 Destination { get; set; }
        private float Duration { get; set; }

        private bool IsAppearing { get; set; }

        private void Awake()
        {
            if (handCollider == null)
            {
                handCollider = GetComponent<Collider2D>();
            }
        }

        public void Init(Vector3 destination, float speed)
        {
            Destination = destination;

            if (handCollider != null)
            {
                handCollider.enabled = true;
            }

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
            if (handCollider != null)
            {
                handCollider.enabled = false;
            }

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