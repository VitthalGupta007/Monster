using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class BoomerangAbilityBehavior : AbilityBehavior<BoomerangAbilityData, BoomerangAbilityLevel>
    {
        [SerializeField] GameObject boomerangPrefab;
        public GameObject BoomerangPrefab => boomerangPrefab;

        private PoolComponent<BoomerangProjectileBehavior> projectilePool;

        private Coroutine abilityCoroutine;
        private List<BoomerangProjectileBehavior> projectiles = new List<BoomerangProjectileBehavior>();

        private void Awake()
        {
            projectilePool = new PoolComponent<BoomerangProjectileBehavior>("Boomerang Ability Projectile", BoomerangPrefab, 5);
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
                for(int i = 0; i < AbilityLevel.ProjectileCount; i++)
                {
                    var projectile = projectilePool.GetEntity();

                    projectile.transform.position = PlayerBehavior.CenterPosition;

                    var enemy = StageController.EnemiesSpawner.GetClosestEnemy(projectile.transform.position);

                    Vector2 direction;
                    if(enemy == null)
                    {
                        direction = Random.insideUnitCircle.normalized;
                    } else
                    {
                        direction = (enemy.Center - projectile.transform.position.XY()).normalized;
                    }

                    projectile.DamageMultiplier = AbilityLevel.Damage;
                    projectile.ProjectileLifetime = AbilityLevel.ProjectileLifetime;
                    projectile.ProjectileTravelDistance = AbilityLevel.ProjectileTravelDistance;
                    projectile.Size = AbilityLevel.ProjectileSize;

                    projectile.onBoomerangFinished += OnBoomerangFinished;

                    projectile.Spawn(direction);

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

                projectile.onBoomerangFinished -= OnBoomerangFinished;

                projectile.Disable();
            }

            projectiles.Clear();
        }

        private void OnBoomerangFinished(BoomerangProjectileBehavior boomerang)
        {
            boomerang.onBoomerangFinished -= OnBoomerangFinished;

            projectiles.Remove(boomerang);
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}