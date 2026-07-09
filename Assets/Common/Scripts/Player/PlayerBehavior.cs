using VXMonster.Core.Easing;
using VXMonster.Core.Extensions;
using VXMonster.Core.Upgrades;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using VXMonster.Gameplay;
using VXMonster.Save;

namespace VXMonster.Core
{
    public class PlayerBehavior : MonoBehaviour
    {
        protected static readonly int DEATH_HASH = "Death".GetHashCode();
        protected static readonly int REVIVE_HASH = "Revive".GetHashCode();
        protected static readonly int RECEIVING_DAMAGE_HASH = "Receiving Damage".GetHashCode();

        protected static PlayerBehavior instance;
        public static PlayerBehavior Player => instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            instance = null;
        }

        [SerializeField] protected CharactersDatabase charactersDatabase;

        [Header("Stats")]
        [SerializeField, Min(0.01f)] protected float speed = 2;
        [SerializeField, Min(0.1f)] protected float defaultMagnetRadius = 0.75f;
        [SerializeField, Min(1f)] protected float xpMultiplier = 1;
        [SerializeField, Range(0.1f, 1f)] protected float cooldownMultiplier = 1;
        [SerializeField, Range(0, 100)] protected int initialDamageReductionPercent = 0;
        [SerializeField, Min(1f)] protected float initialProjectileSpeedMultiplier = 1;
        [SerializeField, Min(1f)] protected float initialSizeMultiplier = 1f;
        [SerializeField, Min(1f)] protected float initialDurationMultiplier = 1f;
        [SerializeField, Min(1f)] protected float initialGoldMultiplier = 1;

        [Header("References")]
        [SerializeField] protected HealthbarBehavior healthbar;
        [SerializeField] protected Transform centerPoint;
        [SerializeField] protected PlayerEnemyCollisionHelper collisionHelper;

        public static Transform CenterTransform => instance.centerPoint;
        public static Vector2 CenterPosition {
            get
            {
                if(instance.Character != null && instance.Character.CenterTransform != null)
                {
                    return instance.Character.CenterTransform.position;
                }
                return instance.centerPoint.position;
            }
        }

        [Header("Death and Revive")]
        [SerializeField] protected ParticleSystem reviveParticle;

        [Space]
        [SerializeField] protected SpriteRenderer reviveBackgroundSpriteRenderer;
        [SerializeField, Range(0, 1)] protected float reviveBackgroundAlpha;
        [SerializeField, Range(0, 1)] protected float reviveBackgroundSpawnDelay;
        [SerializeField, Range(0, 1)] protected float reviveBackgroundHideDelay;

        [Space]
        [SerializeField] protected SpriteRenderer reviveBottomSpriteRenderer;
        [SerializeField, Range(0, 1)] protected float reviveBottomAlpha;
        [SerializeField, Range(0, 1)] protected float reviveBottomSpawnDelay;
        [SerializeField, Range(0, 1)] protected float reviveBottomHideDelay;

        [Header("Other")]
        [SerializeField] protected Vector2 fenceOffset;
        [SerializeField] protected Color hitColor;
        [SerializeField, Tooltip("Vampire Survivors-style global invuln after taking damage (seconds).")]
        protected float postHitInvulnDuration = 0.24f;

        public event UnityAction onPlayerDied;

        public float Damage { get; protected set; }
        public float MagnetRadiusSqr { get; protected set; }
        public float Speed { get; protected set; }

        public float XPMultiplier { get; protected set; }
        public float CooldownMultiplier { get; protected set; }
        public float DamageReductionMultiplier { get; protected set; }
        public float ProjectileSpeedMultiplier { get; protected set; }
        public float SizeMultiplier { get; protected set; }
        public float DurationMultiplier { get; protected set; }
        public float GoldMultiplier { get; protected set; }

        private float lastMagnetAbilityMultiplier = 1f;
        private float lastMaxHpAbilityMultiplier = 1f;
        private float lastGoldAbilityMultiplier = 1f;
        private float lastCooldownAbilityMultiplier = 1f;

        public Vector2 LookDirection { get; protected set; }
        public bool IsMovingAlowed { get; set; }

