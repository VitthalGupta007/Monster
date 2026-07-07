using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class SolarMagnifierAbilityBehavior : AbilityBehavior<SolarMagnifierAbilityData, SolarMagnifierAbilityLevel>
    {
        public static readonly int SOLAR_MAGNIFIER_LAUNCH_HASH = "Solar Magnifier Launch".GetHashCode();

        [SerializeField] GameObject sunRayPrefab;
        public GameObject SunRayPrefab => sunRayPrefab;

        private PoolComponent<SunRayProjectileBehavior> projectilePool;

        private Coroutine abilityCoroutine;
        private List<SunRayProjectileBehavior> projectiles = new List<SunRayProjectileBehavior>();

        private void Awake()
        {
            projectilePool = new PoolComponent<SunRayProjectileBehavior>("Sun Ray Ability Projectile", SunRayPrefab, 5);
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

                    var enemy = StageController.EnemiesSpawner.GetRandomVisibleEnemy();

                    projectile.DamagePerSecond = AbilityLevel.DamagePerSecond;
                    projectile.AdditionalDamagePerSecond = AbilityLevel.AdditionalDamagePerSecond;
                    projectile.DamageInterval = 1f / AbilityLevel.DamageRate;
                    projectile.AdditionalDamageRadius = AbilityLevel.AdditionalDamageRadius;

                    projectile.Lifetime = AbilityLevel.ProjectileLifetime * PlayerBehavior.Player.DurationMultiplier;
                    projectile.Speed = AbilityLevel.ProjectileSpeed;

                    projectile.onFinished += OnProjectileFinished;

                    projectile.Spawn(enemy);

                    projectiles.Add(projectile);

                    GameController.AudioManager.PlaySound(SOLAR_MAGNIFIER_LAUNCH_HASH);

                    yield return new WaitForSeconds(AbilityLevel.TimeBetweenProjectiles);
                }

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier - AbilityLevel.TimeBetweenProjectiles * AbilityLevel.ProjectileCount);
            }
        }

        private void Disable()
        {
            if (abilityCoroutine != null) StopCoroutine(abilityCoroutine);

            for (int i = 0; i < projectiles.Count; i++)
            {
                var projectile = projectiles[i];

                projectile.onFinished -= OnProjectileFinished;

                projectile.Disable();
            }

            projectiles.Clear();
        }

        private void OnProjectileFinished(SunRayProjectileBehavior sunRay)
        {
            sunRay.onFinished -= OnProjectileFinished;

            projectiles.Remove(sunRay);
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}