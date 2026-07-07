using System.Runtime.CompilerServices;
using UnityEngine;

namespace OctoberStudio.Extensions
{
    public static class VectorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 XZ(this Vector3 vector) => new Vector2(vector.x, vector.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 XY(this Vector3 vector) => new Vector2(vector.x, vector.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 YZ(this Vector3 vector) => new Vector2(vector.y, vector.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 XYZ(this Vector4 value) => new Vector3(value.x, value.y, value.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 XY(this Vector4 value) => new Vector2(value.x, value.y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 SetZ(this Vector4 vector, float value)
        {
            vector.z = value;

            return vector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetX(this Vector3 vector, float value)
        {
            vector.x = value;

            return vector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetY(this Vector3 vector, float value)
        {
            vector.y = value;

            return vector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 SetZ(this Vector3 vector, float value)
        {
            vector.z = value;

            return vector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SetX(this Vector2 vector, float value)
        {
            vector.x = value;

            return vector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SetY(this Vector2 vector, float value)
        {
            vector.y = value;

            return vector;
        }
    }
}