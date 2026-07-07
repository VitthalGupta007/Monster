using OctoberStudio.Easing;
using OctoberStudio.Extensions;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace OctoberStudio.Enemy
{
    public class EnemyMegaSlimeProjectileBehavior : SimpleEnemyProjectileBehavior
    {
        private static readonly int IS_FLYING_DOWN_BOOL = Animator.StringToHash("Is Flying Down");

        [SerializeField] Collider2D projectileCollider;
        [SerializeField] GameObject visuals;
        [SerializeField] Animator animator;
        [SerializeField] ParticleSystem hitParticle;

        private Coroutine attackCoroutine;
        public new UnityAction<EnemyMegaSlimeProjectileBehavior> onFinished;

        private WarningCircleBehavior warningCircle;

        private IEasingCoroutine easingCoroutine;

        public override void Init(Vector2 position, Vector2 direction)
        {
            base.Init(position, direction);

            projectileCollider.enabled = false;

            Vector3 upPosition = transform.position.SetY(CameraManager.TopBound + 2f);

            easingCoroutine = transform.DoPosition(upPosition, 0.3f).SetEasing(EasingType.SineIn).SetOnFinish(SwordHiddlen);
        }       

        private void SwordHiddlen()
        {
            visuals.gameObject.SetActive(false);

            attackCoroutine = StartCoroutine(AttackCoroutine());
        }

        private IEnumerator AttackCoroutine()
        {
            warningCircle = StageController.PoolsManager.GetEntity<WarningCircleBehavior>("Warning Circle");

            warningCircle.transform.position = PlayerBehavior.Player.transform.position;

            warningCircle.Play(1f, 0.3f, 100, null);
            warningCircle.Follow(PlayerBehavior.Player.transform, Vector3.zero, Time.deltaTime * 3);

            yield return new WaitForSeconds(2f);

            warningCircle.StopFollowing();

            Vector2 targetPosition = warningCircle.transform.position;

            transform.position = targetPosition.SetY(CameraManager.TopBound + 2f);

            visuals.gameObject.SetActive(true);

            animator.SetBool(IS_FLYING_DOWN_BOOL, true);

            easingCoroutine = transform.DoPosition(targetPosition, 0.5f).SetOnFinish(() =>
            {
                animator.SetBool(IS_FLYING_DOWN_BOOL, false);
                projectileCollider.enabled = true;

                hitParticle.Play();

                warningCircle.gameObject.SetActive(false);
                warningCircle = null;

                easingCoroutine = EasingManager.DoAfter(0.3f, () => projectileCollider.enabled = false);
            });

            yield return new WaitForSeconds(2);

            gameObject.SetActive(false);
            onFinished?.Invoke(this);

            attackCoroutine = null;
        }

        protected override void Update()
        {

        }

        public void Clear()
        {
            if (attackCoroutine != null) StopCoroutine(attackCoroutine);

            if(warningCircle != null)
            {
                warningCircle.gameObject.SetActive(false);
            }

            easingCoroutine.StopIfExists();

            gameObject.SetActive(false);
        }
    }
}