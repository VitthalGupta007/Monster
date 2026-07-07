using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class FireballAbilityBehavior : AbilityBehavior<FireballAbilityData, FireballAbilityLevel>
    {
        [SerializeField] GameObject fireballPrefab;
        public GameObject FireballPrefab => fireballPrefab;

        private PoolComponent<FireballProjectileBehavior> fireballPool;

        private Coroutine abilityCoroutine;

        private List<FireballProjectileBehavior> aliveFireballs = new List<FireballProjectileBehavior>();

        private void Awake()
        {
            fireballPool = new PoolComponent<FireballProjectileBehavior>("Fireball Ability Projectile", FireballPrefab, 6);
        }

        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            if (abilityCoroutine != null) Disable();

            abilityCoroutine = StartCoroutine(AbilityCoroutine());
        }

        private IEnumerator AbilityCoroutine()
        {
            while (true)
            {
                for(int i = 0; i < AbilityLevel.ProjectilesCount; i++)
                {
                    var fireball = fireballPool.GetEntity();

                    fireball.transform.position = PlayerBehavior.CenterPosition;

                    var closestEnemy = StageController.EnemiesSpawner.GetClosestEnemy(PlayerBehavior.CenterPosition);

                    if(closestEnemy != null)
                    {
                        Vector2 direction = (closestEnemy.Center - fireball.transform.position.XY()).normalized;
                        fireball.transform.rotation = Quaternion.FromToRotation(Vector2.up, direction);
                    } else
                    {
                        fireball.transform.rotation = Quaternion.FromToRotation(Vector2.up, Random.insideUnitCircle.normalized);
                    }

                    fireball.DamageMultiplier = AbilityLevel.Damage;
                    fireball.Speed = AbilityLevel.Speed;
                    fireball.Lifetime = AbilityLevel.FireballLifetime;
                    fireball.Size = AbilityLevel.ProjectileSize;
                    fireball.ExplosionRadius = AbilityLevel.ExplosionRadius;

                    fireball.onFinished += OnFireballFinished;

                    fireball.Init();

                    aliveFireballs.Add(fireball);

                    yield return new WaitForSeconds(AbilityLevel.TimeBetweenFireballs);
                }

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier - AbilityLevel.TimeBetweenFireballs * AbilityLevel.ProjectilesCount);
            }
        }

        private void OnFireballFinished(FireballProjectileBehavior fireball)
        {
            fireball.onFinished -= OnFireballFinished;

            aliveFireballs.Remove(fireball);
        }

        public void Disable()
        {
            StopCoroutine(abilityCoroutine);

            for(int i = 0; i < aliveFireballs.Count; i++)
            {
                var fireball = aliveFireballs[i];

                fireball.onFinished -= OnFireballFinished;

                fireball.Clear();
            }

            aliveFireballs.Clear();
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}