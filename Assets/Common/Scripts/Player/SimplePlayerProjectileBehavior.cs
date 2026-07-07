using OctoberStudio.Easing;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class SimplePlayerProjectileBehavior : ProjectileBehavior
    {
        [SerializeField] protected float speed;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected TrailRenderer trail;
        [SerializeField] protected float lifetime = -1;
        [SerializeField] protected Transform rotatingPart;
        [SerializeField] protected bool selfDestructOnHit = true;
        [SerializeField] protected List<ParticleSystem> particles;

        protected Vector3 direction;
        protected float spawnTime;

        public float Speed { get; set; }
        public float LifeTime { get; set; }

        protected IEasingCoroutine scaleCoroutine;

        public UnityAction<SimplePlayerProjectileBehavior> onFinished;

        public virtual void Init(Vector2 position, Vector2 direction)
        {
            Init();

            transform.position = position;
            transform.localScale = Vector3.one * PlayerBehavior.Player.SizeMultiplier;
            this.direction = direction;

            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles[i].Clear();
                particles[i].Play();
            }

            if (rotatingPart != null) rotatingPart.rotation = Quaternion.FromToRotation(Vector2.up, direction);

            Speed = speed * PlayerBehavior.Player.ProjectileSpeedMultiplier;

            spawnTime = Time.time;

            LifeTime = lifetime * PlayerBehavior.Player.DurationMultiplier;

            if (trail != null) trail.Clear();
        }

        protected virtual void Update()
        {
            if (spriteRenderer != null && !spriteRenderer.isVisible) Clear();

            transform.position += direction * Time.deltaTime * Speed;

            if (LifeTime > 0)
            {
                if(Time.time - spawnTime > LifeTime)
                {
                    Clear();

                    onFinished?.Invoke(this);
                }
            }
        }

        public virtual IEasingCoroutine ScaleRotatingPart(Vector3 initialScale, Vector3 targetScale)
        {
            if (rotatingPart == null) return null;

            rotatingPart.localScale = initialScale;

            scaleCoroutine = rotatingPart.DoLocalScale(targetScale, 0.25f);

            return scaleCoroutine;
        }

        public virtual void Clear()
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particles[i].Clear();
            }

            scaleCoroutine.StopIfExists();
            gameObject.SetActive(false);
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (selfDestructOnHit)
            {
                Clear();

                onFinished?.Invoke(this);
            }
        }
    }
}