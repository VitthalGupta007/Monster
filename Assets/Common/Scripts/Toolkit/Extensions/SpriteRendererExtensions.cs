using System.Runtime.CompilerServices;
using UnityEngine;

namespace VXMonster.Core.Extensions
{
    public static class SpriteRendererExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SpriteRenderer SetAlpha(this SpriteRenderer renderer, float alpha)
        {
            renderer.color = renderer.color.SetAlpha(alpha);
            return renderer;
        }
    }
}