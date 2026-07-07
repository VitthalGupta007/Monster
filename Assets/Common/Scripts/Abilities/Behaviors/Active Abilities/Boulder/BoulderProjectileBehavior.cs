using UnityEngine;

namespace OctoberStudio.Abilities
{
    public class BoulderProjectileBehavior : CameraSpaceProjectile
    {
        [SerializeField] bool kickBack;

        [SerializeField] Transform boulderTransform;
        public float Size { get; set; }
        public float AngularSpeed { get; set; }

        public override Rect GetBounds()
        {
            return new Rect(boulderTransform.position + (Vector3.down + Vector3.left) * Size / 2, Vector3.one * Size);
        }

        public void SetData(float size, float damageMultiplier, float speed, float angularSpeed)
        {
            transform.localScale = Vector3.one * size;

            Size = size;
            KickBack = kickBack;
            DamageMultiplier = damageMultiplier;

            AngularSpeed = angularSpeed;
            Speed = speed;
        }

        public override void Update()
        {
            base.Update();

            var xSign = -Direction.x / Mathf.Abs(Direction.x);

            transform.localScale = Vector3.one * Size * PlayerBehavior.Player.SizeMultiplier;

            boulderTransform.eulerAngles += Vector3.forward * xSign * Time.deltaTime * AngularSpeed * PlayerBehavior.Player.ProjectileSpeedMultiplier;
        }
    }
}