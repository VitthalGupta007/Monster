using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class GuardianEyeAbilityBehavior : AbilityBehavior<GuardianEyeAbilityData, GuardianEyeAbilityLevel>
    {
        private static readonly int GUARDIAN_EYE_HASH = "Guardian Eye".GetHashCode();

        [SerializeField] CircleCollider2D abilityCollider;
        [SerializeField] Transform visuals;

        private Dictionary<EnemyBehavior, float> enemies = new Dictionary<EnemyBehavior, float>();

        private float lastTimeSound;

        private void Awake()
        {
            lastTimeSound = -100;
        }

        private void LateUpdate()
        {
            transform.position = PlayerBehavior.CenterPosition;

            visuals.localScale = Vector2.one * AbilityLevel.FieldRadius * 2 * PlayerBehavior.Player.SizeMultiplier;
            abilityCollider.radius = AbilityLevel.FieldRadius * PlayerBehavior.Player.SizeMultiplier;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            EnemyBehavior enemy = collision.GetComponent<EnemyBehavior>();

            if (!enemies.ContainsKey(enemy))
            {
                enemies.Add(enemy, Time.time);

                enemy.TakeDamage(AbilityLevel.Damage * PlayerBehavior.Player.Damage);
            }
        }

        private void Update()
        {
            foreach (var enemy in enemies.Keys.ToList())
            {
                float time = enemies[enemy];

                if(Time.time - time > AbilityLevel.DamageCooldown * PlayerBehavior.Player.CooldownMultiplier)
                {
                    enemies[enemy] = Time.time;

                    enemy.TakeDamage(AbilityLevel.Damage * PlayerBehavior.Player.Damage);
                }
            }

            if(Time.time - lastTimeSound > 5f)
            {
                lastTimeSound = Time.time;

                GameController.AudioManager.PlaySound(GUARDIAN_EYE_HASH);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            EnemyBehavior enemy = collision.GetComponent<EnemyBehavior>();

            if (enemies.ContainsKey(enemy))
            {
                enemies.Remove(enemy);
            }
        }
    }
}