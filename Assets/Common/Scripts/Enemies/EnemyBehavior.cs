using VXMonster.Core.Easing;
using VXMonster.Core.Enemy;
using VXMonster.Core.Extensions;
using VXMonster.Core.Timeline;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using VXMonster.Gameplay;

namespace VXMonster.Core
{
    public class EnemyBehavior : MonoBehaviour
    {
        protected static readonly int _Overlay = Shader.PropertyToID("_Overlay");
        protected static readonly int _Disolve = Shader.PropertyToID("_Disolve");

        private static readonly int HIT_HASH = "Hit".GetHashCode();

        [Header("Settings")]
        [Tooltip("The speed of the enemy")]
        [SerializeField] protected float speed;
        public float Speed { get; protected set; }

        [Tooltip("The LevelData's 'Enemy Damage' is multiplied by this value to determine the damage of the enemy on each level")]
        [SerializeField] float damage = 1f;

        [Tooltip("The LevelData's 'Enemy HP' is multiplied by this value to determine the HP of the enemy on each level")]
        [SerializeField] float hp;

        [FormerlySerializedAs("canBekickedBack")]
        [SerializeField] bool canBeKickedBack = true;

        [SerializeField] bool shouldFadeIn;

        [Header("References")]
        [SerializeField] Rigidbody2D rb;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] DissolveSettings dissolveSettings;
        [SerializeField] SpriteRenderer shadowSprite;
        [SerializeField] Collider2D enemyCollider;

        public Vector2 Center => enemyCollider.bounds.center;

        [Header("Hit")]
        [SerializeField] float hitScaleAmount = 0.2f;
        [SerializeField] Color hitColor = Color.white;

        public EnemyData Data { get; private set; }
        public WaveOverride WaveOverride { get; protected set; }

        public bool IsVisible => spriteRenderer.isVisible;
        public bool IsAlive => HP > 0;
        // The enemy does not receive damage when this property is true
        public bool IsInvulnerable { get; protected set; }
        
        public float HP { get; private set; }
        public float MaxHP { get; private set; }

        public bool ShouldSpawnChestOnDeath { get; set; }

        public bool IsBoss { get; set; }

        IEasingCoroutine fallBackCoroutine;

        private Dictionary<EffectType, List<Effect>> appliedEffects = new Dictionary<EffectType, List<Effect>>();

        public bool IsRushing { get; protected set; }
        protected Vector2 RushDirection { get; set; }
        public float SpawnTime { get; protected set; }

        protected bool IsMoving { get; set; }
        public bool IsMovingToCustomPoint { get; protected set; }
        public Vector2 CustomPoint { get; protected set; }

        public float LastTimeDamagedPlayer { get; set; }

        private Material sharedMaterial;
        private Material effectsMaterial;

        private float shadowAlpha;

        public event UnityAction<EnemyBehavior> onEnemyDied;
        public event UnityAction<EnemyBehavior> onEnemyHidden;
        public event UnityAction<float, float> onHealthChanged;

        private float lastTimeSwitchedDirection = 0;

        IEasingCoroutine damageCoroutine;
        protected IEasingCoroutine scaleCoroutine;
        IEasingCoroutine fadeInCoroutine;

        private float damageTextValue;
        private float lastTimeDamageText;

