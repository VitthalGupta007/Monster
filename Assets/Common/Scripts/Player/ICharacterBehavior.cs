using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public interface ICharacterBehavior
    {
        Transform Transform { get; }
        Transform CenterTransform { get; }

        void SetSpeed(float speed);

        void SetLocalScale(Vector3 scale);
        void SetSortingOrder(int order);

        void PlayReviveAnimation();
        void PlayDefeatAnimation();

        void FlashHit(UnityAction onFinish = null);
    }
}