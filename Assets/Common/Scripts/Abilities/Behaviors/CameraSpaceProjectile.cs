using UnityEngine;

namespace OctoberStudio
{
    public abstract class CameraSpaceProjectile : ProjectileBehavior
    {
        private Vector3 direction;
        public Vector3 Direction { get => direction; set => direction = value; }

        public float Speed { get; set; }

        // Start is called before the first frame update
        void Start()
        {

        }

        public abstract Rect GetBounds();

        public virtual void Update()
        {
            transform.position += direction * Time.deltaTime * Speed * PlayerBehavior.Player.ProjectileSpeedMultiplier;

            Rect bounds = GetBounds();

            float distanceX;
            if (CameraManager.IsPointOutsideCameraRight(bounds.max, out distanceX) || StageController.FieldManager.IsPointOutsideFieldRight(bounds.max, out distanceX))
            {
                if (direction.x > 0) direction.x *= -1;
            }
            else if (CameraManager.IsPointOutsideCameraLeft(bounds.min, out distanceX) || StageController.FieldManager.IsPointOutsideFieldLeft(bounds.max, out distanceX))
            {
                if (direction.x < 0) direction.x *= -1;
            }

            if (distanceX > 0f) transform.position += direction * Mathf.Abs(distanceX / direction.x);

            float distanceY;
            if (CameraManager.IsPointOutsideCameraTop(bounds.max, out distanceY) || StageController.FieldManager.IsPointOutsideFieldTop(bounds.max, out distanceY))
            {
                if (direction.y > 0) direction.y *= -1;
            }
            else if (CameraManager.IsPointOutsideCameraBottom(bounds.min, out distanceY) || StageController.FieldManager.IsPointOutsideFieldBottom(bounds.max, out distanceY))
            {
                if (direction.y < 0) direction.y *= -1;
            }

            if (distanceY > 0) transform.position += direction * Mathf.Abs(distanceY / direction.y);
        }
    }
}