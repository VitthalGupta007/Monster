using OctoberStudio.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class VoidBehavior : EnemyBehavior
    {
        private static readonly int TELEPORT_TRIGGER = Animator.StringToHash("Teleport");
        private static readonly int IS_CHARGING_BOOL = Animator.StringToHash("Is Charging");

        [SerializeField] GameObject blackHolePrefab;
        [SerializeField] Animator animator;
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] float blackHoleDamage = 5f;

        private PoolComponent<VoidBlackHoleBehavior> blackHolesPool;
        private List<VoidBlackHoleBehavior> blackHoles = new List<VoidBlackHoleBehavior>();

        private PoolComponent<SimpleEnemyProjectileBehavior> projectilesPool;

        private bool isTeleporting;
        private Vector2 teleportDestination;

        private bool IsHolesActive { get; set; }
        private bool IsCharging { get; set; }

        private Coroutine teleportCoroutine;
        private Coroutine behaviorCoroutine;

        private List<EnemyBehavior> shades = new List<EnemyBehavior>();

        protected override void Awake()
        {
            base.Awake();

            blackHolesPool = new PoolComponent<VoidBlackHoleBehavior>(blackHolePrefab, 5);
            projectilesPool = new PoolComponent<SimpleEnemyProjectileBehavior>(projectilePrefab, 5);
        }

        public override void Play()
        {
            base.Play();

            IsMoving = false;

            behaviorCoroutine = StartCoroutine(BehaviorCoroutine());
            teleportCoroutine = StartCoroutine(TeleportCoroutine());
        }

        private IEnumerator TeleportCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(5);

                for(int i = 0; i < 50; i++)
                {
                    var position = StageController.FieldManager.Fence.GetRandomPointInside(1);

                    bool collides = false;
                    foreach(var blackHole in blackHoles)
                    {
                        if (blackHole.Contains(position)){
                            collides = true;
                            break;
                        }
                    }

                    if (!collides)
                    {
                        isTeleporting = true;
                        teleportDestination = position;

                        animator.SetTrigger(TELEPORT_TRIGGER);

                        break;
                    }
                }

                yield return new WaitForSeconds(4);

                if (IsHolesActive)
                {
                    IsCharging = true;

                    animator.SetBool(IS_CHARGING_BOOL, true);

                    for(int i = 0; i < 3; i++)
                    {
                        foreach (var blackHole in blackHoles)
                        {
                            blackHole.Charge(projectilesPool);
                        }

                        yield return new WaitForSeconds(0.6f);
                    }
                    animator.SetBool(IS_CHARGING_BOOL, false);
                    IsCharging = false;
                }
            }
        }

        private IEnumerator BehaviorCoroutine()
        {
            while (true)
            {
                for (int i = 0; i < 5; i++)
                {
                    var blackHole = blackHolesPool.GetEntity();
                    blackHole.Damage = blackHoleDamage * StageController.Stage.EnemyDamage;
                    bool placed = true;

                    int counter = 0;
                    while (true)
                    {
                        var position = StageController.FieldManager.Fence.GetRandomPointInside(1);

                        blackHole.transform.position = position;

                        bool isPlaceAvailable = true;
                        for (int j = 0; j < blackHoles.Count; j++)
                        {
                            if (blackHoles[j].Intersects(blackHole))
                            {
                                isPlaceAvailable = false;
                                break;
                            }
                        }

                        if (isPlaceAvailable && !blackHole.Contains(transform.position) && !blackHole.Contains(PlayerBehavior.Player.transform.position))
                        {
                            blackHoles.Add(blackHole);
                            break;
                        }

                        counter++;
                        if (counter == 20)
                        {
                            placed = false;
                            break;
                        }
                    }

                    if (!placed) blackHole.gameObject.SetActive(false);
                    yield return new WaitForSeconds(0.1f);
                }

                IsHolesActive = true;

                for (int i = 0; i < 5; i++)
                {
                    yield return new WaitForSeconds(2f);

                    var blackHole = blackHoles[i % blackHoles.Count];

                    var shade = StageController.EnemiesSpawner.Spawn(EnemyType.Shade, blackHole.transform.position, OnShadeDied);
                    shades.Add(shade);
                }

                yield return new WaitWhile(() => IsCharging);

                IsHolesActive = false;

                for (int i = 0; i < blackHoles.Count; i++)
                {
                    blackHoles[i].Hide();
                }

                blackHoles.Clear();

                yield return new WaitForSeconds(1f);
            }
            
        }

        private void OnShadeDied(EnemyBehavior shade)
        {
            shade.onEnemyDied -= OnShadeDied;

            shades.Remove(shade);
        }

        public void Teleport()
        {
            if (isTeleporting)
            {
                isTeleporting = false;

                transform.position = teleportDestination;
            }
        }

        protected override void Die(bool flash)
        {
            StopCoroutine(teleportCoroutine);
            StopCoroutine(behaviorCoroutine);

            for(int i = 0; i < blackHoles.Count; i++)
            {
                blackHoles[i].Clear();
            }

            blackHoles.Clear();

            for(int i = 0; i < shades.Count; i++)
            {
                shades[i].onEnemyDied -= OnShadeDied;
                shades[i].Kill();
            }

            shades.Clear();

            base.Die(flash);
        }
    }
}