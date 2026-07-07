using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class VoidBlackHoleBehavior : MonoBehaviour
    {
        protected static readonly int HIDE_TRIGGER = Animator.StringToHash("Hide");

        [SerializeField] protected Animator animator;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected ParticleSystem chargeParticle;

        [SerializeField] protected float projectileDamage = 10f;

        protected Coroutine damageCoroutine;

        public float Damage { get; set; }

        protected bool IsHiding { get; set; }

        protected List<SimpleEnemyProjectileBehavior> projectiles = new List<SimpleEnemyProjectileBehavior>();

        public virtual void Hide() 
        {
            animator.SetTrigger(HIDE_TRIGGER);

            IsHiding = true;
        }

        public virtual void OnHidden()
        {
            if(IsHiding)
            {
                IsHiding = false;
                gameObject.SetActive(false);
            }
        }

        public virtual void Charge(PoolComponent<SimpleEnemyProjectileBehavior> projectilePool)
        {
            chargeParticle.Play();

            EasingManager.DoAfter(0.6f, () =>
            {
                for(int i = 0; i < 10; i++)
                {
                    var angle = 360f / 10 * i;
                    var projectile = projectilePool.GetEntity();
                    projectile.onFinished += OnProjectileFinished;
                    projectile.Damage = projectileDamage * StageController.Stage.EnemyDamage;
                    projectiles.Add(projectile);
                    projectile.Init(transform.position, Quaternion.Euler(0, 0, angle) * Vector2.up);
                } 
            });
        }

        protected virtual void OnProjectileFinished(SimpleEnemyProjectileBehavior projectile)
        {
            projectile.onFinished -= OnProjectileFinished;
            projectiles.Remove(projectile);
        }

        protected virtual IEnumerator DamageCoroutine()
        {
            while(true)
            {
                yield return new WaitForSeconds(1f);

                PlayerBehavior.Player.TakeDamage(Damage);
            }
        }

        public virtual bool Intersects(VoidBlackHoleBehavior other)
        {
            return spriteRenderer.bounds.Intersects(other.spriteRenderer.bounds);
        }

        public virtual bool Contains(Vector2 position)
        {
            Vector3 position3d = ((Vector3)position).SetZ(spriteRenderer.bounds.center.z);
            return spriteRenderer.bounds.Contains(position3d);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.GetComponent<PlayerEnemyCollisionHelper>() != null)
            {
                damageCoroutine = StartCoroutine(DamageCoroutine());
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.GetComponent<PlayerEnemyCollisionHelper>() != null)
            {
                StopCoroutine(damageCoroutine);
            }
        }

        public virtual void Clear()
        {
            for(int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].onFinished -= OnProjectileFinished;
                projectiles[i].Disable();
            }

            projectiles.Clear();
            gameObject.SetActive(false);
        }
    }
}