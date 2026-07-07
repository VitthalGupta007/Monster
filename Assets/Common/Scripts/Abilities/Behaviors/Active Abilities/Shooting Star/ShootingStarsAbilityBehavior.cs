using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class ShootingStarsAbilityBehavior : AbilityBehavior<ShootingStarAbilityData, ShootingStartAbilityLevel>
    {
        public static readonly int SHOOTING_STARS_LAUNCH_HASH = "Shooting Stars Launch".GetHashCode();

        [SerializeField] GameObject starPrefab;
        public GameObject StarPrefab => starPrefab;

        private List<ShootingStarProjectile> stars = new List<ShootingStarProjectile>();
        float angle = 0;

        private float radiusMultiplier = 1;

        private PoolComponent<ShootingStarProjectile> projectilesPool;

        private Coroutine abilityCoroutine;

        private void Awake()
        {
            projectilesPool = new PoolComponent<ShootingStarProjectile>("Shooting Star Ability Projectile", starPrefab, 6);
        }

        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            if(abilityCoroutine != null) StopCoroutine(abilityCoroutine);

            for(int i = 0; i < stars.Count; i++)
            {
                stars[i].gameObject.SetActive(false);
            }

            stars.Clear();

            abilityCoroutine = StartCoroutine(AbilityCoroutine());
        }

        private IEnumerator AbilityCoroutine()
        {
            while (true)
            {
                float lifetime = AbilityLevel.ProjectileLifetime * PlayerBehavior.Player.DurationMultiplier;
                float cooldown = AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier;

                for (int i = 0; i < AbilityLevel.ProjectilesCount; i++)
                {
                    var star = projectilesPool.GetEntity();

                    star.Init();

                    star.DamageMultiplier = AbilityLevel.Damage;
                    star.KickBack = true;

                    star.Spawn();

                    stars.Add(star);
                }

                EasingManager.DoFloat(0, 1, 0.5f, value => radiusMultiplier = value).SetEasing(EasingType.SineOut);

                yield return new WaitForSeconds(lifetime - 0.5f);

                for(int i = 0; i < stars.Count; i++)
                {
                    var star = stars[i];

                    star.Hide();
                }

                EasingManager.DoFloat(1, 0, 0.5f, value => radiusMultiplier = value).SetEasing(EasingType.SineOut);

                float delay = cooldown - lifetime;
                if (delay < 0.5f) delay = 0.5f;

                GameController.AudioManager.PlaySound(SHOOTING_STARS_LAUNCH_HASH);

                yield return new WaitForSeconds(delay);

                stars.Clear();
            }
        }

        private void LateUpdate()
        {
            transform.position = PlayerBehavior.CenterPosition;

            angle += AbilityLevel.AngularSpeed * PlayerBehavior.Player.ProjectileSpeedMultiplier * Time.deltaTime;

            for (int i = 0; i < stars.Count; i++)
            {
                float projectileAngle = 360f / stars.Count * i;

                projectileAngle += angle;

                stars[i].transform.localPosition = transform.position + Quaternion.Euler(0, 0, projectileAngle) * Vector3.up * AbilityLevel.Radius * radiusMultiplier * PlayerBehavior.Player.SizeMultiplier;
            }
        }

        public override void Clear()
        {
            if (abilityCoroutine != null) StopCoroutine(abilityCoroutine);

            for (int i = 0; i < stars.Count; i++)
            {
                stars[i].Clear();
            }

            stars.Clear();

            projectilesPool.Destroy();

            base.Clear();
        }
    }
}