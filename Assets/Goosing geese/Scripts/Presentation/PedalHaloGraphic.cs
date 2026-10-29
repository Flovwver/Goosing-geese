using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class PedalHaloGraphic : MaskableGraphic
    {
        [SerializeField, Range(12, 96)]
        private int _segments = 48;
        [SerializeField, Range(2, 24)]
        private int _rings = 12;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect rect = GetPixelAdjustedRect();
            Vector2 center = rect.center;
            Vector2 radius = rect.size * 0.5f;
            int segments = Mathf.Clamp(_segments, 12, 96);
            int rings = Mathf.Clamp(_rings, 2, 24);

            for (int ring = 0; ring <= rings; ring++)
            {
                float distance = (float)ring / rings;
                Color tint = color;
                tint.a *= 1f - Mathf.SmoothStep(0f, 1f, distance);

                for (int segment = 0; segment <= segments; segment++)
                {
                    float angle = segment * Mathf.PI * 2f / segments;
                    Vector2 position = center + new Vector2(Mathf.Cos(angle) * radius.x, Mathf.Sin(angle) * radius.y) * distance;
                    mesh.AddVert(position, tint, Vector2.zero);

                    if (ring == 0 || segment == 0)
                        continue;

                    int current = ring * (segments + 1) + segment;
                    int previous = current - segments - 1;
                    mesh.AddTriangle(previous - 1, current - 1, current);
                    mesh.AddTriangle(previous - 1, current, previous);
                }
            }
        }
    }
}
