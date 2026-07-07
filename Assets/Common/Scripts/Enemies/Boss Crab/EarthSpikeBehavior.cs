using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Enemy
{
    public class EarthSpikeBehavior : SimpleEnemyProjectileBehavior
    {
        private static readonly int HIDE_TRIGGER = Animator.StringToHash("Hide");

        [SerializeField] Animator animator;
        [SerializeField] SpriteRenderer visuals;
        [SerializeField] Collider2D spikeCollider;
        [SerializeField] ParticleSystem particle;

        bool isHiding = false;

        private WarningCircleBehavior warningCircle;

        public event UnityAction<EarthSpikeBehavior> onHidden;

        public float Lifetime { get; private set; }

        public void Spawn(float lifetime)
        {
            visuals.enabled = false;
            spikeCollider.enabled = false;

            warningCircle = StageController.PoolsManager.GetEntity<WarningCircleBehavior>("Warning Circle");

            warningCircle.transform.position = transform.position;

            warningCircle.Play(1f, 0.3f, 0.3f, ShowVisuals);

            isHiding = false;

            Lifetime = lifetime;
        }

        private void ShowVisuals()
        {
            visuals.enabled = true;
            spikeCollider.enabled = true;

            animator.Rebind();
            animator.Update(0);

            particle.Play();

            if (Lifetime > 0)
            {
                EasingManager.DoAfter(Lifetime, Hide);
            }
        }

        public void Clear()
        {
            warningCircle.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public void Hide()
        {
            animator.SetTrigger(HIDE_TRIGGER);

            isHiding = true;
        }

        public void OnHidden()
        {
            if(isHiding)
            {
                onHidden?.Invoke(this);
                gameObject.SetActive(false);
            }
        }
    }
}