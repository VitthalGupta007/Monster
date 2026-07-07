using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Abilities
{
    public class EclipseRayProjectileBehavior : ProjectileBehavior
    {
        public UnityAction<EclipseRayProjectileBehavior> onFinished;

        private Coroutine movementCoroutine;

        public float InitialRadius { get; set; }
        public float ProjectileLifetime { get; set; }
        public float AngularSpeed { get; set; }

        public void Spawn(float startingAngle)
        {
            Init();
            transform.localScale = Vector3.one * PlayerBehavior.Player.SizeMultiplier;

            KickBack = false;

            movementCoroutine = StartCoroutine(MovementCoroutine(startingAngle));
        }

        private IEnumerator MovementCoroutine(float startingAngle)
        {
            var time = 0f;
            var duration = ProjectileLifetime * PlayerBehavior.Player.DurationMultiplier;

            var startPosition = PlayerBehavior.Player.transform.position;

            var distance = InitialRadius * PlayerBehavior.Player.SizeMultiplier;
            var currentDistance = distance;
            do
            {
                time += Time.deltaTime;
                var t = time / duration;

                var angle = startingAngle + time * AngularSpeed * PlayerBehavior.Player.ProjectileSpeedMultiplier;

                currentDistance -= Time.deltaTime * (distance / duration);

                var position = startPosition + Quaternion.Euler(0, 0, angle) * Vector3.up * currentDistance;

                transform.position = position;

                yield return null;

            } while (time < duration);

            gameObject.SetActive(false);
            onFinished?.Invoke(this);

            movementCoroutine = null;
        }

        public void Disable()
        {
            if (movementCoroutine != null) StopCoroutine(movementCoroutine);
            gameObject.SetActive(false);
        }
    }
}