        protected bool invincible = false;
        protected float postHitInvulnUntil;

        protected List<EnemyBehavior> enemiesInside = new List<EnemyBehavior>();

        protected CharactersSave charactersSave;
        public CharacterData Data { get; set; }
        protected ICharacterBehavior Character { get; set; }

        protected virtual void Awake()
        {
            charactersSave = GameController.SaveManager.GetSave<CharactersSave>("Characters");
            Data = charactersDatabase.GetCharacterData(charactersSave.SelectedCharacterId);

            Character = Instantiate(Data.Prefab).GetComponent<ICharacterBehavior>();
            Character.Transform.SetParent(transform);
            Character.Transform.ResetLocal();

            instance = this;
            healthbar.Init(Data.BaseHP);
            healthbar.SetAutoHideWhenMax(true);
            healthbar.SetAutoShowOnChanged(true);

            RecalculateMagnetRadius(1);
            RecalculateMoveSpeed(1);
            RecalculateDamage(1);
            RecalculateMaxHP(1);
            RecalculateXPMuliplier(1);
            RecalculateCooldownMuliplier(1);
            RecalculateDamageReduction(0);
            RecalculateProjectileSpeedMultiplier(1f);
            RecalculateSizeMultiplier(1f);
            RecalculateDurationMultiplier(1);
            RecalculateGoldMultiplier(1);

            LookDirection = Vector2.right;

            IsMovingAlowed = true;
        }

