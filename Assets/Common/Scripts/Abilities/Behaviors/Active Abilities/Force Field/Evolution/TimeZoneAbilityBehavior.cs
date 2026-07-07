using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class TimeZoneAbilityBehavior : AbilityBehavior<TimeGazerAbilityData, TimeGazerAbilityLevel>
    {
        public static readonly int TIME_GAZER_HASH = "Time Gazer".GetHashCode();

        [SerializeField] CircleCollider2D abilityCollider;
        [SerializeField] Transform visuals;

        private Dictionary<EnemyBehavior, float> enemies = new Dictionary<EnemyBehavior, float>();

        private Effect slowDownEffect;

        private float lastTimeSound;

        private void Awake()
        {
            slowDownEffect = new Effect(EffectType.Speed, 1);
            lastTimeSound = -100;
        }

        public override void Init(AbilityData data, int stageId)
        {
            base.Init(data, stageId);

            slowDownEffect.SetModifier(AbilityLevel.SlowDownMultiplier);
        }

        private void LateUpdate()
        {
            transform.position = PlayerBehavior.CenterPosition;

            visuals.localScale = Vector2.one * AbilityLevel.FieldRadius * 2 * PlayerBehavior.Player.SizeMultiplier;
            abilityCollider.radius = AbilityLevel.FieldRadius * PlayerBehavior.Player.SizeMultiplier;

            if(Time.time - lastTimeSound > 5)
            {
                lastTimeSound = Time.time;

                GameController.AudioManager.PlaySound(TIME_GAZER_HASH);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            EnemyBehavior enemy = collision.GetComponent<EnemyBehavior>();

            if (!enemies.ContainsKey(enemy))
            {
                enemies.Add(enemy, Time.time);

                enemy.TakeDamage(AbilityLevel.Damage * PlayerBehavior.Player.Damage);

                enemy.AddEffect(slowDownEffect);
            }
        }

        private void Update()
        {
            foreach (var enemy in enemies.Keys.ToList())
            {
                float time = enemies[enemy];

                if (Time.time - time > AbilityLevel.DamageCooldown * PlayerBehavior.Player.CooldownMultiplier)
                {
                    enemies[enemy] = Time.time;

                    enemy.TakeDamage(AbilityLevel.Damage * PlayerBehavior.Player.Damage);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            EnemyBehavior enemy = collision.GetComponent<EnemyBehavior>();

            if (enemies.ContainsKey(enemy))
            {
                enemies.Remove(enemy);

                enemy.RemoveEffect(slowDownEffect);
            }
        }
    }
}