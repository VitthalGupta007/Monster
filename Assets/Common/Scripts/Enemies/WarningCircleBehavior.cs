using OctoberStudio.Easing;
using System;
using UnityEngine;

namespace OctoberStudio
{
    public class WarningCircleBehavior : MonoBehaviour
    {
        [SerializeField] SpriteRenderer sprite;

        private IEasingCoroutine coroutine;

        private bool isFollowing;
        private Transform followTarget;
        private Vector3 followOffset;
        private float followDrag;

        public void Play(float size, float spawnDuration, float stayDuration, Action onFinished)
        {
            coroutine.StopIfExists();

            sprite.size = Vector2.zero;
            coroutine = EasingManager.DoFloat(0, size, spawnDuration, SetSize).SetEasing(EasingType.SineOut).SetOnFinish(() => DelayBeforeDisable(stayDuration, onFinished));
        }

        public void Follow(Transform target, Vector3 offset, float drag)
        {
            isFollowing = true;
            followTarget = target;
            followOffset = offset;
            followDrag = drag;
        }

        public void StopFollowing()
        {
            isFollowing = false;
        }

        private void SetSize(float size)
        {
            sprite.size = new Vector2(size, size / 2f);
        }

        private void LateUpdate()
        {
            if (isFollowing)
            {
                transform.position = Vector3.Lerp(transform.position, followTarget.position + followOffset, followDrag);
            }
        }

        private void DelayBeforeDisable(float duration, Action onFinished)
        {
            coroutine = EasingManager.DoAfter(duration, () => {
                onFinished?.Invoke();
                gameObject.SetActive(false);
            });
        }

        private void OnDisable()
        {
            coroutine.StopIfExists();

            isFollowing = false;
        }
    }
}