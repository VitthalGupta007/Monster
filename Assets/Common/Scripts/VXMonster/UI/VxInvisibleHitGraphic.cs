using UnityEngine;
using UnityEngine.UI;

namespace VXMonster.UI
{
    /// <summary>
    /// Raycast-only graphic (no mesh). Used to enlarge a Button hit box without drawing.
    /// Pattern: parent Button + this graphic; visible Image as child with raycastTarget=false.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class VxInvisibleHitGraphic : Graphic
    {
        public override void SetMaterialDirty() { }

        public override void SetVerticesDirty() { }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
