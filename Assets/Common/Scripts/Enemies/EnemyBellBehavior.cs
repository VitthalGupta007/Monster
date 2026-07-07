using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class EnemyBellBehavior : EnemyBehavior
    {
        public static readonly int IS_ATTACKING_BOOL = Animator.StringToHash("Is Attacking");
        public static readonly int IS_CHARGING_BOOL = Animator.StringToHash("Is Charging");

        [SerializeField] float attackCooldown = 5f;
        [SerializeField] Animator animator;

        private PoolComponent<EnemyBellProjectile> projectilePool;
        private PoolComponent<SimpleEnemyProjectileBehavior> smallProjectilePool;
        private PoolComponent<SimpleEnemyProjectileBehavior> spikeProjectilePool;
        private PoolComponent<ParticleSystem> explosionParticlePool;

        private List<SimpleEnemyProjectileBehavior> projectiles = new List<SimpleEnemyProjectileBehavior>();

        private Coroutine attackCoroutine;
        private Coroutine movementCoroutine;

        [SerializeField] List<Transform> projectilePositions;

        private List<IEasingCoroutine> explosionCoroutines = new List<IEasingCoroutine>();

        protected override void Awake()
        {
            base.Awake();

            projectilePool = new PoolComponent<EnemyBellProjectile>("Bell Enemy Projectile", projectilePrefab, 5);
            smallProjectilePool = new PoolComponent<SimpleEnemyProjectileBehavior>("Bell Enemy Small Projectile", smallProjectilePrefab, 30);
            spikeProjectilePool = new PoolComponent<SimpleEnemyProjectileBehavior>("Bell Enemy Spike Projectile", spikeProjectilePrefab, 10);

            explosionParticlePool = new PoolComponent<ParticleSystem>("Bell Enemy Projectile Explosion", explosionParticlePrefab, 5);
        }

        public override void Play()
        {
            base.Play();

            attackCoroutine = StartCoroutine(AttackCoroutine());
            movementCoroutine = StartCoroutine(MovementCoroutine());
        }

        [SerializeField] float movementCooldown = 0.5f;

        private IEnumerator MovementCoroutine()
        {
            while (true)
            {
                IsMoving = true;

                var randomPoint = StageController.FieldManager.Fence.GetRandomPointInside(1);

                IsMovingToCustomPoint = true;
                CustomPoint = randomPoint;

                yield return new WaitUntil(() => Vector2.Distance(transform.position, randomPoint) < 0.2f);

                IsMovingToCustomPoint = false;

                IsMoving = false;

                yield return new WaitForSeconds(movementCooldown);
            }
        }

        private IEnumerator AttackCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(attackCooldown);

                animator.SetBool(IS_CHARGING_BOOL, true);
                yield return SimpleAttackCoroutine();
                animator.SetBool(IS_CHARGING_BOOL, false);

                yield return new WaitForSeconds(attackCooldown);

                animator.SetBool(IS_CHARGING_BOOL, true);
                yield return SlowAttackCoroutine();
                animator.SetBool(IS_CHARGING_BOOL, false);
                
                yield return new WaitForSeconds(attackCooldown);

                animator.SetBool(IS_CHARGING_BOOL, true);
                yield return SmallAttackCoroutine();
                animator.SetBool(IS_CHARGING_BOOL, false);
            }
        }

        [Header("Simple Attack")]
        [SerializeField] int simpleAttackWavesCount = 3;
        [SerializeField] float simpleAttackDamage = 1;
        [SerializeField] float simpleAttackProjectileCooldown = 0.2f;
        [SerializeField] float simpleAttackStayDuration = 1f;
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] GameObject explosionParticlePrefab;

        private IEnumerator SimpleAttackCoroutine()
        {
            for(int i = 0; i < simpleAttackWavesCount; i++)
            {
                var simpleProjectiles = new List<EnemyBellProjectile>();

                for (int j = 0; j < projectilePositions.Count; j++)
                {
                    var projectile = projectilePool.GetEntity();

                    projectile.transform.position = projectilePositions[j].transform.position;

                    projectile.StickToTransform(projectilePositions[j]);
                    projectile.onFinished += OnProjectileFinished;

                    projectiles.Add(projectile);
                    simpleProjectiles.Add(projectile);

                    yield return new WaitForSeconds(simpleAttackProjectileCooldown);
                }

                yield return new WaitForSeconds(simpleAttackStayDuration);

                foreach (var projectile in simpleProjectiles)
                {
                    var distanceToPlayer = PlayerBehavior.CenterPosition - projectile.transform.position.XY();
                    var playerDirection = distanceToPlayer.normalized;

                    projectile.LifeTime = distanceToPlayer.magnitude / projectile.Speed;
                    projectile.Damage = StageController.Stage.EnemyDamage * simpleAttackDamage;

                    projectile.Init(projectile.transform.position, playerDirection);

                    yield return new WaitForSeconds(simpleAttackProjectileCooldown);
                }
            }
            
        }

        [Header("Slow Attack")]
        [SerializeField] int slowAttackRadialAmount = 10;
        [SerializeField] int slowAttackWavesAmount = 3;
        [SerializeField] float slowAttackDamage = 1;
        [SerializeField] float slowAttackWaveCooldown = 1;
        [SerializeField] GameObject spikeProjectilePrefab;

        private IEnumerator SlowAttackCoroutine()
        {
            for (int j = 0; j < slowAttackWavesAmount; j++)
            {
                for (int i = 0; i < slowAttackRadialAmount; i++)
                {
                    var angle = 360f / 10f * i;

                    var direction = Quaternion.Euler(0, 0, angle) * Vector2.up;

                    var projectile = spikeProjectilePool.GetEntity();

                    projectile.Init(transform.position, direction);
                    projectile.Damage = StageController.Stage.EnemyDamage * slowAttackDamage;

                    projectile.onFinished += OnProjectileFinished;

                    projectiles.Add(projectile);
                }

                yield return new WaitForSeconds(slowAttackWaveCooldown);
            }

            
        }

        [Header("Burst Attack")]
        [SerializeField] int burstAttackRadialAmount = 10;
        [SerializeField] int burstAttackSingleBurstAmount = 3;
        [SerializeField] float burstAttackDamage = 1;
        [SerializeField] float burstAttackProjectileCooldown = 0.05f;
        [SerializeField] GameObject smallProjectilePrefab;

        private IEnumerator SmallAttackCoroutine()
        {
            for (int i = 0; i < burstAttackRadialAmount; i++)
            {
                var angle = 360f / 10f * i;

                var direction = Quaternion.Euler(0, 0, angle) * Vector2.up;

                for (int j = 0; j < burstAttackSingleBurstAmount; j++)
                {
                    var projectile = smallProjectilePool.GetEntity();

                    projectile.Init(transform.position, direction);
                    projectile.Damage = StageController.Stage.EnemyDamage * burstAttackDamage;

                    projectile.onFinished += OnProjectileFinished;

                    projectiles.Add(projectile);

                    yield return new WaitForSeconds(burstAttackProjectileCooldown);
                }
            }
        }

        private void OnProjectileFinished(SimpleEnemyProjectileBehavior projectile)
        {
            projectile.onFinished -= OnProjectileFinished;
            projectiles.Remove(projectile);

            if(projectile is EnemyBellProjectile bellProjectile)
            {
                var particle = explosionParticlePool.GetEntity();
                if (bellProjectile.Successful)
                {
                    particle.transform.position = PlayerBehavior.Player.transform.position;
                } else
                {
                    particle.transform.position = bellProjectile.transform.position;
                }

                particle.Play();

                IEasingCoroutine coroutine = null;
                coroutine = EasingManager.DoAfter(5f, () =>
                {
                    particle.gameObject.SetActive(false);
                    explosionCoroutines.Remove(coroutine);
                });

                explosionCoroutines.Add(coroutine);
            }
        }

        protected override void Die(bool flash)
        {
            base.Die(flash);

            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            if(movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
                movementCoroutine = null;
            }

            foreach(var projectile in projectiles)
            {
                projectile.onFinished -= OnProjectileFinished;
                projectile.Disable();
            }

            projectiles.Clear();

            projectilePool.Destroy();
            smallProjectilePool.Destroy();
            spikeProjectilePool.Destroy();

            foreach(var coroutine in explosionCoroutines)
            {
                coroutine.StopIfExists();
            }

            explosionCoroutines.Clear();

            explosionParticlePool.DisableAllEntities();
            explosionParticlePool.Destroy();
        }
    }
}