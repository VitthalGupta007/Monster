using UnityEngine;

namespace OctoberStudio
{
    public class EnemyMaskProjectile : SimpleEnemyProjectileBehavior
    {
        [SerializeField] SpriteRenderer visuals;
        [SerializeField] SpriteRenderer shadow;

        public override void Init(Vector2 position, Vector2 direction)
        {
            base.Init(position, direction);

            var rotation = Quaternion.FromToRotation(Vector2.left, direction);

            visuals.transform.rotation = rotation;
            shadow.transform.rotation = rotation;
        }
    }
}