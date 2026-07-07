using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class MeteorAbilityBehavior : AbilityBehavior<MeteorAbilityData, MeteorAbilityLevel>
    {
        [SerializeField] GameObject meteorPrefab;
        public GameObject MeteorPrefab => meteorPrefab;

        private PoolComponent<MeteorProjectileBehavior> meteorPool;

        private Coroutine abilityCoroutine;

        private List<MeteorProjectileBehavior> aliveMeteors = new List<MeteorProjectileBehavior>();

        private void Awake()
        {
            meteorPool = new PoolComponent<MeteorProjectileBehavior>("Meteor Ability Projectile", MeteorPrefab, 6);
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
                for (int i = 0; i < AbilityLevel.ProjectilesCount; i++)
                {
                    var meteor = meteorPool.GetEntity();

                    meteor.transform.position = PlayerBehavior.CenterPosition;

                    meteor.DamageMultiplier = AbilityLevel.Damage;
                    meteor.ExplosionRadius = AbilityLevel.ExplosionRadius;

                    meteor.onFinished += OnFireballFinished;

                    var impactPosition = PlayerBehavior.CenterPosition;

                    int counter = 0;
                    while(Vector3.Distance(impactPosition, PlayerBehavior.CenterPosition) < 0.5f)
                    {
                        if (counter > 10) break;

                        impactPosition = CameraManager.GetPointInsideCamera();

                        counter++;
                    }

                    meteor.Init(impactPosition);

                    aliveMeteors.Add(meteor);

                    yield return new WaitForSeconds(AbilityLevel.TimeBetweenProjectiles);
                }

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier - AbilityLevel.TimeBetweenProjectiles * AbilityLevel.ProjectilesCount);
            }
        }

        private void OnFireballFinished(MeteorProjectileBehavior meteor)
        {
            meteor.onFinished -= OnFireballFinished;

            aliveMeteors.Remove(meteor);
        }

        public void Disable()
        {
            StopCoroutine(abilityCoroutine);

            for (int i = 0; i < aliveMeteors.Count; i++)
            {
                var meteor = aliveMeteors[i];

                meteor.onFinished -= OnFireballFinished;

                meteor.Clear();
            }

            aliveMeteors.Clear();
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}