using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class IceShardAbilityBehavior : AbilityBehavior<IceShardAbilityData, IceShardAbilityLevel>
    {
        private static readonly int ICE_SHARD_LAUNCH_HASH = "Ice Shard Launch".GetHashCode();

        [SerializeField] GameObject iceShardPrefab;
        public GameObject IceShardPrefab => iceShardPrefab;

        private PoolComponent<IceShardProjectileBehavior> projectilePool;
        public List<IceShardProjectileBehavior> projectiles = new List<IceShardProjectileBehavior>();

        IEasingCoroutine projectileCoroutine;
        Coroutine abilityCoroutine;

        private void Awake()
        {
            projectilePool = new PoolComponent<IceShardProjectileBehavior>("Ice Shard Projectiule", IceShardPrefab, 6);
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
                var newProjectiles = new List<IceShardProjectileBehavior>();

                for (int i = 0; i < AbilityLevel.ProjectilesCount; i++)
                {
                    var projectile = projectilePool.GetEntity();

                    projectile.transform.position = PlayerBehavior.CenterPosition;
                    projectile.SetData(AbilityLevel.ProjectileSize, AbilityLevel.Damage, AbilityLevel.ProjectileSpeed);
                    projectile.Direction = Random.onUnitSphere.SetZ(0).normalized;

                    projectiles.Add(projectile);
                    newProjectiles.Add(projectile);
                }

                GameController.AudioManager.PlaySound(ICE_SHARD_LAUNCH_HASH);

                projectileCoroutine = EasingManager.DoAfter(AbilityLevel.ProjectileLifetime * PlayerBehavior.Player.DurationMultiplier, () => {
                    for (int i = 0; i < newProjectiles.Count; i++)
                    {
                        newProjectiles[i].gameObject.SetActive(false);
                        projectiles.Remove(newProjectiles[i]);
                    }

                    newProjectiles.Clear();
                });

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier);
            }
        }

        private void Disable()
        {
            projectileCoroutine.StopIfExists();

            for (int i = 0; i < projectiles.Count; i++)
            {
                projectiles[i].gameObject.SetActive(false);
            }

            projectiles.Clear();

            StopCoroutine(abilityCoroutine);
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}