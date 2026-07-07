using OctoberStudio.Easing;
using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class CrabBehavior : EnemyBehavior
    {
        private static readonly int IS_BURROWED_BOOL = Animator.StringToHash("Is Burrowed");
        private static readonly int IS_CHARGING_BOOL = Animator.StringToHash("Is Charging");

        [SerializeField] Animator animator;

        [Header("Particles")]
        [Tooltip("This particle plays when crab burrows into the ground")]
        [SerializeField] ParticleSystem burrowingParticle;
        [Tooltip("This particle plays during underground movement of the crab")]
        [SerializeField] ParticleSystem burrowingTrail;
        [Tooltip("This particle plays every tome the crab hits the ground with it's claw")]
        [SerializeField] ParticleSystem hitParticle;

        [Header("Burrowing")]
        [Tooltip("Amount of time the crab stays burrowed underground")]
        [SerializeField] float burrowedDuration = 4f;
        [Tooltip("How fast does the crab moves while burrowed")]
        [SerializeField] float burrowedMoveSpeed = 3f;

        [Header("Charging")]
        [Tooltip("The prefab of the earth spike")]
        [SerializeField] GameObject spikePrefab;
        [Tooltip("From this point the spikes will start spawning")]
        [SerializeField] Transform spikeStartPoint;
        [Tooltip("Amount of waves of the earth spikes attack")]
        [SerializeField] float chargingWavesCount = 3;
        [Tooltip("The speed of the spikes moving from the crab")]
        [SerializeField] float spikeSpeed;
        [Tooltip("Spike Spawns each 'value' seconds")]
        [SerializeField] float timeBetweenSpikes;
        [SerializeField] float spikeDamage;

        private PoolComponent<EarthSpikeBehavior> spikesPool;
        private List<EarthSpikeBehavior> spikes = new List<EarthSpikeBehavior>();

        private Coroutine behaviorCoroutine;

        private bool IsHittingClaw { get; set; }

        protected override void Awake()
        {
            base.Awake();

            spikesPool = new PoolComponent<EarthSpikeBehavior>(spikePrefab, 50);
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
                yield return Movement();

                yield return SpikesAttack();
                
                yield return Movement();

                yield return BurrowAttack();
            }
        }

        private IEnumerator Movement()
        {
            IsMoving = true;

            var randomPoint = StageController.FieldManager.Fence.GetRandomPointInside(1);

            while(Vector2.Distance(randomPoint, PlayerBehavior.Player.transform.position) > 2)
            {
                randomPoint = StageController.FieldManager.Fence.GetRandomPointInside(1);
            }

            IsMovingToCustomPoint = true;
            CustomPoint = randomPoint;

            yield return new WaitUntil(() => Vector2.Distance(transform.position, randomPoint) < 0.2f);

            IsMovingToCustomPoint = false;

            IsMoving = false;
        }

        private IEnumerator BurrowAttack()
        {
            animator.SetBool(IS_BURROWED_BOOL, true);

            IsMoving = false;

            burrowingParticle.Play();

            yield return new WaitForSeconds(0.7f);

            IsMoving = true;

            IsInvulnerable = true;

            burrowingTrail.Play();

            var cacheSpeed = speed;
            speed = burrowedMoveSpeed;

            yield return new WaitForSeconds(burrowedDuration);

            IsInvulnerable = false;

            IsMoving = false;
            animator.SetBool(IS_BURROWED_BOOL, false);
            speed = cacheSpeed;

            burrowingTrail.Stop();

            yield return new WaitForSeconds(0.7f);
        }

        private IEnumerator SpikesAttack()
        {
            IsMoving = false;
            animator.SetBool(IS_CHARGING_BOOL, true);

            IsHittingClaw = true;

            for (int i = 0; i < chargingWavesCount; i++)
            {
                var spikeStartPosition = spikeStartPoint.position;
                var playerPosition = PlayerBehavior.Player.transform.position;

                var distance = Vector2.Distance(spikeStartPosition, playerPosition);
                if (distance < 2f) distance = 2f;

                var direction = Vector3.up;
                var endPosition = spikeStartPosition + direction * (distance + 1f);

                var duration = Vector2.Distance(spikeStartPosition, endPosition) / spikeSpeed;
                var coroutine = spikeStartPoint.DoPosition(endPosition, duration);

                float lastTimeSpawned = Time.time;

                var singleSpikes = new List<EarthSpikeBehavior>();

                var startAngle = 360f / 5f / 3f * i;

                while (coroutine.IsActive)
                {
                    if (lastTimeSpawned + timeBetweenSpikes < Time.time)
                    {
                        lastTimeSpawned = Time.time;

                        Vector3 vector = spikeStartPoint.position - spikeStartPosition;

                        for (int j = 0; j < 5; j++)
                        {
                            var angle = startAngle + (360f / 5f) * j;

                            var position = spikeStartPosition + Quaternion.Euler(0, 0, angle) * vector;

                            if (StageController.FieldManager.ValidatePosition(position, Vector2.one * 0.2f))
                            {
                                var spike = spikesPool.GetEntity();
                                
                                spike.transform.position = position;
                                spike.Spawn(0);
                                spike.Damage = spikeDamage * StageController.Stage.EnemyDamage;
                                singleSpikes.Add(spike);
                                spikes.Add(spike);
                            }
                        }
                    }

                    yield return null;
                }

                spikeStartPoint.position = spikeStartPosition;

                coroutine = EasingManager.DoAfter(1f, () =>
                {
                    for (int i = 0; i < singleSpikes.Count; i++)
                    {
                        singleSpikes[i].Hide();
                        spikes.Remove(singleSpikes[i]);
                    }

                    singleSpikes.Clear();
                });

                yield return new WaitForSeconds(0.3f);
            }

            yield return new WaitForSeconds(0.7f);

            animator.SetBool(IS_CHARGING_BOOL, false);

            IsHittingClaw = false;

            IsMoving = true;
        }

        protected override void Die(bool flash)
        {
            if(behaviorCoroutine != null) StopCoroutine(behaviorCoroutine);

            for(int i = 0; i < spikes.Count; i++)
            {
                spikes[i].Clear();
            }

            spikes.Clear();

            base.Die(flash);
        }

        public void PlayClawHitParticle()
        {
            if (hitParticle != null && IsHittingClaw) hitParticle.Play();
        }
    }
}