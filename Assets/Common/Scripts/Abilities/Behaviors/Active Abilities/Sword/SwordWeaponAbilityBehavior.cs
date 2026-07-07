using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class SwordWeaponAbilityBehavior : AbilityBehavior<SteelSwordWeaponAbilityData, SteelSwordWeaponAbilityLevel>
    {
        public static readonly int STEEL_SWORD_ATTACK_HASH = "Steel Sword Attack".GetHashCode();

        [SerializeField] GameObject slashPrefab;
        public GameObject SlashPrefab => slashPrefab;

        private PoolComponent<SwordSlashBehavior> slashPool;
        private List<SwordSlashBehavior> slashes = new List<SwordSlashBehavior>();

        [SerializeField] List<Transform> shashDirections;

        IEasingCoroutine projectileCoroutine;
        Coroutine abilityCoroutine;

        private float AbilityCooldown => AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier;

        private void Awake()
        {
            slashPool = new PoolComponent<SwordSlashBehavior>("Sword Slash", SlashPrefab, 50);
        }

        protected override void SetAbilityLevel(int stageId)
        {
            base.SetAbilityLevel(stageId);

            if (abilityCoroutine != null) Disable();

            abilityCoroutine = StartCoroutine(AbilityCoroutine());
        }

        private IEnumerator AbilityCoroutine()
        {
            var lastTimeSpawned = Time.time - AbilityCooldown;

            while (true)
            {
                for(int i = 0; i < AbilityLevel.SlashesCount; i++)
                {
                    var slash = slashPool.GetEntity();

                    slash.transform.position = PlayerBehavior.CenterPosition;
                    slash.transform.rotation = Quaternion.FromToRotation(Vector2.right, PlayerBehavior.Player.LookDirection) * shashDirections[i].localRotation;

                    slash.DamageMultiplier = AbilityLevel.Damage;
                    slash.KickBack = false;

                    slash.Size = AbilityLevel.SlashSize;

                    slash.Init();

                    slash.onFinished += OnProjectileFinished;
                    slashes.Add(slash);

                    GameController.AudioManager.PlaySound(STEEL_SWORD_ATTACK_HASH);

                    yield return new WaitForSeconds(AbilityLevel.TimeBetweenSlashes * PlayerBehavior.Player.CooldownMultiplier);
                }

                yield return new WaitForSeconds(AbilityLevel.AbilityCooldown * PlayerBehavior.Player.CooldownMultiplier - AbilityLevel.TimeBetweenSlashes * PlayerBehavior.Player.CooldownMultiplier * AbilityLevel.SlashesCount);
            }
        }

        private void OnProjectileFinished(SwordSlashBehavior slash)
        {
            slash.onFinished -= OnProjectileFinished;

            slashes.Remove(slash);
        }

        private void Disable()
        {
            projectileCoroutine.StopIfExists();

            for (int i = 0; i < slashes.Count; i++)
            {
                slashes[i].Disable();
            }

            slashes.Clear();

            StopCoroutine(abilityCoroutine);
        }

        public override void Clear()
        {
            Disable();

            base.Clear();
        }
    }
}