        protected virtual void Update()
        {
            if (healthbar.IsZero) return;

            if (!IsPostHitInvulnerable() && enemiesInside.Count > 0)
            {
                var strongest = 0f;
                for (var i = 0; i < enemiesInside.Count; i++)
                {
                    var enemy = enemiesInside[i];
                    if (enemy == null) continue;
                    var dmg = enemy.GetDamage();
                    if (dmg > strongest) strongest = dmg;
                }

                if (strongest > 0f)
                {
                    TakeDamage(strongest);
                }
            }

            if (!IsMovingAlowed) return;

            var input = GameController.InputManager.MovementValue;

            float joysticPower = input.magnitude;
            Character.SetSpeed(joysticPower);

            if (!Mathf.Approximately(joysticPower, 0) && Time.timeScale > 0)
            {
                var frameMovement = input * Time.deltaTime * Speed;

                if (StageController.FieldManager.ValidatePosition(transform.position + Vector3.right * frameMovement.x, fenceOffset))
                {
                    transform.position += Vector3.right * frameMovement.x;
                }

                if (StageController.FieldManager.ValidatePosition(transform.position + Vector3.up * frameMovement.y, fenceOffset))
                {
                    transform.position += Vector3.up * frameMovement.y;
                }

                collisionHelper.transform.localPosition = Vector3.zero;

                Character.SetLocalScale(new Vector3(input.x > 0 ? 1 : -1, 1, 1));

                LookDirection = input.normalized;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsInsideMagnetRadius(Transform target)
        {
            return (transform.position - target.position).sqrMagnitude <= MagnetRadiusSqr;
        }

        public virtual void RecalculateMagnetRadius(float magnetRadiusMultiplier)
        {
            lastMagnetAbilityMultiplier = magnetRadiusMultiplier;
            var mult = magnetRadiusMultiplier;
            if (RelicsManager.Instance != null)
            {
                mult *= RelicsManager.Instance.GetPickupRadiusMultiplier();
            }

            MagnetRadiusSqr = Mathf.Pow(defaultMagnetRadius * mult, 2);
        }

        public virtual void RecalculateMoveSpeed(float moveSpeedMultiplier)
        {
            var talentMult = GameSessionManager.Instance?.GetTalentMoveSpeedMultiplier() ?? 1f;
            Speed = speed * moveSpeedMultiplier * talentMult;
        }

        public virtual void RecalculateDamage(float damageMultiplier)
        {
            Damage = Data.BaseDamage * damageMultiplier;
            if (GameController.UpgradesManager.IsUpgradeAquired(UpgradeType.Damage))
            {
                Damage *= GameController.UpgradesManager.GetUpgadeValue(UpgradeType.Damage);
            }
        }

        public virtual void RecalculateMaxHP(float maxHPMultiplier)
        {
            lastMaxHpAbilityMultiplier = maxHPMultiplier;
            var upgradeValue = GameController.UpgradesManager.GetUpgadeValue(UpgradeType.Health);
            var mult = maxHPMultiplier;
            if (RelicsManager.Instance != null)
            {
                mult *= RelicsManager.Instance.GetMaxHpMultiplier();
            }

            healthbar.ChangeMaxHP((Data.BaseHP + upgradeValue + (GameSessionManager.Instance?.GetTalentMaxHpBonus() ?? 0f)) * mult);
        }

        public virtual void RecalculateXPMuliplier(float xpMultiplier)
        {
            XPMultiplier = this.xpMultiplier * xpMultiplier;
        }

        public virtual void RecalculateCooldownMuliplier(float cooldownMultiplier)
        {
            lastCooldownAbilityMultiplier = cooldownMultiplier;
            var mult = this.cooldownMultiplier * cooldownMultiplier;
            if (RelicsManager.Instance != null)
            {
                mult *= RelicsManager.Instance.GetCooldownMultiplier();
            }

            CooldownMultiplier = mult;
        }

        public virtual void RecalculateDamageReduction(float damageReductionPercent)
        {
            DamageReductionMultiplier = (100f - initialDamageReductionPercent - damageReductionPercent) / 100f;

            if (GameController.UpgradesManager.IsUpgradeAquired(UpgradeType.Armor))
            {
                DamageReductionMultiplier *= GameController.UpgradesManager.GetUpgadeValue(UpgradeType.Armor);
            }

            DamageReductionMultiplier = Mathf.Clamp(DamageReductionMultiplier, 0.1f, 1f);
        }

        public virtual void RecalculateProjectileSpeedMultiplier(float projectileSpeedMultiplier)
        {
            ProjectileSpeedMultiplier = initialProjectileSpeedMultiplier * projectileSpeedMultiplier;
        }

        public virtual void RecalculateSizeMultiplier(float sizeMultiplier)
        {
            SizeMultiplier = initialSizeMultiplier * sizeMultiplier;
        }

        public virtual void RecalculateDurationMultiplier(float durationMultiplier)
        {
            DurationMultiplier = initialDurationMultiplier * durationMultiplier;
        }

        public virtual void RecalculateGoldMultiplier(float goldMultiplier)
        {
            lastGoldAbilityMultiplier = goldMultiplier;
            var mult = initialGoldMultiplier * goldMultiplier;
            if (RelicsManager.Instance != null)
            {
                mult *= RelicsManager.Instance.GetGoldMultiplier();
            }

            mult *= GameSessionManager.Instance?.GetTalentGoldMultiplier() ?? 1f;
            GoldMultiplier = mult;
        }

        public virtual void RefreshRelicModifiers()
        {
            RecalculateMagnetRadius(lastMagnetAbilityMultiplier);
            RecalculateMaxHP(lastMaxHpAbilityMultiplier);
            RecalculateGoldMultiplier(lastGoldAbilityMultiplier);
            RecalculateCooldownMuliplier(lastCooldownAbilityMultiplier);
        }

        public virtual void RestoreHP(float hpPercent)
        {
            healthbar.AddPercentage(hpPercent);
        }

        public virtual void Heal(float hp)
        {
            healthbar.AddHP(hp + GameController.UpgradesManager.GetUpgadeValue(UpgradeType.Healing));
        }

        public virtual void Revive()
        {
            Character.PlayReviveAnimation();
            reviveParticle.Play();

            invincible = true;
            IsMovingAlowed = false;
            healthbar.ResetHP(1f);

            Character.SetSortingOrder(102);

            reviveBackgroundSpriteRenderer.DoAlpha(0f, 0.3f, reviveBottomHideDelay).SetUnscaledTime(true).SetOnFinish(() => reviveBackgroundSpriteRenderer.gameObject.SetActive(false));
            reviveBottomSpriteRenderer.DoAlpha(0f, 0.3f, reviveBottomHideDelay).SetUnscaledTime(true).SetOnFinish(() => reviveBottomSpriteRenderer.gameObject.SetActive(false));

            GameController.AudioManager.PlaySound(REVIVE_HASH);
            EasingManager.DoAfter(1f, () =>
            {
                IsMovingAlowed = true;
                Character.SetSortingOrder(0);
            });

            EasingManager.DoAfter(3, () => invincible = false);
        }

        public virtual void CheckTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.layer == 7)
            {
                if (invincible) return;

                var enemy = collision.GetComponent<EnemyBehavior>();

                if (enemy != null)
                {
                    enemiesInside.Add(enemy);
                    enemy.LastTimeDamagedPlayer = Time.time;

                    enemy.onEnemyDied += OnEnemyDied;
                    TakeDamage(enemy.GetDamage());
                }
            }
            else
            {
                if (invincible) return;

                var projectile = collision.GetComponent<SimpleEnemyProjectileBehavior>();
                if (projectile != null)
                {
                    TakeDamage(projectile.Damage);
                }
            }
        }

