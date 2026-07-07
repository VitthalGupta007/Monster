using VXMonster.Core.Easing;
using VXMonster.Core.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VXMonster.Core.Abilities
{
    public class ThunderRingAbilityBehavior : AbilityBehavior<ThunderRingAbilityData, ThunderRingAbilityLevel>
    {
        public static readonly int THUNDER_RING_STRIKE_HASH = "Thunder Ring Strike".GetHashCode();

        [SerializeField] GameObject lightningPrefab;
        public GameObject LightningPrefab => lightningPrefab;

        [SerializeField] GameObject ballLightningPrefab;
        public GameObject BallLightningPrefab => ballLightningPrefab;

        protected PoolComponent<ParticleSystem> lightningPool;
        protected PoolComponent<SimplePlayerProjectileBehavior> ballLightningPool;

        private Coroutine abilityCoroutine;

        private List<IEasingCoroutine> easingCoroutines = new List<IEasingCoroutine>();

        private void Awake()
        {
            lightningPool = new PoolComponent<ParticleSystem>("Ball Lightning ability particle", LightningPrefab, 6);
            ballLightningPool = new PoolComponent<SimplePlayerProjectileBehavior>("Ball Lightning Secondary Projectile", BallLightningPrefab, 6);
        }

        public override void Init(AbilityData data, int stageId)
        {
            base.Init(data, stageId);

            abilityCoroutine = StartCoroutine(AbilityCoroutine());
        }

        private IEnumerator AbilityCoroutine()
        {
            while (true)
            {
                for (int i = 0; i < AbilityLevel.LightningsCount; i++)
                {
                    yield return new WaitForSeconds(AbilityLevel.DurationBetweenHits);

                    var particle = lightningPool.GetEntity();

                    var spawner = StageController.EnemiesSpawner;
                    var enemy = spawner.GetRandomVisibleEnemy();

                    if (enemy != null)
                    {
                        particle.transform.position = enemy.transform.position;

                        enemy.TakeDamage(PlayerBehavior.Player.Damage * AbilityLevel.Damage);
                    }
                    else
                    {
                        particle.transform.position = PlayerBehavior.Player.transform.position + Vector3.up + Vector3.left;
                    }

                    float angle = Random.Range(0, 180f);

                    for (int j = 0; j < AbilityLevel.BallLightningCount; j++) 
                    {
                        var ball = ballLightningPool.GetEntity();

                        angle += 360f / AbilityLevel.BallLightningCount;

                        ball.Init(particle.transform.position, Quaternion.Euler(0, 0, angle) * Vector2.up);

                        ball.DamageMultiplier = AbilityLevel.BallDamage;
                        ball.KickBack = false;
                    }

                    IEasingCoroutine easingCoroutine = null;
                    easingCoroutine = EasingManager.DoAfter(1, () =>
                    {
                        particle.gameObject.SetActive(false);
                        easingCoroutines.Remove(easingCoroutine);
                    });

                    easingCoroutines.Add(easingCoroutine);

                    GameController.AudioManager.PlaySound(THUNDER_RING_STRIKE_HASH);
                }

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier - AbilityLevel.DurationBetweenHits);
            }
        }

        public override void Clear()
        {
            StopCoroutine(abilityCoroutine);

            for(int i = 0; i < easingCoroutines.Count; i++)
            {
                var easingCoroutine = easingCoroutines[i];
                if (easingCoroutine.ExistsAndActive()) easingCoroutine.Stop();
            }

            lightningPool.Destroy();
            ballLightningPool.Destroy();

            base.Clear();
        }
    }
}