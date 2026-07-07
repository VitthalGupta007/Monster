using UnityEngine;

namespace OctoberStudio
{
    public class CircleFenceBehavior : BossFenceBehavior
    {
        [SerializeField] float radius = 5f;
        public float Radius { get; private set; }

        public override void Init()
        {
            Radius = radius;

            float perimeter = 2 * Mathf.PI * radius;
            FencePoolSize = Mathf.CeilToInt(perimeter / linkDistance);

            base.Init();
        }

        public override void SpawnFence(Vector2 center)
        {
            base.SpawnFence(center);

            float angleStep = 360f / FencePoolSize;

            for(int i = 0; i < FencePoolSize; i++)
            {
                var position = Center + (Vector2)(Quaternion.Euler(0, 0, angleStep * i) * Vector2.up * Radius);

                if (StageController.FieldManager.ValidatePosition(position, Vector2.zero, false))
                {
                    var link = fenceLinkPool.GetEntity();
                    link.transform.position = position;
                    link.transform.rotation = Quaternion.identity;

                    fenceLinks.Add(link);
                }
            }
        }

        public override Vector2 GetRandomPointInside(float offset)
        {
            return Center + Random.insideUnitCircle * (Radius - offset);
        }

        public override bool ValidatePosition(Vector2 position, Vector2 offset)
        {
            var biggerOffset = Mathf.Max(offset.x, offset.y);

            return Vector2.Distance(Center, position) < (Radius - biggerOffset);
        }

        public override Vector2 GetIntersectionPoint(Vector2 start, Vector2 end, float offset)
        {
            var path = end - start;
            var distance = path.magnitude;

            if(distance < Mathf.Epsilon) return start;

            var startToCenter = start - Center;

            var offsetRadius = Radius - offset;

            var a = Vector2.Dot(path, path);
            var b = 2 * Vector2.Dot(startToCenter, path);
            var c = Vector2.Dot(startToCenter, startToCenter) - offsetRadius * offsetRadius;

            var discriminant = b * b - 4 * a * c;

            if (discriminant < 0) return start;

            var sqrtDisc = Mathf.Sqrt(discriminant);

            // Two roots
            var t1 = (-b - sqrtDisc) / (2 * a);
            var t2 = (-b + sqrtDisc) / (2 * a);

            var t = Mathf.Max(t1, t2);
            if (t < 0) return start;

            return start + path * t;
        }

        public void SetRadiusOverride(float radiusOverride)
        {
            Radius = radiusOverride;

            float perimeter = 2 * Mathf.PI * Radius;
            FencePoolSize = Mathf.CeilToInt(perimeter / linkDistance);
        }

        public void ResetRadiusOverride()
        {
            Radius = radius;

            float perimeter = 2 * Mathf.PI * Radius;
            FencePoolSize = Mathf.CeilToInt(perimeter / linkDistance);
        }

        public override void RemoveFence()
        {
            base.RemoveFence();

            ResetRadiusOverride();
        }
    }
}