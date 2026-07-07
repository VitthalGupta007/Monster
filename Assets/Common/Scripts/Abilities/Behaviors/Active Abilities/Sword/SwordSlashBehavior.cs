using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Abilities
{
    public class SwordSlashBehavior : ProjectileBehavior
    {
        [SerializeField] Collider2D slashCollider;
        [SerializeField] float duration;

        public float Size { get; set; }

        public UnityAction<SwordSlashBehavior> onFinished;

        private IEasingCoroutine waitingCoroutine;
        private IEasingCoroutine colliderCoroutine;

        public override void Init()
        {
            base.Init();

            transform.localScale = Vector3.one * Size * PlayerBehavior.Player.SizeMultiplier;

            slashCollider.enabled = true;

            colliderCoroutine = EasingManager.DoAfter(0.1f, () => slashCollider.enabled = false);

            waitingCoroutine = EasingManager.DoAfter(duration, () => {
                onFinished?.Invoke(this);
                Disable();
            });
        }

        public void Disable()
        {
            waitingCoroutine.StopIfExists();
            colliderCoroutine.StopIfExists();

            gameObject.SetActive(false);
            slashCollider.enabled = true;
        }
    }
}