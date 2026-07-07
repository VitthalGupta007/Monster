using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class EnemyMaskBehavior : EnemyBehavior
    {
        private static readonly int IS_CHARING_BOOL = Animator.StringToHash("Is Charging");

        [SerializeField] GameObject projectilePrefab;
        [SerializeField] GameObject handPrefab;

        [SerializeField] float attackCooldown = 3f;
        [SerializeField] int projectilesCount = 4;

        [SerializeField] Animator animator;
        [SerializeField] float handDamage;

        private PoolComponent<SimpleEnemyProjectileBehavior> projectilePool;
        private PoolComponent<EnemyMaskHandBehavior> handPool;

        private List<SimpleEnemyProjectileBehavior> projectiles = new List<SimpleEnemyProjectileBehavior>();

        private Coroutine attackCoroutine;

        protected override void Awake()
        {
            base.Awake();

            projectilePool = new PoolComponent<SimpleEnemyProjectileBehavior>("Mask Enemy Projectile", projectilePrefab, projectilesCount * 3);
            handPool = new PoolComponent<EnemyMaskHandBehavior>("Mask Enemy Hand", handPrefab, 6);
        }

        public override void Play()
        {
            base.Play();

            attackCoroutine = StartCoroutine(BossBehavior());
        }

        private IEnumerator BossBehavior()
        {
            while (IsAlive)
            {
                yield return new WaitForSeconds(attackCooldown);

                IsMoving = false;

                for (int i = 0; i < 3; i++)
                {
                    var playerPosition = PlayerBehavior.Player.transform.position.XY();

                    Vector2 position = Vector2.zero;
                    bool foundPosition = false;

                    for (int j = 0; j < 20; j++)
                    {
                        position = playerPosition + Random.insideUnitCircle.normalized * 2;

                        if(StageController.FieldManager.ValidatePosition(position, Vector2.one * 0.2f))
                        {
                            foundPosition = true;
                            break;
                        }
                    }

                    if (foundPosition)
                    {
                        var hand = handPool.GetEntity();
                        hand.transform.position = position;
                        hand.Damage = handDamage * StageController.Stage.EnemyDamage;

                        var direction = (playerPosition - position).normalized;

                        hand.Init(playerPosition + direction * 2f, 5);

                        hand.onFinished += OnProjectileFinished;

                        projectiles.Add(hand);
                    }

                    yield return new WaitForSeconds(0.7f);
                }

                IsMoving = true;

                yield return new WaitForSeconds(attackCooldown);

                IsMoving = false;

                animator.SetBool(IS_CHARING_BOOL, true);

                yield return new WaitForSeconds(attackCooldown);

                animator.SetBool(IS_CHARING_BOOL, false);

                IsMoving = true;
            }
        }

        public void WaveAttack()
        {
            var playerDirection = (PlayerBehavior.CenterPosition - transform.position.XY()).normalized;

            var attackRotation = Quaternion.FromToRotation(Vector2.up, playerDirection);
            var stepAngle = 360f / projectilesCount;

            for (int i = 0; i < projectilesCount; i++)
            {
                var rotation = attackRotation * Quaternion.Euler(0, 0, stepAngle * i);

                var direction = rotation * Vector2.up;

                var projectile = projectilePool.GetEntity();

                projectile.Init(transform.position, direction);
                projectile.Damage = GetDamage();

                projectile.onFinished += OnProjectileFinished;

                projectiles.Add(projectile);
            }
        }

        private void OnProjectileFinished(SimpleEnemyProjectileBehavior projectile)
        {
            projectile.onFinished -= OnProjectileFinished;

            projectiles.Remove(projectile);
        }

        protected override void Die(bool flash)
        {
            base.Die(flash);

            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            for(int i = 0; i < projectiles.Count; i++)
            {
                var projectile = projectiles[i];
                projectile.onFinished -= OnProjectileFinished;
                projectile.Disable();
            }

            projectiles.Clear();

            projectilePool.Destroy();
            handPool.Destroy();
        }
    }
}