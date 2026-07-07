using OctoberStudio.Easing;
using UnityEngine;

namespace OctoberStudio.Enemy
{
    public class EnemyBellProjectile : SimpleEnemyProjectileBehavior
    {
        private IEasingCoroutine scaleCoroutine;
        private Transform hiddenParent;
        private Vector3 localPosition;

        public bool Successful { get; private set; }

        public void StickToTransform(Transform hiddenParent)
        {
            this.hiddenParent = hiddenParent;
            transform.position = hiddenParent.position;
            localPosition = Vector3.zero;

            transform.localScale = Vector3.zero;
            scaleCoroutine = transform.DoLocalScale(Vector3.one, 0.3f);

            IsActive = false;
            Successful = false;
        }

        protected override void Update()
        {
            base.Update();

            if (!IsActive)
            {
                transform.position = hiddenParent.position + localPosition;
            }
        }

        protected override void SuccessfulHit()
        {
            Successful = true;

            base.SuccessfulHit();
        }

        public override void Disable()
        {
            base.Disable();

            scaleCoroutine.StopIfExists();
        }
    }
}