        public virtual void CheckTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.layer == 7)
            {
                if (invincible) return;

                var enemy = collision.GetComponent<EnemyBehavior>();

                if (enemy != null)
                {
                    enemiesInside.Remove(enemy);
                    enemy.onEnemyDied -= OnEnemyDied;
                }
            }
        }

        protected virtual void OnEnemyDied(EnemyBehavior enemy)
        {
            enemy.onEnemyDied -= OnEnemyDied;
            enemiesInside.Remove(enemy);
        }

        protected float lastTimeVibrated = 0f;

        public virtual void TakeDamage(float damage)
        {
            if (invincible || IsPostHitInvulnerable() || healthbar.IsZero) return;

            healthbar.Subtract(damage * DamageReductionMultiplier);
            BeginPostHitInvuln();

            Character.FlashHit();

            if (healthbar.IsZero)
            {
                if (TryPhoenixRevive())
                {
                    return;
                }

                Character.PlayDefeatAnimation();
                Character.SetSortingOrder(102);

                reviveBackgroundSpriteRenderer.gameObject.SetActive(true);
                reviveBackgroundSpriteRenderer.DoAlpha(reviveBackgroundAlpha, 0.3f, reviveBackgroundSpawnDelay).SetUnscaledTime(true);
                reviveBackgroundSpriteRenderer.transform.position = transform.position.SetZ(reviveBackgroundSpriteRenderer.transform.position.z);

                reviveBottomSpriteRenderer.gameObject.SetActive(true);
                reviveBottomSpriteRenderer.DoAlpha(reviveBottomAlpha, 0.3f, reviveBottomSpawnDelay).SetUnscaledTime(true);

                GameController.AudioManager.PlaySound(DEATH_HASH);

                EasingManager.DoAfter(0.5f, () =>
                {
                    onPlayerDied?.Invoke();
                }).SetUnscaledTime(true);

                GameController.VibrationManager.StrongVibration();
            } else
            {
                if(Time.time - lastTimeVibrated > 0.05f)
                {
                    GameController.VibrationManager.LightVibration();
                    lastTimeVibrated = Time.time;
                }
                
                GameController.AudioManager.PlaySound(RECEIVING_DAMAGE_HASH);
            }
        }

        protected virtual bool IsPostHitInvulnerable()
        {
            return Time.time < postHitInvulnUntil;
        }

        protected virtual void BeginPostHitInvuln()
        {
            var duration = Mathf.Max(0.01f, postHitInvulnDuration);
            postHitInvulnUntil = Time.time + duration;
        }

        protected virtual bool TryPhoenixRevive()
        {
            var session = GameSessionManager.Instance?.RunSession;
            if (session == null || session.PhoenixReviveUsed) return false;
            if (RelicsManager.Instance == null || !RelicsManager.Instance.HasPhoenixRevive()) return false;

            session.PhoenixReviveUsed = true;
            healthbar.ResetHP(0.35f);
            Character.SetSortingOrder(0);
            invincible = true;
            EasingManager.DoAfter(2f, () => invincible = false).SetUnscaledTime(true);
            GameController.AudioManager.PlaySound(REVIVE_HASH);
            return true;
        }
    }
}