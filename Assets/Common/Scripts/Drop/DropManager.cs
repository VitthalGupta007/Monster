using OctoberStudio.Drop;
using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using OctoberStudio.Pool;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OctoberStudio
{
    public class DropManager : MonoBehaviour
    {
        [SerializeField] DropDatabase database;

        protected Dictionary<DropType, PoolComponent<DropBehavior>> dropPools = new Dictionary<DropType, PoolComponent<DropBehavior>>();
        protected Dictionary<DropType, float> lastTimeDropped = new Dictionary<DropType, float>();

        protected List<DropBehavior> dropList = new List<DropBehavior>();
        protected List<DropBehavior> waitingDrop = new List<DropBehavior>();

        protected NativeList<float2> dropPositions;
        protected NativeList<bool> isInside;

        protected JobHandle insideMagnetJobHandle;
        protected InsideMagnetJob insideMagnetJob;

        protected bool isJobRunning;
        protected int cachedCapacity;

        protected bool pickUpAllWhenFinished = false;

        public virtual void Init()
        {
            for (int i = 0; i < database.GemsCount; i++)
            {
                var data = database.GetGemData(i);

                var pool = new PoolComponent<DropBehavior>($"Drop_{data.DropType}", data.Prefab, 100);

                dropPools.Add(data.DropType, pool);
                lastTimeDropped.Add(data.DropType, 0);
            }

            dropPositions = new NativeList<float2>(500, Allocator.Persistent);
            isInside = new NativeList<bool>(500, Allocator.Persistent);

            cachedCapacity = dropPositions.Capacity;

            insideMagnetJob = new InsideMagnetJob();
            insideMagnetJob.positions = dropPositions.AsDeferredJobArray();
            insideMagnetJob.isInside = isInside.AsDeferredJobArray();
        }

        protected virtual void Update()
        {
            if (dropList.Count == 0) return;

            insideMagnetJob.playerPosition = PlayerBehavior.CenterPosition;
            insideMagnetJob.magnetDistanceSqr = PlayerBehavior.Player.MagnetRadiusSqr;

            insideMagnetJobHandle = insideMagnetJob.Schedule(dropList.Count, 64);
            JobHandle.ScheduleBatchedJobs();

            isJobRunning = true;
        }

        protected virtual void LateUpdate()
        {
            if (!isJobRunning)
            {
                pickUpAllWhenFinished = false;

                MoveWaitingDropToActive();

                return;
            }
            isJobRunning = false;

            insideMagnetJobHandle.Complete();

            if (pickUpAllWhenFinished)
            {
                PickUpAll();
            } else
            {
                var delay = 0f;
                for (int i = 0; i < dropList.Count; i++)
                {
                    if (isInside[i])
                    {
                        dropList[i].transform.DoPositionJob(PlayerBehavior.CenterTransform, 0.25f, delay, false, EasingType.BackIn).SetOnFinish(dropList[i].OnPickedUp);
                        delay += 0.002f;

                        dropList.RemoveAtSwapBack(i);
                        dropPositions.RemoveAtSwapBack(i);
                        isInside.RemoveAtSwapBack(i);

                        i--;
                    }
                }
            }

            MoveWaitingDropToActive();
        }

        protected virtual void MoveWaitingDropToActive()
        {
            if (waitingDrop != null && waitingDrop.Count > 0)
            {
                for (int i = 0; i < waitingDrop.Count; i++)
                {
                    AddDropToList(waitingDrop[i]);
                }

                waitingDrop.Clear();
            }
        }

        public virtual void PickUpAllDrop()
        {
            if (isJobRunning)
            {
                pickUpAllWhenFinished = true;
            } else
            {
                PickUpAll();
            }   
        }

        protected virtual void PickUpAll()
        {
            var delay = 0f;
            for (int i = 0; i < dropList.Count; i++)
            {
                if (dropList[i].DropData.AffectedByMagnet)
                {
                    dropList[i].transform.DoPositionJob(PlayerBehavior.CenterTransform, 0.25f, delay, false, EasingType.BackIn).SetOnFinish(dropList[i].OnPickedUp);
                    delay += 0.001f;

                    dropList.RemoveAtSwapBack(i);
                    dropPositions.RemoveAtSwapBack(i);
                    isInside.RemoveAtSwapBack(i);

                    i--;
                }
            }
        }

        public virtual bool CheckDropCooldown(DropType dropType)
        {
            return Time.time - lastTimeDropped[dropType] >= database.GetGemData(dropType).DropCooldown;
        }

        public virtual void Drop(DropType dropType, Vector3 position)
        {
            var drop = dropPools[dropType].GetEntity();

            drop.Init(database.GetGemData(dropType));
            drop.transform.position = position;

            lastTimeDropped[dropType] = Time.time;

            if (!isJobRunning)
            {
                AddDropToList(drop);
            }
            else
            {
                waitingDrop.Add(drop);
            }
        }

        protected virtual void AddDropToList(DropBehavior drop)
        {
            dropList.Add(drop);
            dropPositions.Add(drop.transform.position.XY());
            isInside.Add(false);

            if (cachedCapacity != isInside.Capacity)
            {
                insideMagnetJob.positions = dropPositions.AsDeferredJobArray();
                insideMagnetJob.isInside = isInside.AsDeferredJobArray();
            }
        }

        protected virtual void OnDestroy()
        {
            dropPositions.Dispose();
            isInside.Dispose();
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        protected struct InsideMagnetJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float2> positions;
            [ReadOnly] public float2 playerPosition;
            [ReadOnly] public float magnetDistanceSqr;

            [WriteOnly] public NativeArray<bool> isInside;

            public void Execute(int index)
            {
                isInside[index] = math.distancesq(positions[index], playerPosition) <= magnetDistanceSqr;
            }
        }
    }
}