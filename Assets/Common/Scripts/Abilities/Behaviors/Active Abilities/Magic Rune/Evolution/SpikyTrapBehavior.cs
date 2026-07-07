using OctoberStudio.Easing;
using OctoberStudio.Pool;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class SpikyTrapBehavior : MonoBehaviour
    {
        public static readonly int SPIKY_TRAP_EXPLOSION_HASH = "Spiky Trap Explosion".GetHashCode();

        [SerializeField] CircleCollider2D mineTriggerCollider;
        [SerializeField] GameObject mineVisuals;
        [SerializeField] ParticleSystem explosionParticle;

        PoolComponent<SimplePlayerProjectileBehavior> spikesPool;

        public float DamageMultiplier { get; private set; }
        public float DamageRadius { get; private set; }

        private int spikesCount;
        private float spikeDamageMultiplier;

        private IEasingCoroutine lifetimeCoroutine;

        public void SetData(SpikyTrapAbilityLevel stage, PoolComponent<SimplePlayerProjectileBehavior> spikesPool)
        {
            this.spikesPool = spikesPool;
            spikesCount = stage.SpikesCount;
            spikeDamageMultiplier = stage.SpikeDamage;

            var size = stage.MineSize * PlayerBehavior.Player.SizeMultiplier;
            transform.localScale = Vector3.one * size;
            mineTriggerCollider.radius = stage.MineTriggerRadius / size;

            DamageMultiplier = stage.Damage;

            DamageRadius = stage.MineDamageRadius * PlayerBehavior.Player.SizeMultiplier;

            mineVisuals.SetActive(true);

            EasingManager.DoAfter(0.2f, () => mineTriggerCollider.enabled = true);
            lifetimeCoroutine = EasingManager.DoAfter(stage.MineLifetime, Explode);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var enemy = collision.GetComponent<EnemyBehavior>();

            if (enemy != null)
            {
                Explode();
            }
        }

        private void Explode()
        {
            mineTriggerCollider.enabled = false;
            mineVisuals.SetActive(false);

            var enemies = StageController.EnemiesSpawner.GetEnemiesInRadius(transform.position, DamageRadius);

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];

                enemy.TakeDamage(PlayerBehavior.Player.Damage * DamageMultiplier);
            }

            explosionParticle.Play();

            float angle = Random.Range(0, 180f);

            for (int i = 0; i < spikesCount; i++)
            {
                var spike = spikesPool.GetEntity();
                angle += 360f / spikesCount;

                spike.Init(transform.position, Quaternion.Euler(0, 0, angle) * Vector2.up);

                spike.DamageMultiplier = spikeDamageMultiplier;
                spike.KickBack = false;
            }

            lifetimeCoroutine.StopIfExists();

            GameController.AudioManager.PlaySound(SPIKY_TRAP_EXPLOSION_HASH);

            EasingManager.DoAfter(1f, () => gameObject.SetActive(false));
        }
    }
}