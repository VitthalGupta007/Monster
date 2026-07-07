using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class RecoilerAbilityBehavior : AbilityBehavior<RecoilerAbilityData, RecoilerAbilityLevel>
    {
        public static readonly int RECOILER_LAUNCH_HASH = "Recoiler Launch".GetHashCode();

        [SerializeField] GameObject recoilerPrefab;
        public GameObject RecoilerPrefab => recoilerPrefab;

        private PoolComponent<RecoilerProjectileBehavior> projectilePool;

        private Coroutine abilityCoroutine;
        private List<RecoilerProjectileBehavior> projectiles = new List<RecoilerProjectileBehavior>();

        private void Awake()
        {
            projectilePool = new PoolComponent<RecoilerProjectileBehavior>("Recoiler Ability Projectile", RecoilerPrefab, 5);
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
                SpawnProjectile(0);
                SpawnProjectile(180);

                GameController.AudioManager.PlaySound(RECOILER_LAUNCH_HASH);

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier);
            }
        }

        private void SpawnProjectile(float startingAngle)
        {
            var projectile = projectilePool.GetEntity();

            projectile.transform.position = PlayerBehavior.CenterPosition;

            projectile.DamageMultiplier = AbilityLevel.Damage;
            projectile.ProjectileLifetime = AbilityLevel.ProjectileLifetime;
            projectile.ProjectileTravelDistance = AbilityLevel.ProjectileTravelDistance;
            projectile.Size = AbilityLevel.ProjectileSize;
            projectile.AngularSpeed = AbilityLevel.AngularSpeed;

            projectile.onRecoilerFinished += OnRecoilerFinished;

            projectile.Spawn(startingAngle);

            projectiles.Add(projectile);
        }

        private void Disable()
        {
            if (abilityCoroutine != null) StopCoroutine(abilityCoroutine);

            for (int i = 0; i < projectiles.Count; i++)
            {
                var projectile = projectiles[i];

                projectile.onRecoilerFinished -= OnRecoilerFinished;

                projectile.Disable();
            }

            projectiles.Clear();
        }

        private void OnRecoilerFinished(RecoilerProjectileBehavior recoiler)
        {
            recoiler.onRecoilerFinished -= OnRecoilerFinished;

            projectiles.Remove(recoiler);
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}