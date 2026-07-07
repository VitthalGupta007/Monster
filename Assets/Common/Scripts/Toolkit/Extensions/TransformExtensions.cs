using UnityEngine;

namespace OctoberStudio.Extensions
{
    public static class TransformExtensions
    {
        public static Transform ResetLocal(this Transform transform)
        {
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            transform.localScale = Vector3.one;

            return transform;
        }

        public static Transform ResetGlobal(this Transform transform)
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            transform.localScale = Vector3.one;

            return transform;
        }
    }
}