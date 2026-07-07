using OctoberStudio.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Easing
{
    public class EasingManager : MonoBehaviour
    {
        private static EasingManager instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            instance = null;
        }

        protected EasingPositionJobRunner positionJobRunner;
        public static EasingPositionJobRunner PositionJobRunner => instance.positionJobRunner;

        public virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            positionJobRunner = new EasingPositionJobRunner();
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            positionJobRunner?.Clear();
            positionJobRunner = null;
        }

        protected virtual void Update()
        {
            positionJobRunner?.Update();
        }

        protected virtual void LateUpdate()
        {
            positionJobRunner?.LateUpdate();
        }

        public static IEasingCoroutine DoFloat(float from, float to, float duration, UnityAction<float> action, float delay = 0)
        {
            return new FloatEasingCoroutine(from, to, duration, delay, action);
        }

        public static IEasingCoroutine DoAfter(float seconds, UnityAction action, bool unscaledTime = false)
        {
            return new WaitCoroutine(seconds, unscaledTime).SetOnFinish(action);
        }

        public static IEasingCoroutine DoAfter(Func<bool> condition)
        {
            return new WaitForConditionCoroutine(condition);
        }

        public static IEasingCoroutine DoNextFrame()
        {
            return new NextFrameCoroutine();
        }
        public static IEasingCoroutine DoNextFrame(UnityAction action)
        {
            return new NextFrameCoroutine().SetOnFinish(action);
        }

        public static IEasingCoroutine DoNextFixedFrame()
        {
            return new NextFixedFrameCoroutine();
        }

        public static Coroutine StartCustomCoroutine(IEnumerator coroutine)
        {
            return instance.StartCoroutine(coroutine);
        }

        public static void StopCustomCoroutine(Coroutine coroutine)
        {
            if (instance != null) instance.StopCoroutine(coroutine);
        }
    }

    public interface IEasingCoroutine
    {
        bool IsActive { get; }
        IEasingCoroutine SetEasing(EasingType easingType);
        IEasingCoroutine SetEasingCurve(AnimationCurve easingCurve);
        IEasingCoroutine SetOnFinish(UnityAction callback);
        IEasingCoroutine SetUnscaledTime(bool unscaledTime);
        IEasingCoroutine SetDelay(float delay);
        void Stop();
    }

    public abstract class EmptyCoroutine : IEasingCoroutine
    {
        protected Coroutine coroutine;

        public bool IsActive { get; protected set; }

        protected UnityAction finishCallback;

        protected EasingType easingType = EasingType.Linear;

        protected float delay = -1;

        protected bool unscaledTime;

        protected bool useCurve;

        protected AnimationCurve easingCurve;

        public IEasingCoroutine SetEasing(EasingType easingType)
        {
            this.easingType = easingType;
            useCurve = false;
            return this;
        }

        public IEasingCoroutine SetOnFinish(UnityAction callback)
        {
            finishCallback = callback;
            return this;
        }

        public IEasingCoroutine SetUnscaledTime(bool unscaledTime)
        {
            this.unscaledTime = unscaledTime;
            return this;
        }

        public IEasingCoroutine SetEasingCurve(AnimationCurve curve)
        {
            easingCurve = curve;
            useCurve = true;

            return this;
        }

        public IEasingCoroutine SetDelay(float delay)
        {
            this.delay = delay;

            return this;
        }

        public void Stop()
        {
            EasingManager.StopCustomCoroutine(coroutine);

            IsActive = false;
        }
    }

    public class NextFrameCoroutine : EmptyCoroutine
    {
        public NextFrameCoroutine()
        {
            coroutine = EasingManager.StartCustomCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            yield return null;

            finishCallback?.Invoke();

            IsActive = false;
        }
    }

    public class NextFixedFrameCoroutine : EmptyCoroutine
    {
        public NextFixedFrameCoroutine()
        {
            coroutine = EasingManager.StartCustomCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            yield return new WaitForFixedUpdate();

            finishCallback?.Invoke();

            IsActive = false;
        }
    }

    public class WaitCoroutine : EmptyCoroutine
    {
        protected float duration;

        public WaitCoroutine(float duration, bool unscaledTime = false)
        {
            this.duration = duration;
            this.unscaledTime = unscaledTime;

            coroutine = EasingManager.StartCustomCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            while (delay > 0)
            {
                yield return null;

                delay -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            }

            if (unscaledTime)
            {
                yield return new WaitForSecondsRealtime(duration);
            }
            else
            {
                yield return new WaitForSeconds(duration);
            }

            finishCallback?.Invoke();

            IsActive = false;
        }
    }

    public class WaitForConditionCoroutine : EmptyCoroutine
    {
        private Func<bool> condition;

        public WaitForConditionCoroutine(Func<bool> condition)
        {
            this.condition = condition;
            coroutine = EasingManager.StartCustomCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            while (delay > 0)
            {
                yield return null;

                delay -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            }

            do
            {
                yield return null;
            } while (!condition());

            finishCallback?.Invoke();

            IsActive = false;
        }
    }

    public abstract class EasingCoroutine<T> : EmptyCoroutine
    {
        protected T from;
        protected T to;
        protected float duration;

        protected UnityAction<T> callback;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract T Lerp(T a, T b, float t);

        public EasingCoroutine(T from, T to, float duration, float delay, UnityAction<T> callback)
        {
            this.from = from;
            this.to = to;
            this.duration = duration;
            this.callback = callback;
            this.delay = delay;

            coroutine = EasingManager.StartCustomCoroutine(Coroutine());
        }

        private IEnumerator Coroutine()
        {
            IsActive = true;

            float time = 0;

            while (time < duration)
            {
                yield return null;

                if (delay > 0)
                {
                    delay -= unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

                    if (delay > 0) continue;
                }

                time += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float t;
                if (useCurve)
                {
                    t = easingCurve.Evaluate(time / duration);
                }
                else
                {
                    t = EasingFunctions.ApplyEasing(time / duration, easingType);
                }

                T value = Lerp(from, to, t);
                callback?.Invoke(value);
            }

            callback.Invoke(to);
            finishCallback?.Invoke();

            IsActive = false;
        }
    }

    public class FloatEasingCoroutine : EasingCoroutine<float>
    {
        public FloatEasingCoroutine(float from, float to, float duration, float delay, UnityAction<float> callback) : base(from, to, duration, delay, callback)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override float Lerp(float a, float b, float t)
        {
            return Mathf.LerpUnclamped(a, b, t);
        }
    }

    public class VectorEasingCoroutine3 : EasingCoroutine<Vector3>
    {
        public VectorEasingCoroutine3(Vector3 from, Vector3 to, float duration, float delay, UnityAction<Vector3> callback) : base(from, to, duration, delay, callback)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return Vector3.LerpUnclamped(a, b, t);
        }
    }

    public class VectorEasingCoroutine2 : EasingCoroutine<Vector2>
    {
        public VectorEasingCoroutine2(Vector2 from, Vector2 to, float duration, float delay, UnityAction<Vector2> callback) : base(from, to, duration, delay, callback)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return Vector2.LerpUnclamped(a, b, t);
        }
    }

    public class ColorEasingCoroutine : EasingCoroutine<Color>
    {
        public ColorEasingCoroutine(Color from, Color to, float duration, float delay, UnityAction<Color> callback) : base(from, to, duration, delay, callback)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override Color Lerp(Color a, Color b, float t)
        {
            return Color.LerpUnclamped(a, b, t);
        }
    }

    public abstract class EasingJobAnimation
    {
        protected float startTime;
        public float StartTime => startTime;

        protected float endTime;
        public float EndTime => endTime;

        protected bool useUnscaledTime;
        public bool UseUnscaledTime => useUnscaledTime;
        
        protected EasingType easingType;
        public EasingType EasingType => easingType;

        protected UnityAction finishCallback;

        protected EasingJobAnimation(float duration, float delay, bool useUnscaledTime, EasingType easingType)
        {
            this.useUnscaledTime = useUnscaledTime;

            var time = useUnscaledTime ? Time.unscaledTime : Time.time;
            startTime = time + delay;
            endTime = startTime + duration;

            this.easingType = easingType;
        }

        public bool IsActive => useUnscaledTime ? Time.unscaledTime < endTime : Time.time < endTime;
        public bool IsStarted => useUnscaledTime ? startTime <= Time.unscaledTime : startTime <= Time.time;

        public virtual void SetOnFinish(UnityAction finishCallback)
        {
            this.finishCallback = finishCallback;
        }

        public virtual void Finish()
        {
            finishCallback?.Invoke();
        }
    }

    public class PositionEasingJobAnimation : EasingJobAnimation
    {
        protected Transform transform;
        protected Transform targetTransform;

        public float2 Position { get => transform.position.XY(); set => transform.position = (Vector2)value; }
        public float2 Target => targetTransform.position.XY();

        public PositionEasingJobAnimation(Transform transform, Transform targetTransform, float duration, float delay, bool useUnscaledTime, EasingType easingType) : base(duration, delay, useUnscaledTime, easingType)
        {
            this.transform = transform;
            this.targetTransform = targetTransform;

            IsValid = true;

            EasingManager.PositionJobRunner.AddJobAnimaiton(this);
        }

        public bool IsValid { get; protected set; }
    }

    public class EasingPositionJobRunner
    {
        protected List<PositionEasingJobAnimation> waitingAnimations;
        protected List<PositionEasingJobAnimation> activeAnimations;

        [ReadOnly] public NativeList<float2> timeData;
        [ReadOnly] public NativeList<float> useUnscaledTime;

        [ReadOnly] public NativeList<FunctionPointer<EasingFunctions.EasingFunction>> easingFunctions;

        [ReadOnly] public NativeList<float2> startPositions;
        [ReadOnly] public NativeList<float2> targets;

        [WriteOnly] public NativeList<float2> positions;

        public bool isJobRunning = false;

        protected DoPosition2DJob doPosition2DJob;
        protected JobHandle doPosition2DJobHandle;
        protected int capacityCache;

        public EasingPositionJobRunner()
        {
            waitingAnimations = new List<PositionEasingJobAnimation>(10);
            activeAnimations = new List<PositionEasingJobAnimation>(50);

            timeData = new NativeList<float2>(50, Allocator.Persistent);
            useUnscaledTime = new NativeList<float>(50, Allocator.Persistent);

            easingFunctions = new NativeList<FunctionPointer<EasingFunctions.EasingFunction>>(50, Allocator.Persistent);

            startPositions = new NativeList<float2>(50, Allocator.Persistent);
            targets = new NativeList<float2>(50, Allocator.Persistent);

            positions = new NativeList<float2>(50, Allocator.Persistent);
            
            doPosition2DJob = new DoPosition2DJob();

            capacityCache = timeData.Capacity;

            ReinitializeJob();
        }

        public virtual void AddJobAnimaiton(PositionEasingJobAnimation jobAnimation)
        {
            if (jobAnimation.IsStarted && !isJobRunning)
            {
                AddActiveAnimation(jobAnimation);
            } else
            {
                waitingAnimations.Add(jobAnimation);
            }
        }

        public virtual void Update()
        {
            if (waitingAnimations.Count > 0)
            {
                for (int i = 0; i < waitingAnimations.Count; i++)
                {
                    if (waitingAnimations[i].IsStarted)
                    {
                        AddActiveAnimation(waitingAnimations[i]);
                        waitingAnimations.RemoveAt(i);

                        i--;
                    }
                }
            }

            if (activeAnimations.Count == 0) return;
            
            for (int i = 0; i < activeAnimations.Count; i++)
            {
                var animation = activeAnimations[i];

                if (animation.IsValid)
                {
                    targets[i] = activeAnimations[i].Target;
                }
                else
                {
                    RemoveActiveAnimation(i);
                    i--;
                }
            }

            doPosition2DJob.scaledTime = Time.time;
            doPosition2DJob.unscaledTime = Time.unscaledTime;

            doPosition2DJobHandle = doPosition2DJob.Schedule(activeAnimations.Count, 16);
            JobHandle.ScheduleBatchedJobs();

            isJobRunning = true;
        }

        protected virtual void ReinitializeJob()
        {
            doPosition2DJob.timeData = timeData.AsDeferredJobArray();
            doPosition2DJob.useUnscaledTime = useUnscaledTime.AsDeferredJobArray();

            doPosition2DJob.easingFunctions = easingFunctions.AsDeferredJobArray();

            doPosition2DJob.startPositions = startPositions.AsDeferredJobArray();
            doPosition2DJob.targets = targets.AsDeferredJobArray();

            doPosition2DJob.positions = positions.AsDeferredJobArray();
        }

        protected virtual void AddActiveAnimation(PositionEasingJobAnimation jobAnimation)
        {
            activeAnimations.Add(jobAnimation);

            timeData.Add(new float2(jobAnimation.StartTime, jobAnimation.EndTime));
            useUnscaledTime.Add(jobAnimation.UseUnscaledTime ? 1f : 0f);

            easingFunctions.Add(EasingFunctions.Functions[(int)jobAnimation.EasingType]);

            startPositions.Add(jobAnimation.Position);
            targets.Add(jobAnimation.Target);

            positions.Add(float2.zero);

            if(timeData.Capacity != capacityCache)
            {
                capacityCache = timeData.Capacity;
                ReinitializeJob();
            }
        }

        public virtual void LateUpdate()
        {
            if (!isJobRunning) return;
            isJobRunning = false;

            doPosition2DJobHandle.Complete();

            for (int i = 0; i < activeAnimations.Count; i++)
            {
                var animation = activeAnimations[i];

                var remove = false;
                if (animation.IsValid)
                {
                    activeAnimations[i].Position = positions[i];

                    if (!animation.IsActive)
                    {
                        animation.Finish();
                        remove = true;
                    }
                } else
                {
                    remove = true;
                }

                if (remove)
                {
                    RemoveActiveAnimation(i);
                    i--;
                }
            }
        }

        protected virtual void RemoveActiveAnimation(int index)
        {
            activeAnimations.RemoveAt(index);

            timeData.RemoveAt(index);
            useUnscaledTime.RemoveAt(index);
            easingFunctions.RemoveAt(index);
            startPositions.RemoveAt(index);
            targets.RemoveAt(index);
            positions.RemoveAt(index);
        }

        public virtual void Clear()
        {
            if(isJobRunning) doPosition2DJobHandle.Complete();

            timeData.Dispose();
            useUnscaledTime.Dispose();

            easingFunctions.Dispose();
            startPositions.Dispose();

            targets.Dispose();
            positions.Dispose();
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public struct DoPosition2DJob : IJobParallelFor
        {
            // x - startTime, y = endTime
            [ReadOnly] public NativeArray<float2> timeData;
            [ReadOnly] public NativeArray<float> useUnscaledTime;

            [ReadOnly] public NativeArray<FunctionPointer<EasingFunctions.EasingFunction>> easingFunctions;

            [ReadOnly] public NativeArray<float2> startPositions;
            [ReadOnly] public NativeArray<float2> targets;

            [WriteOnly] public NativeArray<float2> positions;

            [ReadOnly] public float scaledTime;
            [ReadOnly] public float unscaledTime;

            public void Execute(int i)
            {
                var time = math.select(unscaledTime, scaledTime, useUnscaledTime[i] == 0f);
                var t = math.unlerp(timeData[i].x, timeData[i].y, time);

                t = easingFunctions[i].Invoke(math.saturate(t));

                positions[i] = startPositions[i] + (targets[i] - startPositions[i]) * t;
            }
        }
    }
}