using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class FlyingDaggerAbilityBehavior : AbilityBehavior<FlyingDaggerAbilityData, FlyingDaggerAbilityLevel>
    {
        [SerializeField] GameObject daggerPrefab;
        public GameObject DaggerPrefab => daggerPrefab;

        private PoolComponent<FlyingDaggerProjectileBehavior> projectilePool;

        private Coroutine abilityCoroutine;
        private List<FlyingDaggerProjectileBehavior> projectiles = new List<FlyingDaggerProjectileBehavior>();

        private void Awake()
        {
            projectilePool = new PoolComponent<FlyingDaggerProjectileBehavior>("Flying Dagger Ability Projectile", DaggerPrefab, 5);
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

                    projectile.transform.position = PlayerBehavior.CenterPosition;

                    var deviation = AbilityLevel.Spread / 2f;
                    Vector3 direction = Quaternion.Euler(0, 0, Random.Range(-deviation, deviation)) * Vector2.up;

                    projectile.DamageMultiplier = AbilityLevel.Damage;
                    projectile.ProjectileLifetime = AbilityLevel.ProjectileLifetime;
                    projectile.Size = AbilityLevel.ProjectileSize;

                    projectile.onFinished += OnDaggerFinished;

                    projectile.Spawn(direction * AbilityLevel.ThrowingForce * PlayerBehavior.Player.ProjectileSpeedMultiplier);

                    projectiles.Add(projectile);

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

                projectile.onFinished -= OnDaggerFinished;

                projectile.Disable();
            }

            projectiles.Clear();
        }

        private void OnDaggerFinished(FlyingDaggerProjectileBehavior dagger)
        {
            dagger.onFinished -= OnDaggerFinished;

            projectiles.Remove(dagger);
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}