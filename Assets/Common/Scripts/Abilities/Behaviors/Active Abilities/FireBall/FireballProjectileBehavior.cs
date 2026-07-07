using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Abilities
{
    public class FireballProjectileBehavior : MonoBehaviour
    {
        private static readonly int FIREBALL_LAUNCH_HASH = "Fireball Launch".GetHashCode();
        private static readonly int FIREBALL_EXPLOSION_HASH = "Fireball Explosion".GetHashCode();

        [SerializeField] Collider2D fireballCollider;

        [SerializeField] ParticleSystem explosionParticle;
        [SerializeField] GameObject visuals;

        private IEasingCoroutine movementCoroutine;
        private IEasingCoroutine disableCoroutine;

        public float DamageMultiplier { get; set; }
        public float ExplosionRadius { get; set; }
        public float Lifetime { get; set; }
        public float Speed { get; set; }
        public float Size { get; set; }

        public event UnityAction<FireballProjectileBehavior> onFinished;

        public void Init()
        {
            transform.localScale = Vector3.one * Size * PlayerBehavior.Player.SizeMultiplier;

            var distance = Speed * Lifetime * PlayerBehavior.Player.DurationMultiplier;
            var selfDestructPosition = transform.position + transform.rotation * Vector3.up * distance;

            movementCoroutine = transform.DoPosition(selfDestructPosition, Lifetime / PlayerBehavior.Player.ProjectileSpeedMultiplier).SetOnFinish(Explode);

            visuals.SetActive(true);
            fireballCollider.enabled = true;

            GameController.AudioManager.PlaySound(FIREBALL_LAUNCH_HASH);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemy = other.GetComponent<EnemyBehavior>();

            if (enemy != null)
            {
                movementCoroutine.Stop();

                Explode();
            }
        }

        private void Explode()
        {
            fireballCollider.enabled = false;
            visuals.SetActive(false);

            var enemies = StageController.EnemiesSpawner.GetEnemiesInRadius(transform.position, ExplosionRadius);

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];

                enemy.TakeDamage(PlayerBehavior.Player.Damage * DamageMultiplier);
            }

            explosionParticle.Play();

            disableCoroutine = EasingManager.DoAfter(1f, () =>
            {
                gameObject.SetActive(false);
                onFinished?.Invoke(this);
            });

            GameController.AudioManager.PlaySound(FIREBALL_EXPLOSION_HASH);
        }

        public void Clear()
        {
            movementCoroutine.StopIfExists();
            disableCoroutine.StopIfExists();

            gameObject.SetActive(false);

            visuals.SetActive(true);
            fireballCollider.enabled = true;
        }
    }
}