using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Enemy
{
    public class HoneyMineBehavior : MonoBehaviour
    {
        private static readonly int LAND_HASH = Animator.StringToHash("Land");
        private static readonly int SHAKE_HASH = Animator.StringToHash("Shake");

        [SerializeField] Animator animator;
        [SerializeField] ParticleSystem explosionParticle;
        [SerializeField] Collider2D mineCollider;
        [SerializeField] AnimationCurve movementCurve;
        [SerializeField] float damageRadius;
        [SerializeField] GameObject blobObject;

        public event UnityAction<HoneyMineBehavior> onExploded;

        public float Damage { get; set; }

        IEasingCoroutine coroutine;
        private bool isShaking = false;

        private void Awake()
        {
            mineCollider.enabled = false;
        }

        public void Spawn(Vector2 position, Vector2 direction, Vector2 landingPosition)
        {
            var startPosition = position - direction * 0.3f;
            transform.position = startPosition;
            var key = position + direction * 1;

            transform.localScale = Vector3.one * 0.1f;
            transform.DoLocalScale(Vector3.one, 0.3f);

            coroutine = EasingManager.DoFloat(0, 1, 0.5f, (t) => {
                transform.position = EvaluateQuadratic(startPosition, key, landingPosition, t);
            }).SetEasingCurve(movementCurve).SetOnFinish(() =>
            {
                animator.SetTrigger(LAND_HASH);
                mineCollider.enabled = true;
            });

            blobObject.SetActive(true);
            isShaking = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isShaking) return;
            if (other.GetComponent<PlayerEnemyCollisionHelper>() == null) return;

            isShaking = true;
            animator.SetTrigger(SHAKE_HASH);
            coroutine = EasingManager.DoAfter(1f, () => {
                explosionParticle.Play();
                blobObject.SetActive(false);

                if (Vector2.Distance(transform.position, PlayerBehavior.Player.transform.position) < damageRadius)
                {
                    PlayerBehavior.Player.TakeDamage(Damage);
                }

                coroutine = EasingManager.DoAfter(5f,() => { 
                    Clear();
                    onExploded?.Invoke(this);
                });
            });
        }

        public void Clear()
        {
            coroutine.StopIfExists();
            mineCollider.enabled = false;
            gameObject.SetActive(false);
            blobObject.SetActive(true);

            isShaking = false;
        }

        private static Vector2 EvaluateQuadratic(Vector2 p1, Vector2 key, Vector2 p2, float t)
        {
            Vector3 c1 = Vector3.Lerp(p1, key, t);
            Vector3 c2 = Vector3.Lerp(key, p2, t);
            return Vector3.Lerp(c1, c2, t);
        }
    }
}