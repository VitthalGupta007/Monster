using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class LightningAmuletAbilityBehavior : AbilityBehavior<LightningAmuletAbilityData, LightningAmuletAbilityLevel>
    {
        private static readonly int LIGHTNING_AMULET_HASH = "Lightning Amulet".GetHashCode();

        [SerializeField] GameObject lightningPrefab;
        public GameObject LightningPrefab => lightningPrefab;

        protected PoolComponent<ParticleSystem> lightningPool;

        private List<IEasingCoroutine> easingCoroutines = new List<IEasingCoroutine>();

        private Coroutine abilityCoroutine;

        private void Awake()
        {
            lightningPool = new PoolComponent<ParticleSystem>("Lightning ability particle", LightningPrefab, 6);
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
                for(int i = 0; i < AbilityLevel.LightningsCount; i++)
                {
                    yield return new WaitForSeconds(AbilityLevel.DurationBetweenHits);

                    var particle = lightningPool.GetEntity();

                    var spawner = StageController.EnemiesSpawner;
                    var enemy = spawner.GetRandomVisibleEnemy();

                    if(enemy != null)
                    {
                        particle.transform.position = enemy.transform.position;

                        enemy.TakeDamage(PlayerBehavior.Player.Damage * AbilityLevel.Damage);

                        var enemiesInRadius = StageController.EnemiesSpawner.GetEnemiesInRadius(enemy.transform.position, AbilityLevel.AdditionalDamageRadius);

                        foreach(var closeEnemy in enemiesInRadius)
                        {
                            if (closeEnemy != enemy) closeEnemy.TakeDamage(PlayerBehavior.Player.Damage * AbilityLevel.AdditionalDamage);
                        }
                    } else
                    {
                        particle.transform.position = PlayerBehavior.Player.transform.position + Vector3.up + Vector3.left;
                    }

                    IEasingCoroutine easingCoroutine = null;
                    easingCoroutine = EasingManager.DoAfter(1, () =>
                    {
                        particle.gameObject.SetActive(false);
                        easingCoroutines.Remove(easingCoroutine);
                    });

                    easingCoroutines.Add(easingCoroutine);

                    GameController.AudioManager.PlaySound(LIGHTNING_AMULET_HASH);
                }

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier - AbilityLevel.DurationBetweenHits);
            }
        }

        public override void Clear()
        {
            StopCoroutine(abilityCoroutine);

            for (int i = 0; i < easingCoroutines.Count; i++)
            {
                var easingCoroutine = easingCoroutines[i];
                if (easingCoroutine.ExistsAndActive()) easingCoroutine.Stop();
            }

            lightningPool.Destroy();

            base.Clear();
        }
    }
}