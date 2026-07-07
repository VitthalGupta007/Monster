using OctoberStudio.Pool;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OctoberStudio.UI
{
    public class WorldSpaceTextManager : MonoBehaviour
    {
        [SerializeField] protected RectTransform canvasRect;

        [SerializeField] protected GameObject textIndicatorPrefab;

        [SerializeField] protected AnimationCurve scaleCurve;
        [SerializeField] protected AnimationCurve positionCurve;
        [SerializeField] protected float maxScale;
        [SerializeField] protected float maxY;
        [SerializeField] protected float duration;

        protected PoolComponent<TextIndicatorBehavior> indicatorsPool;
        protected List<IndicatorData> indicators = new List<IndicatorData>();
        protected List<IndicatorData> waitingIndicators = new List<IndicatorData>();

        protected Camera mainCamera;
        protected float invertedDuration;

        protected NativeArray<float2> scalePositionCurve;
        protected NativeList<float> spawnTimes;
        protected NativeList<float2> worldPositions;

        protected NativeList<float4> scalePosition;
        protected NativeList<bool> isFinished;

        protected int capacityCache;
        protected bool isJobRunning;

        protected IndicatorJob indicatorJob;
        protected JobHandle indicatorJobHandle;

        private void Start()
        {
            indicatorsPool = new PoolComponent<TextIndicatorBehavior>(textIndicatorPrefab, 500, canvasRect);

            mainCamera = Camera.main;
            invertedDuration = 1f / duration;

            // We can't use Animation Curve inside a Job. Instead we approximate it by sampling it 50 times
            scalePositionCurve = CreateApproximateCurve(scaleCurve, positionCurve, 50, Allocator.Persistent);

            spawnTimes = new NativeList<float>(100, Allocator.Persistent);
            worldPositions = new NativeList<float2>(100, Allocator.Persistent);

            scalePosition = new NativeList<float4>(100, Allocator.Persistent);
            isFinished = new NativeList<bool>(100, Allocator.Persistent);

            capacityCache = isFinished.Capacity;

            indicatorJob = new IndicatorJob
            {
                scalePositionCurve = scalePositionCurve,

                spawnTimes = spawnTimes.AsDeferredJobArray(),
                worldPositions = worldPositions.AsDeferredJobArray(),

                scalePosition = scalePosition.AsDeferredJobArray(),
                isFinished = isFinished.AsDeferredJobArray(),

                invertedDuration = 1f / duration,
                maxY = maxY,
                maxScale = maxScale,
            };
        }

        protected virtual void OnDestroy()
        {
            scalePositionCurve.Dispose();
            spawnTimes.Dispose();
            worldPositions.Dispose();

            scalePosition.Dispose();
            isFinished.Dispose();
        }

        public void SpawnText(Vector2 worldPos, string text)
        {
            var viewportPos = mainCamera.WorldToViewportPoint(worldPos);

            var indicator = indicatorsPool.GetEntity();

            indicator.SetText(text);
            indicator.SetAnchors(viewportPos);
            indicator.SetPosition(Vector2.zero);

            var data = new IndicatorData { indicator = indicator, spawnTime = Time.time, worldPosition = worldPos };

            if (isJobRunning)
            {
                waitingIndicators.Add(data);
            } else
            {
                AddIndicatorData(data);
            }
        }

        protected virtual void AddIndicatorData(IndicatorData data)
        {
            indicators.Add(data);

            spawnTimes.Add(data.spawnTime);
            worldPositions.Add(data.worldPosition);
            scalePosition.Add(new float4());
            isFinished.Add(false);

            if(isFinished.Capacity != capacityCache)
            {
                // The previous arrays of Nativelists were disposed, we need to assign new ones
                indicatorJob.spawnTimes = spawnTimes.AsDeferredJobArray();
                indicatorJob.worldPositions = worldPositions.AsDeferredJobArray();

                indicatorJob.scalePosition = scalePosition.AsDeferredJobArray();
                indicatorJob.isFinished = isFinished.AsDeferredJobArray();

                capacityCache = isFinished.Capacity;
            }
        }

        protected virtual void Update()
        {
            if (indicators.Count == 0) return;

            indicatorJob.viewProjMatrix = mainCamera.projectionMatrix * mainCamera.worldToCameraMatrix;
            indicatorJob.time = Time.time;

            indicatorJobHandle = indicatorJob.Schedule(indicators.Count, 32);
            JobHandle.ScheduleBatchedJobs();

            isJobRunning = true;
        }

        protected virtual void LateUpdate()
        {
            if (isJobRunning)
            {
                isJobRunning = false;

                indicatorJobHandle.Complete();

                for (int i = 0; i < indicators.Count; i++)
                {
                    var indicator = indicators[i].indicator;

                    if (isFinished[i])
                    {
                        indicator.gameObject.SetActive(false);

                        indicators.RemoveAtSwapBack(i);
                        spawnTimes.RemoveAtSwapBack(i);
                        worldPositions.RemoveAtSwapBack(i);
                        scalePosition.RemoveAtSwapBack(i);
                        isFinished.RemoveAtSwapBack(i);

                        i--;
                        continue;
                    }

                    indicator.SetAnimationParameters(scalePosition[i]);
                }
            }

            if (waitingIndicators.Count > 0)
            {
                for (int i = 0; i < waitingIndicators.Count; i++)
                {
                    AddIndicatorData(waitingIndicators[i]);
                }

                waitingIndicators.Clear();
            }
        }

        public NativeArray<float2> CreateApproximateCurve(AnimationCurve curve1, AnimationCurve curve2, int resolution, Allocator allocator)
        {
            var samples = new NativeArray<float2>(resolution, allocator);

            for (int i = 0; i < resolution; i++)
            {
                var t = i / (float)(resolution - 1);
                samples[i] = new float2(curve1.Evaluate(t), curve2.Evaluate(t));
            }

            return samples;
        }

        protected class IndicatorData
        {
            public TextIndicatorBehavior indicator;
            public float spawnTime;
            public Vector2 worldPosition;
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public struct IndicatorJob : IJobParallelFor
        {
            // x = scale, y = position
            [NativeDisableParallelForRestriction]
            [ReadOnly] public NativeArray<float2> scalePositionCurve;
            [ReadOnly] public NativeArray<float> spawnTimes;
            [ReadOnly] public NativeArray<float2> worldPositions;

            // x = scale, y = position, zw = anchors
            [WriteOnly] public NativeArray<float4> scalePosition;
            [WriteOnly] public NativeArray<bool> isFinished;

            [ReadOnly] public float4x4 viewProjMatrix;

            [ReadOnly] public float invertedDuration;
            [ReadOnly] public float time;

            [ReadOnly] public float maxY;
            [ReadOnly] public float maxScale;

            public void Execute(int index)
            {
                var t = (time - spawnTimes[index]) * invertedDuration;
                isFinished[index] = t >= 1f;

                var curveData = EvaluateCurve(t);

                var result = new float4();
                result.x = curveData.x * maxScale;
                result.y = curveData.y * maxY;
                result.zw = GetViewport(index); 

                scalePosition[index] = result;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public float2 EvaluateCurve(float t)
            {
                var normalized = math.saturate(t);
                var fIndex = normalized * (scalePositionCurve.Length - 1);
                var i0 = (int)math.floor(fIndex);
                var i1 = math.min(i0 + 1, scalePositionCurve.Length - 1);
                var frac = fIndex - i0;

                return math.lerp(scalePositionCurve[i0], scalePositionCurve[i1], frac);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public float2 GetViewport(int index)
            {
                var worldPos = new float4(worldPositions[index], 0f, 1f);
                float4 clip = math.mul(viewProjMatrix, worldPos);
                float3 ndc = clip.xyz / clip.w;

                return new float2(ndc.x * 0.5f + 0.5f, ndc.y * 0.5f + 0.5f);
            }
        }
    }
}