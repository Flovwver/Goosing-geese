using UnityEngine;
using UnityEngine.UI;

public enum MenuSymbol
{
    Gear,
    Trophy,
    Garage
}

[RequireComponent(typeof(CanvasRenderer))]
public class MenuSymbolGraphic : MaskableGraphic
{
    private const int RingSegments = 32;
    private const int GearTeeth = 8;
    [SerializeField] private MenuSymbol _symbol;

    protected override void OnPopulateMesh(VertexHelper vertices)
    {
        vertices.Clear();

        if (_symbol == MenuSymbol.Gear)
        {
            Ring(vertices, Vector2.zero, 0.25f, 0.055f);
            Ring(vertices, Vector2.zero, 0.09f, 0.04f);

            for (int i = 0; i < GearTeeth; i++)
            {
                float angle = i * Mathf.PI * 2f / GearTeeth;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Stroke(vertices, direction * 0.23f, direction * 0.36f, 0.09f);
            }
        }
        else if (_symbol == MenuSymbol.Trophy)
        {
            Quad(vertices, new Vector2(-0.27f, 0.3f), new Vector2(-0.15f, -0.02f), new Vector2(0.15f, -0.02f), new Vector2(0.27f, 0.3f));
            Stroke(vertices, new Vector2(0f, -0.02f), new Vector2(0f, -0.26f), 0.07f);
            Stroke(vertices, new Vector2(-0.22f, -0.29f), new Vector2(0.22f, -0.29f), 0.07f);
            Stroke(vertices, new Vector2(-0.27f, 0.24f), new Vector2(-0.39f, 0.24f), 0.05f);
            Stroke(vertices, new Vector2(-0.39f, 0.24f), new Vector2(-0.34f, 0.05f), 0.05f);
            Stroke(vertices, new Vector2(-0.34f, 0.05f), new Vector2(-0.17f, 0f), 0.05f);
            Stroke(vertices, new Vector2(0.27f, 0.24f), new Vector2(0.39f, 0.24f), 0.05f);
            Stroke(vertices, new Vector2(0.39f, 0.24f), new Vector2(0.34f, 0.05f), 0.05f);
            Stroke(vertices, new Vector2(0.34f, 0.05f), new Vector2(0.17f, 0f), 0.05f);
        }
        else
        {
            Stroke(vertices, new Vector2(-0.38f, 0.1f), new Vector2(0f, 0.38f), 0.06f);
            Stroke(vertices, new Vector2(0f, 0.38f), new Vector2(0.38f, 0.1f), 0.06f);
            Stroke(vertices, new Vector2(-0.3f, 0.13f), new Vector2(-0.3f, -0.32f), 0.05f);
            Stroke(vertices, new Vector2(0.3f, 0.13f), new Vector2(0.3f, -0.32f), 0.05f);
            Stroke(vertices, new Vector2(-0.32f, -0.32f), new Vector2(0.32f, -0.32f), 0.05f);

            for (int row = 0; row < 4; row++)
            {
                float height = 0.04f - row * 0.09f;
                Stroke(vertices, new Vector2(-0.2f, height), new Vector2(0.2f, height), 0.035f);
            }
        }
    }

    public void SetSymbol(MenuSymbol symbol)
    {
        _symbol = symbol;
        raycastTarget = false;
        SetVerticesDirty();
    }

    private void Ring(VertexHelper vertices, Vector2 center, float radius, float width)
    {
        for (int i = 0; i < RingSegments; i++)
        {
            float first = i * Mathf.PI * 2f / RingSegments;
            float second = (i + 1) * Mathf.PI * 2f / RingSegments;
            Stroke(vertices, center + new Vector2(Mathf.Cos(first), Mathf.Sin(first)) * radius,
                center + new Vector2(Mathf.Cos(second), Mathf.Sin(second)) * radius, width);
        }
    }

    private void Stroke(VertexHelper vertices, Vector2 start, Vector2 end, float width)
    {
        Vector2 offset = new Vector2(-(end - start).y, (end - start).x).normalized * width * 0.5f;
        Quad(vertices, start - offset, start + offset, end + offset, end - offset);
    }

    private void Quad(VertexHelper vertices, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        Rect rect = rectTransform.rect;
        float size = Mathf.Min(rect.width, rect.height);
        var quad = new UIVertex[4];
        Vector2[] points = { a, b, c, d };

        for (int i = 0; i < quad.Length; i++)
        {
            quad[i] = UIVertex.simpleVert;
            quad[i].position = rect.center + points[i] * size;
            quad[i].color = color;
        }

        vertices.AddUIVertexQuad(quad);
    }
}
