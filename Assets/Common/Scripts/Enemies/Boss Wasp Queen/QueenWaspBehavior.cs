using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class QueenWaspBehavior : EnemyBehavior
    {
        private static readonly int IS_CHARGING_HASH = Animator.StringToHash("IsCharging");
        private static readonly int IS_SHOOTING_HASH = Animator.StringToHash("IsShooting");

        private Coroutine behaviorCoroutine;

        [SerializeField] Animator animator;
        [SerializeField] Vector2 fenceOffset;

        [Header("Charge")]
        [SerializeField] SpriteRenderer chargeSprite;
        [SerializeField] float chargeSpriteDistance;
        [SerializeField] float chargeWarningDuration;
        [SerializeField] float chargeMovementDuration;
        [SerializeField] float chargeMovementSpeed;
        [SerializeField] int chargesCount;

        [Header("Shooting")]
        [SerializeField] ParticleSystem shootParticle;
        [SerializeField] int minesAmount;
        [SerializeField] int attacksCount;
        [SerializeField] float timeBetweenAttacks;
        [SerializeField] GameObject minePrefab;
        [SerializeField] Transform mineSpawnPosition;
        [SerializeField] float mineDamageMultiplier;

        [Header("Following")]
        [SerializeField] float followingPlayerDuration;

        private int placedMines = 0;

        private PoolComponent<HoneyMineBehavior> minesPool;
        private List<HoneyMineBehavior> activeMines = new List<HoneyMineBehavior>();

        protected override void Awake()
        {
            base.Awake();

            minesPool = new PoolComponent<HoneyMineBehavior>(minePrefab, 10);
        }

        public override void Play()
        {
            base.Play();

            behaviorCoroutine = StartCoroutine(BehaviorCoroutine());
        }

        private IEnumerator BehaviorCoroutine()
        {
            while (IsAlive)
            {
                yield return MoveToPlayer();
                for(int i = 0; i < chargesCount; i++)
                {
                    yield return Charge();
                }
                
                yield return MoveToPlayer();
                yield return PlaceMines();
            }
        }

        private IEnumerator MoveToPlayer()
        {
            IsMoving = true;

            yield return new WaitForSeconds(followingPlayerDuration);

            IsMoving = false;
        }

        private IEnumerator Charge()
        {
            float time = 0;

            chargeSprite.gameObject.SetActive(true);
            var movementDirection = Vector2.up;

            while (time < chargeWarningDuration)
            {
                time += Time.deltaTime;

                movementDirection = (PlayerBehavior.Player.transform.position - chargeSprite.transform.position).normalized;
                chargeSprite.transform.rotation = Quaternion.FromToRotation(Vector2.up, movementDirection);
                chargeSprite.size = chargeSprite.size.SetY(time / chargeWarningDuration * chargeSpriteDistance);

                if (!scaleCoroutine.ExistsAndActive())
                {
                    var scale = transform.localScale;

                    if (movementDirection.x > 0 && scale.x < 0 || movementDirection.x < 0 && scale.x > 0)
                    {
                        scale.x *= -1;
                        transform.localScale = scale;
                    }
                }
                yield return null;
            }

            chargeSprite.gameObject.SetActive(false);
            animator.SetBool(IS_CHARGING_HASH, true);

            time = 0;

            while (time < chargeMovementDuration)
            {
                var position = transform.position.XY() + Time.deltaTime * movementDirection * chargeMovementSpeed;
                if (StageController.FieldManager.ValidatePosition(position, fenceOffset))
                {
                    transform.position = position;
                }

                time += Time.deltaTime;

                yield return null;
            }

            animator.SetBool(IS_CHARGING_HASH, false);
        }

        private IEnumerator PlaceMines()
        {
            for (int i = 0; i < attacksCount; i++)
            {
                placedMines = 0;
                animator.SetBool(IS_SHOOTING_HASH, true);

                while (placedMines < minesAmount)
                {
                    yield return null;
                }

                animator.SetBool(IS_SHOOTING_HASH, false);

                yield return new WaitForSeconds(timeBetweenAttacks);
            }

            if (!scaleCoroutine.ExistsAndActive())
            {
                var scale = transform.localScale;
                var movementDirection = (PlayerBehavior.CenterPosition - transform.position.XY()).normalized;

                if (movementDirection.x > 0 && scale.x < 0 || movementDirection.x < 0 && scale.x > 0)
                {
                    scale.x *= -1;
                    transform.localScale = scale;
                }
            }
        }

        public void Shoot()
        {
            if(shootParticle != null) shootParticle.Play();

            placedMines++;

            var mine = minesPool.GetEntity();
            var direction = Vector3.right * transform.localScale.x;

            var landingPosition = PlayerBehavior.CenterPosition;

            mine.Damage = StageController.Stage.EnemyDamage * mineDamageMultiplier;
            mine.Spawn(mineSpawnPosition.position, direction, landingPosition);
            mine.onExploded += OnMineExploded;
            activeMines.Add(mine);
        }

        private void OnMineExploded(HoneyMineBehavior mine)
        {
            mine.onExploded -= OnMineExploded;
        }

        protected override void Die(bool flash)
        {
            StopCoroutine(behaviorCoroutine);

            for(int i = 0; i < activeMines.Count; i++)
            {
                activeMines[i].Clear();
                activeMines[i].onExploded -= OnMineExploded;
            }

            activeMines.Clear();

            chargeSprite.gameObject.SetActive(false);

            base.Die(flash);
        }
    }
}