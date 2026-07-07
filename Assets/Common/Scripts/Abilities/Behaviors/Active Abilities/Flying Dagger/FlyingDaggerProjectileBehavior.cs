using OctoberStudio.Easing;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Abilities
{
    public class FlyingDaggerProjectileBehavior : ProjectileBehavior
    {
        private static readonly int FLYING_DAGGER_LAUNCH_HASH = "Flying Dagger Launch".GetHashCode();

        [SerializeField] Rigidbody2D rigidBody;

        public UnityAction<FlyingDaggerProjectileBehavior> onFinished;

        private IEasingCoroutine movementCoroutine;

        public float ProjectileLifetime { get; set; }
        public float Size { get; set; }

        public void Spawn(Vector2 force)
        {
            Init();
            transform.localScale = Vector3.one * Size * PlayerBehavior.Player.SizeMultiplier;

            KickBack = true;

            rigidBody.linearVelocity = Vector2.zero;
            rigidBody.angularVelocity = 0;

            transform.rotation = Quaternion.identity;

            rigidBody.AddForce(force, ForceMode2D.Impulse);

            movementCoroutine = EasingManager.DoAfter(ProjectileLifetime, () => { 
                onFinished?.Invoke(this);
                Disable();
            });

            GameController.AudioManager.PlaySound(FLYING_DAGGER_LAUNCH_HASH);
        }

        public void Disable()
        {
            movementCoroutine.StopIfExists();
            gameObject.SetActive(false);
        }
    }
}