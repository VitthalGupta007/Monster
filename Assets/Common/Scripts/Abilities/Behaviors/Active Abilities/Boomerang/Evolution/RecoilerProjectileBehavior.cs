using OctoberStudio.Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Abilities
{
    public class RecoilerProjectileBehavior : ProjectileBehavior
    {
        public UnityAction<RecoilerProjectileBehavior> onRecoilerFinished;

        private Coroutine movementCoroutine;

        public float ProjectileTravelDistance { get; set; }
        public float ProjectileLifetime { get; set; }
        public float Size { get; set; }
        public float AngularSpeed { get; set; }

        public void Spawn(float startingAngle)
        {
            Init();
            transform.localScale = Vector3.one * Size * PlayerBehavior.Player.SizeMultiplier;

            KickBack = false;

            movementCoroutine = StartCoroutine(MovementCoroutine(startingAngle));
        }

        private IEnumerator MovementCoroutine(float startingAngle)
        {
            var time = 0f;
            var duration = ProjectileLifetime * PlayerBehavior.Player.DurationMultiplier;

            var startPosition = PlayerBehavior.CenterPosition;

            var distance = ProjectileTravelDistance * PlayerBehavior.Player.SizeMultiplier;
            var currentDistance = 0f;
            do
            {
                yield return null;

                time += Time.deltaTime;
                var t = time / duration;

                var angle = startingAngle + time * AngularSpeed * PlayerBehavior.Player.ProjectileSpeedMultiplier;

                float cos = Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad));
                float scale = Mathf.Lerp(0.5f, 1f, cos);

                currentDistance += Time.deltaTime * (distance / duration) * scale;

                var position = startPosition +( Quaternion.Euler(0, 0, angle) * Vector2.up * currentDistance).XY();

                transform.position = position;

            } while (time < duration);

            gameObject.SetActive(false);
            onRecoilerFinished?.Invoke(this);

            movementCoroutine = null;
        }

        public void Disable()
        {
            if (movementCoroutine != null) StopCoroutine(movementCoroutine);
            gameObject.SetActive(false);
        }
    }
}