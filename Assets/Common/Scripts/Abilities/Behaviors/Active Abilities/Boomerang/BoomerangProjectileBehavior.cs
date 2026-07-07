using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Abilities
{
    public class BoomerangProjectileBehavior : ProjectileBehavior
    {
        private static readonly int BOOMERANG_THROW_HASH = "Boomerang Throw".GetHashCode();

        public UnityAction<BoomerangProjectileBehavior> onBoomerangFinished;

        [SerializeField] AnimationCurve trajectiory;

        private IEasingCoroutine movementCoroutine;

        public float ProjectileTravelDistance { get; set; }
        public float ProjectileLifetime { get; set; }
        public float Size { get; set; }

        public void Spawn(Vector3 direction)
        {
            Init();

            transform.localScale = Vector3.one * Size * PlayerBehavior.Player.SizeMultiplier;

            var targetPosition = transform.position + direction * ProjectileTravelDistance * PlayerBehavior.Player.SizeMultiplier * PlayerBehavior.Player.DurationMultiplier;
            movementCoroutine = transform.DoPosition(targetPosition, ProjectileLifetime / PlayerBehavior.Player.ProjectileSpeedMultiplier).SetEasingCurve(trajectiory).SetOnFinish(() =>
            {
                gameObject.SetActive(false);
                onBoomerangFinished?.Invoke(this);
            });

            KickBack = false;

            GameController.AudioManager.PlaySound(BOOMERANG_THROW_HASH);
        }

        public void Disable()
        {
            movementCoroutine.StopIfExists();
            gameObject.SetActive(false);
        }
    }
}