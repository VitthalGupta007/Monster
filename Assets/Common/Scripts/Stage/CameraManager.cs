using UnityEngine;

namespace OctoberStudio
{
    public class CameraManager : MonoBehaviour
    {
        private static CameraManager instance;

        [SerializeField] protected Transform target;
        [SerializeField] protected SpriteRenderer spotlightRenderer;
        [SerializeField] protected SpriteRenderer spotlightShadowRenderer;

        protected Vector3 offset;
        protected Camera mainCamera;

        public static float HalfHeight => instance.mainCamera.orthographicSize;
        public static float HalfWidth => instance.mainCamera.orthographicSize * instance.mainCamera.aspect;

        public static float BiggerSide { get; protected set; }

        public static Vector2 Position => instance.transform.position;

        public static float LeftBound => Position.x - HalfWidth;
        public static float RightBound => Position.x + HalfWidth;
        public static float TopBound => Position.y + HalfHeight;
        public static float BottomBound => Position.y - HalfHeight;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            instance = null;
            BiggerSide = 0;
        }

        protected virtual void Awake()
        {
            instance = this;

            mainCamera = GetComponent<Camera>();

            offset = transform.position - target.position;
            spotlightShadowRenderer.size = new Vector2(HalfWidth, HalfHeight) * 2;

            BiggerSide = Mathf.Max(HalfWidth, HalfHeight) * 2f;
        }

        public virtual void Init(StageData stageData)
        {
            spotlightRenderer.color = stageData.SpotlightColor;
            spotlightShadowRenderer.color = stageData.SpotlightShadowColor;
        }

        protected virtual void LateUpdate()
        {
            transform.position = target.position + offset;
        }

        public virtual void SetSize(float size)
        {
            mainCamera.orthographicSize = size;
            spotlightShadowRenderer.size = new Vector2(HalfWidth, HalfHeight) * 2;
            BiggerSide = Mathf.Max(HalfWidth, HalfHeight) * 2f;
        }

        public static bool IsPointOutsideCameraRight(Vector2 point)
        {
            return point.x > RightBound;
        }

        public static bool IsPointOutsideCameraRight(Vector2 point, out float distance)
        {
            bool result = point.x > RightBound;
            distance = result ? point.x - RightBound : 0;
            return result;
        }

        public static bool IsPointOutsideCameraLeft(Vector2 point, out float distance)
        {
            bool result = point.x < LeftBound;
            distance = result ? LeftBound - point.x : 0;
            return result;
        }

        public static bool IsPointOutsideCameraBottom(Vector2 point, out float distance)
        {
            bool result = point.y < BottomBound;
            distance = result ? BottomBound - point.y : 0;
            return result;
        }

        public static bool IsPointOutsideCameraTop(Vector2 point, out float distance)
        {
            bool result = point.y > TopBound;
            distance = result ? point.y - TopBound : 0;
            return result;
        }

        public static Vector2 GetPointInsideCamera(float padding = 0)
        {
            return new Vector2(Random.Range(LeftBound + padding, RightBound - padding), Random.Range(BottomBound + padding, TopBound - padding));
        }

        public static Vector2 GetRandomPointOutsideCamera(float padding = 0)
        {
            if(Random.value > instance.mainCamera.aspect / (instance.mainCamera.aspect + 1))
            {
                float x = Random.value > 0.5f ? LeftBound - padding : RightBound + padding;
                return new Vector2(x, Random.Range(BottomBound - padding, TopBound + padding));
            } else
            {
                float y = Random.value > 0.5f ? TopBound + padding : BottomBound - padding;
                return new Vector2(Random.Range(LeftBound - padding, RightBound + padding), y);
            }
        }
    }
}