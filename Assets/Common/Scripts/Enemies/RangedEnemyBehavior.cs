using VXMonster.Core.Extensions;
using VXMonster.Core.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core.Enemy
{
    public class RangedEnemyBehavior : EnemyBehavior
    {
        private static readonly int RANGED_ATTACK_TRIGGER = Animator.StringToHash("Ranged Attack");

        [Header("Ranged Enemy Data")]
        [SerializeField] Animator animator;
        [SerializeField] AttackType attackType = AttackType.AtPlayer;

        [Space]
        [SerializeField, Min(1)] int minProjectilesPerAttack = 1;
        [SerializeField, Min(1)] int maxProjectilesPerAttack = 1;
        [SerializeField] float timeBetweenProjectiles = 0f;
        [SerializeField] float projectileDamage = 1f;
        [SerializeField, Range(0, 90f)] float projectileSpread = 0f;

        [Header("Overrides")]
        [SerializeField] bool overrideProjectileSpeed = false;
        [SerializeField, Min(0.01f)] float projectileSpeed = 1f;
        [SerializeField] bool overrideProjectileLifetime = false;
        [SerializeField, Min(0.01f)] float projectileLifetime = 1f;

        [Space]
        [SerializeField] GameObject projectilePrefab;

        [Header("Shooting Behavior")]
        [SerializeField, Min(0)] float rangedAttackCooldown = 3;
        [SerializeField] bool useAttackAnimation = false;
        [SerializeField, Min(0)] float minDistanceToPlayerForAttacking = 5f;
        private float minDistanceToPlayerForAttackingSqr;

        [Space]
        [Tooltip("Only works if 'useAttackAnimation' is true")]
        [SerializeField] bool changeSpeedDuringAttackAnimation = false;
        [SerializeField, Min(0)] float speedDuringAttackAnimation = 1f;

        private static Dictionary<GameObject, PoolComponent<SimpleEnemyProjectileBehavior>> projectilePools = new Dictionary<GameObject, PoolComponent<SimpleEnemyProjectileBehavior>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            projectilePools = new Dictionary<GameObject, PoolComponent<SimpleEnemyProjectileBehavior>>();
        }

        private float lastTimeAttacked = 0;

        protected override void Awake()
        {
            base.Awake();

            if (!projectilePools.ContainsKey(projectilePrefab))
            {
                var pool = new PoolComponent<SimpleEnemyProjectileBehavior>(projectilePrefab, maxProjectilesPerAttack * 2);
                projectilePools.Add(projectilePrefab, pool);
            }

            minDistanceToPlayerForAttackingSqr = minDistanceToPlayerForAttacking * 2;
        }

        public override void Play()
        {
            base.Play();

            lastTimeAttacked = Time.time;
        }

        protected override void Update()
        {
            base.Update();

            var distanceToPlayer = (Center - PlayerBehavior.CenterPosition).sqrMagnitude;

            if(distanceToPlayer < minDistanceToPlayerForAttackingSqr && lastTimeAttacked + rangedAttackCooldown < Time.time)
            {
                if (useAttackAnimation)
                {
                    animator.SetTrigger(RANGED_ATTACK_TRIGGER);
                    if (changeSpeedDuringAttackAnimation)
                    {
                        Speed = speedDuringAttackAnimation;
                    }
                } else
                {
                    Attack();
                }

                lastTimeAttacked = Time.time;
            }
        }

        public virtual void Attack()
        {
            if (!IsAlive) return;

            var projectilesCount = Random.Range(minProjectilesPerAttack, maxProjectilesPerAttack + 1);

            if(attackType == AttackType.Circular)
            {
                StartCoroutine(CircularAttack(projectilesCount));
            } else
            {
                StartCoroutine(AtPlayerAttack(projectilesCount));
            }
        }

        protected virtual IEnumerator CircularAttack(int projectilesCount)
        {
            var wait = new WaitForSeconds(timeBetweenProjectiles);

            var enemyToPlayerDirection = (PlayerBehavior.CenterPosition - Center).normalized;
            
            var step = 360f / projectilesCount;

            for (int i = 0; i < projectilesCount; i++)
            {
                var projectileDirection = Quaternion.Euler(0, 0, i * step + Random.Range(-projectileSpread, projectileSpread)) * enemyToPlayerDirection;

                LaunchProjectile(projectileDirection);

                if (timeBetweenProjectiles > 0) yield return wait;
            }
        }

        protected virtual IEnumerator AtPlayerAttack(int projectilesCount)
        {
            var wait = new WaitForSeconds(timeBetweenProjectiles);

            var enemyToPlayerDirection = (PlayerBehavior.CenterPosition - Center).normalized;

            for (int i = 0; i < projectilesCount; i++)
            {
                var projectileDirection = Quaternion.Euler(0, 0, Random.Range(-projectileSpread, projectileSpread)) * enemyToPlayerDirection;

                LaunchProjectile(projectileDirection);

                if (timeBetweenProjectiles > 0) yield return wait;
            }
        }

        protected virtual void LaunchProjectile(Vector2 direction)
        {
            var projectile = projectilePools[projectilePrefab].GetEntity();

            if (overrideProjectileLifetime) projectile.LifeTime = projectileLifetime;
            if (overrideProjectileSpeed) projectile.Speed = projectileSpeed;

            projectile.Damage = projectileDamage;
            projectile.Init(Center, direction);
        }

        public void OnAttackAnimationEnded()
        {
            if (changeSpeedDuringAttackAnimation)
            {
                Speed = speed;
            }
        }

        public enum AttackType
        {
            AtPlayer = 0,
            Circular = 1,
        }

        protected virtual void OnDestroy()
        {
            if (projectilePools.ContainsKey(projectilePrefab))
            {
                projectilePools[projectilePrefab].Destroy();
                projectilePools.Remove(projectilePrefab);
            }
        }
    }
}