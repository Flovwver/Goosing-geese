using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    internal sealed class GarageColorSwatch : MaskableGraphic
    {
        private const int Segments = 32;
        private const float BorderWidth = 3f;
        private Color _paint;
        private bool _preview;
        private bool _selected;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;

            if (radius <= BorderWidth)
                return;

            AddDisc(mesh, rect.center, radius, _preview ? GameUiTheme.Accent : GameUiTheme.Ink);
            AddDisc(mesh, rect.center, radius - BorderWidth, _paint);

            if (_selected)
            {
                Color ink = _paint.grayscale > 0.5f ? GameUiTheme.Ink : Color.white;
                Vector2 joint = rect.center + new Vector2(-0.12f, -0.24f) * radius;
                AddStroke(mesh, rect.center + new Vector2(-0.43f, 0.08f) * radius, joint, ink);
                AddStroke(mesh, joint, rect.center + new Vector2(0.44f, 0.38f) * radius, ink);
            }
        }

        public void Show(Color paint, bool preview, bool selected)
        {
            _paint = paint;
            _preview = preview;
            _selected = selected;
            SetVerticesDirty();
        }

        private static void AddDisc(VertexHelper mesh, Vector2 center, float radius, Color fill)
        {
            int first = mesh.currentVertCount;
            mesh.AddVert(center, fill, Vector2.zero);

            for (int index = 0; index < Segments; index++)
            {
                float angle = index * Mathf.PI * 2f / Segments;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                mesh.AddVert(point, fill, Vector2.zero);
                mesh.AddTriangle(first, first + 1 + index, first + 1 + (index + 1) % Segments);
            }
        }

        private static void AddStroke(VertexHelper mesh, Vector2 start, Vector2 end, Color ink)
        {
            Vector2 perpendicular = Vector2.Perpendicular((end - start).normalized) * 2f;
            int first = mesh.currentVertCount;
            mesh.AddVert(start - perpendicular, ink, Vector2.zero);
            mesh.AddVert(start + perpendicular, ink, Vector2.zero);
            mesh.AddVert(end + perpendicular, ink, Vector2.zero);
            mesh.AddVert(end - perpendicular, ink, Vector2.zero);
            mesh.AddTriangle(first, first + 1, first + 2);
            mesh.AddTriangle(first, first + 2, first + 3);
        }
    }
}
