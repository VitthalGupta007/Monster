using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class VoidStarAbilityBehavior : AbilityBehavior<VoidStarAbilityData, VoidStartAbilityLevel>
    {
        public static readonly int VOID_STARS_LAUNCH_HASH = "Void Stars Launch".GetHashCode();

        [SerializeField] GameObject starPrefab;
        public GameObject StarPrefab => starPrefab;

        private List<ShootingStarProjectile> stars = new List<ShootingStarProjectile>();
        float angle = 0;

        private float radiusMultiplier = 1;

        private PoolComponent<ShootingStarProjectile> projectilesPool;

        private void Awake()
        {
            projectilesPool = new PoolComponent<ShootingStarProjectile>("Void Star Ability Projectile", starPrefab, 6);
        }

        public override void Init(AbilityData data, int levelId)
        {
            base.Init(data, levelId);

            GameController.AudioManager.PlaySound(VOID_STARS_LAUNCH_HASH);
        }

        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            for (int i = 0; i < stars.Count; i++)
            {
                stars[i].gameObject.SetActive(false);
            }

            stars.Clear();

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