        private static int lastFrameHitSound;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            lastFrameHitSound = 0;
        }

        private float lastTimeHitSound;

        protected virtual void Awake()
        {
            sharedMaterial = spriteRenderer.sharedMaterial;
            effectsMaterial = Instantiate(sharedMaterial);

            shadowAlpha = shadowSprite.color.a;
        }

        public void SetData(EnemyData data)
        {
            Data = data;
        }

        public void SetWaveOverride(WaveOverride waveOverride)
        {
            WaveOverride = waveOverride;
        }

        public virtual void SetRushDirection(Vector2 direction)
        {
            IsRushing = true;
            RushDirection = direction;
        }

        public virtual void Play()
        {
            MaxHP = StageController.ScaledEnemyHp * hp;
            Speed = speed;
            if (WaveOverride != null)
            {
                MaxHP = WaveOverride.ApplyHPOverride(MaxHP);
                Speed = WaveOverride.ApplySpeedOverride(Speed);
            }
            
            HP = MaxHP;
            IsMoving = true;

            shadowSprite.SetAlpha(shadowAlpha);

            enemyCollider.enabled = true;
            if (shouldFadeIn)
            {
                spriteRenderer.SetAlpha(0);
                fadeInCoroutine = spriteRenderer.DoAlpha(1, 0.2f);
            }

            SpawnTime = Time.time;
        }

        protected virtual void Update()
        {
            if(!IsAlive || !IsMoving || PlayerBehavior.Player == null) return;

            if(IsRushing && Time.time - SpawnTime > 2f && !spriteRenderer.isVisible)
            {
                Hide();
                return;
            }

            Vector3 direction;

            float speed = Speed;

            var elementStatus = GetComponent<EnemyElementStatus>();
            if (elementStatus != null)
            {
                speed *= elementStatus.SpeedMultiplier;
            }

            if (appliedEffects.TryGetValue(EffectType.Speed, out var speedEffects))
            {
                for (int i = 0; i < speedEffects.Count; i++)
                {
                    Effect effect = speedEffects[i];
                    speed *= effect.Modifier;
                }
            }

            if (IsRushing)
            {
                direction = RushDirection;
            } else
            {
                Vector3 target = IsMovingToCustomPoint ? CustomPoint : PlayerBehavior.Player.transform.position;

                direction = (target - transform.position).normalized;
            }

            transform.position += direction * Time.deltaTime * speed;            

            // Enemy looks in the direction it's moving
            if (!scaleCoroutine.ExistsAndActive())
            {
                var scale = transform.localScale;

                if (direction.x > 0 && scale.x < 0 || direction.x < 0 && scale.x > 0)
                {
                    // Preventing flickering when the enemy flips it's scale every frame
                    if(Time.unscaledTime - lastTimeSwitchedDirection > 0.1f)
                    {
                        scale.x *= -1;
                        transform.localScale = scale;

                        lastTimeSwitchedDirection = Time.unscaledTime;
                    }
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            ProjectileBehavior projectile = other.GetComponent<ProjectileBehavior>();

            if(projectile != null)
            {
                TakeDamage(PlayerBehavior.Player.Damage * projectile.DamageMultiplier);

                if(HP > 0)
                {
                    if (projectile.KickBack && canBeKickedBack)
                    {
                        KickBack(PlayerBehavior.CenterPosition);
                    }

                    if(projectile.Effects != null && projectile.Effects.Count > 0)
                    {
                        AddEffects(projectile.Effects);
                    }
                }
            }
        }

        public float GetDamage()
        {
            var damage = this.damage;
            if(WaveOverride != null) damage = WaveOverride.ApplyDamageOverride(damage);

            var baseDamage = StageController.ScaledEnemyDamage * damage;
            
            if (appliedEffects.ContainsKey(EffectType.Damage))
            {
                var damageEffects = appliedEffects[EffectType.Damage];

                for (int i = 0; i < damageEffects.Count; i++)
                {
                    var effect = damageEffects[i];

                    baseDamage *= effect.Modifier;
                }
            }

            return baseDamage;
        }

        public List<EnemyDropData> GetDropData()
        {
            if (WaveOverride != null) return WaveOverride.ApplyDropOverride(Data.EnemyDrop);
            return Data.EnemyDrop;
        }

        public void TakeDamage(float damage)
        {
            TakeDamage(damage, ElementType.None, 0f);
        }

        public void TakeDamage(float damage, ElementType appliedElement, float elementDuration)
        {
            if (!IsAlive) return;
            if (IsInvulnerable) return;

            var elementStatus = EnsureElementStatus();
            if (appliedElement != ElementType.None && elementDuration > 0f)
            {
                elementStatus.ApplyElement(appliedElement, elementDuration);
                ComboResolver.TryTriggerCombo(elementStatus, this, damage);
            }

            damage *= elementStatus.DamageTakenMultiplier;

            if (RelicsManager.Instance != null)
            {
                damage *= RelicsManager.Instance.GetDamageMultiplier();
                if (IsBoss)
                {
                    damage *= RelicsManager.Instance.GetBossDamageMultiplier();
                }
            }

            HP -= damage;

            onHealthChanged?.Invoke(HP, MaxHP);

            // Showing Damage Text
            damageTextValue += damage;
            if (Time.unscaledTime - lastTimeDamageText > 0.2f && damageTextValue >= 1)
            {
                var damageText = Mathf.RoundToInt(damageTextValue).ToString();
                StageController.WorldSpaceTextManager.SpawnText(transform.position + new Vector3(Random.Range(-0.1f, 0.1f), Random.value * 0.1f), damageText);

                damageTextValue = 0;
                lastTimeDamageText = Time.unscaledTime;
            } else
            {
                damageTextValue += damage;
            }

            // Playing Damage Sound
            if(Time.frameCount != lastFrameHitSound && Time.unscaledTime - lastTimeHitSound > 0.2f)
            {
                GameController.AudioManager.PlaySound(HIT_HASH);

                lastFrameHitSound = Time.frameCount;
                lastTimeHitSound = Time.unscaledTime;
            }   

            if (HP <= 0)
            {
                Die(true);
            }
            else
            {
                // Flashing Color on hit
                if (!damageCoroutine.ExistsAndActive())
                {
                    FlashHit(true);
                }

                // Scaling on Hit
                if (!scaleCoroutine.ExistsAndActive())
                {
                    var x = transform.localScale.x;

                    scaleCoroutine = transform.DoLocalScale(new Vector3(x * (1 - hitScaleAmount), (1 + hitScaleAmount), 1), 0.07f).SetEasing(EasingType.SineOut).SetOnFinish(() =>
                    {
                        scaleCoroutine = transform.DoLocalScale(new Vector3(x, 1, 1), 0.07f).SetEasing(EasingType.SineInOut);
                    });
                }
            }
        }

        private void FlashHit(bool resetMaterial, UnityAction onFinish = null)
        {
            spriteRenderer.material = effectsMaterial;

            var transparentColor = hitColor;
            transparentColor.a = 0;

            // Changin properties of a material creates a new instance of a material. 
            // We cache the material at the start of a game and assign it back when we're done with it
            // This way the batching optimization is restored
            effectsMaterial.SetColor(_Overlay, transparentColor);

            damageCoroutine = effectsMaterial.DoColor(_Overlay, hitColor, 0.05f).SetOnFinish(() =>
            {
                damageCoroutine = effectsMaterial.DoColor(_Overlay, transparentColor, 0.05f).SetOnFinish(() =>
                {
                    if(resetMaterial) spriteRenderer.material = sharedMaterial;
                    onFinish?.Invoke();
                });
            });
        }

        /// <summary>
        /// Used in rush, when enemy runs across the screen and goes behind the screen without being killed.
        /// </summary>
        public virtual void Hide()
        {
            enemyCollider.enabled = false;

            damageCoroutine.StopIfExists();
            fallBackCoroutine.StopIfExists();
            rb.simulated = true;

            fadeInCoroutine.StopIfExists();

            appliedEffects.Clear();

            WaveOverride = null;

            onEnemyHidden?.Invoke(this);

            gameObject.SetActive(false);
            spriteRenderer.material = sharedMaterial;
        }

        public virtual void Kill()
        {
            HP = 0;

            Die(false);
        }

        protected virtual void Die(bool flash)
        {
            enemyCollider.enabled = false;

            damageCoroutine.StopIfExists();

            onEnemyDied?.Invoke(this);
            fallBackCoroutine.StopIfExists();
            rb.simulated = true;

            fadeInCoroutine.StopIfExists();

            // Changin properties of a material creates a new instance of a material. 
            // We cache the material at the start of a game and assign it back when we're done with it
            // This way the batching optimization is restored

            spriteRenderer.material = effectsMaterial;

            if (flash)
            {
                FlashHit(false, () => {
                    effectsMaterial.SetColor(_Overlay, Color.clear);
                    effectsMaterial.DoColor(_Overlay, dissolveSettings.DissolveColor, dissolveSettings.Duration - 0.1f);
                });
            } else
            {
                effectsMaterial.SetColor(_Overlay, Color.clear);
                effectsMaterial.DoColor(_Overlay, dissolveSettings.DissolveColor, dissolveSettings.Duration);
            }

            effectsMaterial.SetFloat(_Disolve, 0);
            effectsMaterial.DoFloat(_Disolve, 1, dissolveSettings.Duration + 0.02f).SetEasingCurve(dissolveSettings.DissolveCurve).SetOnFinish(() =>
            {
                effectsMaterial.SetColor(_Overlay, Color.clear);
                effectsMaterial.SetFloat(_Disolve, 0);

                gameObject.SetActive(false);
                spriteRenderer.material = sharedMaterial;
            });

            shadowSprite.DoAlpha(0, dissolveSettings.Duration);

            appliedEffects.Clear();

            WaveOverride = null;
        }

        public void KickBack(Vector3 position)
        {
            var direction = (transform.position - position).normalized;
            rb.simulated = false;
            fallBackCoroutine.StopIfExists();
            fallBackCoroutine = transform.DoPosition(transform.position + direction * 0.6f, 0.15f).SetEasing(EasingType.ExpoOut).SetOnFinish(() => rb.simulated = true);
        }

        public void AddEffects(List<Effect> effects)
        {
            for(int i = 0; i < effects.Count; i++)
            {
                AddEffect(effects[i]);
            }
        }

        public void AddEffect(Effect effect)
        {
            if (!appliedEffects.ContainsKey(effect.EffectType))
            {
                appliedEffects.Add(effect.EffectType, new List<Effect>());
            }

            List<Effect> effects = appliedEffects[effect.EffectType];

            if (!effects.Contains(effect))
            {
                effects.Add(effect);
            }
        }

        public void RemoveEffect(Effect effect)
        {
            if (!appliedEffects.ContainsKey(effect.EffectType)) return;

            List<Effect> effects = appliedEffects[effect.EffectType];

            if (effects.Contains(effect))
            {
                effects.Remove(effect);
            }
        }

        protected EnemyElementStatus EnsureElementStatus()
        {
            var status = GetComponent<EnemyElementStatus>();
            if (status == null)
            {
                status = gameObject.AddComponent<EnemyElementStatus>();
            }

            return status;
        }

        protected virtual void OnDisable()
        {
            IsMovingToCustomPoint = false;
            IsRushing = false;
        }
    }
}