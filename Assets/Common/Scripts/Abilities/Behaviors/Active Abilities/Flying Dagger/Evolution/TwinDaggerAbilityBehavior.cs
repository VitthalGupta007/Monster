using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class TwinDaggerAbilityBehavior : AbilityBehavior<TwinDaggerAbilityData, TwinDaggerAbilityLevel>
    {
        public static readonly int TWIN_DAGGERS_HASH = "Twin Daggers Launch".GetHashCode();

        [SerializeField] GameObject daggerPrefab;
        public GameObject DaggerPrefab => daggerPrefab;

        private PoolComponent<SimplePlayerProjectileBehavior> projectilePool;

        private Coroutine abilityCoroutine;
        private List<SimplePlayerProjectileBehavior> projectiles = new List<SimplePlayerProjectileBehavior>();

        private void Awake()
        {
            projectilePool = new PoolComponent<SimplePlayerProjectileBehavior>("Twin Dagger Ability Projectile", DaggerPrefab, 5);
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

                    projectile.DamageMultiplier = AbilityLevel.Damage;

                    projectile.onFinished += OnProjectileFinished;

                    var angle = 360f / AbilityLevel.ProjectileCount * i;

                    projectile.Init(projectile.transform.position, Quaternion.Euler(0, 0, angle) * Vector2.up);
                    projectile.KickBack = true;
                    projectiles.Add(projectile);
                }

                GameController.AudioManager.PlaySound(TWIN_DAGGERS_HASH);

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier);
            }
        }

        private void Disable()
        {
            if (abilityCoroutine != null) StopCoroutine(abilityCoroutine);

            for (int i = 0; i < projectiles.Count; i++)
            {
                var projectile = projectiles[i];

                projectile.onFinished -= OnProjectileFinished;

                projectile.Clear();
            }

            projectiles.Clear();
        }

        private void OnProjectileFinished(SimplePlayerProjectileBehavior projectile)
        {
            projectile.onFinished -= OnProjectileFinished;

            projectiles.Remove(projectile);
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}