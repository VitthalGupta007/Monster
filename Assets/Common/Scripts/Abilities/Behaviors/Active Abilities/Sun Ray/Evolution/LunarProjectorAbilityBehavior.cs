using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class LunarProjectorAbilityBehavior : AbilityBehavior<LunarProjectorAbilityData, LunarProjectorAbilityLevel>
    {
        public static readonly int LUNAR_PROJECTOR_LAUNCH_HASH = "Eclipse Ray Launch".GetHashCode();

        [SerializeField] GameObject eclipseRayPrefab;
        public GameObject EclipseRayPrefab => eclipseRayPrefab;

        private PoolComponent<EclipseRayProjectileBehavior> projectilePool;

        private Coroutine abilityCoroutine;
        private List<EclipseRayProjectileBehavior> projectiles = new List<EclipseRayProjectileBehavior>();

        private void Awake()
        {
            projectilePool = new PoolComponent<EclipseRayProjectileBehavior>("Eclipse Ray Ability Projectile", EclipseRayPrefab, 5);
        }

        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            Disable();

            abilityCoroutine = StartCoroutine(AbilityCoroutine());
        }

        private IEnumerator AbilityCoroutine()
        {
            while (true)
            {
                for (int i = 0; i < AbilityLevel.ProjectileCount; i++)
                {
                    var projectile = projectilePool.GetEntity();

                    projectile.transform.position = PlayerBehavior.Player.transform.position;

                    projectile.DamageMultiplier = AbilityLevel.Damage;
                    projectile.ProjectileLifetime = AbilityLevel.ProjectileLifetime;
                    projectile.InitialRadius = AbilityLevel.InitialRadius;
                    projectile.AngularSpeed = AbilityLevel.AngularSpeed;
                    projectile.onFinished += OnRayFinished;

                    var angle = 360f / AbilityLevel.ProjectileCount * i;

                    projectile.Spawn(angle);

                    projectiles.Add(projectile);
                }

                GameController.AudioManager.PlaySound(LUNAR_PROJECTOR_LAUNCH_HASH);

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier);
            }
        }

        private void Disable()
        {
            if (abilityCoroutine != null) StopCoroutine(abilityCoroutine);

            for (int i = 0; i < projectiles.Count; i++)
            {
                var projectile = projectiles[i];

                projectile.onFinished -= OnRayFinished;

                projectile.Disable();
            }

            projectiles.Clear();
        }

        private void OnRayFinished(EclipseRayProjectileBehavior ray)
        {
            ray.onFinished -= OnRayFinished;

            projectiles.Remove(ray);
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}