using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio
{
    public class CharacterBehavior : MonoBehaviour, ICharacterBehavior
    {
        protected static readonly int DEFEAT_TRIGGER = Animator.StringToHash("Defeat");
        protected static readonly int REVIVE_TRIGGER = Animator.StringToHash("Revive");
        protected static readonly int SPEED_FLOAT = Animator.StringToHash("Speed");

        protected static readonly int _Overlay = Shader.PropertyToID("_Overlay");

        [SerializeField] protected SpriteRenderer playerSpriteRenderer;
        [SerializeField] protected Animator animator;
        [SerializeField] protected Color hitColor;
        [SerializeField] protected Transform centerTransform;
        public Transform CenterTransform => centerTransform;

        public Transform Transform => transform;

        protected IEasingCoroutine damageCoroutine;

        public virtual void SetSpeed(float speed)
        {
            animator.SetFloat(SPEED_FLOAT, speed);
        }

        public virtual void SetLocalScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        public virtual void PlayReviveAnimation()
        {
            animator.SetTrigger(REVIVE_TRIGGER);
        }

        public virtual void PlayDefeatAnimation()
        {
            animator.SetTrigger(DEFEAT_TRIGGER);
        }

        public virtual void SetSortingOrder(int order) 
        {
            playerSpriteRenderer.sortingOrder = order;
        }

        public virtual void FlashHit(UnityAction onFinish = null)
        {
            if (damageCoroutine.ExistsAndActive()) return;

            var transparentColor = hitColor;
            transparentColor.a = 0;

            playerSpriteRenderer.material.SetColor(_Overlay, transparentColor);

            damageCoroutine = playerSpriteRenderer.material.DoColor(_Overlay, hitColor, 0.05f).SetOnFinish(() =>
            {
                damageCoroutine = playerSpriteRenderer.material.DoColor(_Overlay, transparentColor, 0.05f).SetOnFinish(onFinish);
            });
        }
    }
}