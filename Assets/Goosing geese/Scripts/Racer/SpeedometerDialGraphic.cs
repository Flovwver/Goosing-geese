using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class SpeedometerDialGraphic : MaskableGraphic
{
    private const float SweepRadians = Mathf.PI;
    private const int HubSegments = 24;

    private readonly float[] _scaleMarks = new float[4];

    [SerializeField, Range(24, 144)] private int _segments = 72;
    [SerializeField, Range(0.1f, 0.4f)] private float _bandThickness = 0.22f;
    [SerializeField, Range(0.7f, 1f)] private float _radiusScale = 0.96f;
    [SerializeField, Range(0.5f, 0.9f)] private float _needleLength = 0.74f;
    [SerializeField, Range(0.01f, 0.05f)] private float _needleWidth = 0.025f;
    [SerializeField, Range(0.04f, 0.12f)] private float _hubRadius = 0.065f;
    [SerializeField] private Color _green = new Color(0.25f, 0.84f, 0.25f, 1f);
    [SerializeField] private Color _yellow = new Color(1f, 0.82f, 0.08f, 1f);
    [SerializeField] private Color _red = new Color(0.96f, 0.23f, 0.18f, 1f);
    [SerializeField] private Color _needleColor = new Color(0.03f, 0.16f, 0.21f, 1f);
    [SerializeField] private Color _dividerColor = Color.white;
    [SerializeField, Min(0f)] private float _dividerWidth = 1.5f;

    [SerializeField, Min(0f)] private float _scalePadding = 30f;
    [SerializeField, Min(0f)] private float _tickLength = 8f;
    [SerializeField, Min(0f)] private float _tickOffset = 3f;
    [SerializeField, Min(0f)] private float _labelOffset = 12f;
    [SerializeField, Min(0f)] private float _tickWidth = 2f;

    [SerializeField, Range(1, 20)] private int _minorDivisions = 10;
    [SerializeField, Min(0f)] private float _minorTickLength = 4f;
    [SerializeField, Range(0f, 0.05f)] private float _tickMergeFraction = 0.02f;

    private int _scaleMarkCount;
    private float _speed;
    private float _greenEnd = 0.5f;
    private float _yellowEnd = 0.75f;

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        GetLayout(out Vector2 center, out float radius);
        DrawBand(mesh, center, radius, 0f, _greenEnd, _green);
        DrawBand(mesh, center, radius, _greenEnd, _yellowEnd, _yellow);
        DrawBand(mesh, center, radius, _yellowEnd, 1f, _red);

        if (_greenEnd > 0f && _greenEnd < 1f)
            DrawDivider(mesh, center, radius, _greenEnd);

        if (_yellowEnd > _greenEnd && _yellowEnd < 1f)
            DrawDivider(mesh, center, radius, _yellowEnd);

        DrawMinorTicks(mesh, center, radius);

        for (int index = 0; index < _scaleMarkCount; index++)
            DrawTick(mesh, center, radius, _scaleMarks[index], _tickLength);

        Vector2 direction = GetDirection(_speed);
        Vector2 side = new Vector2(-direction.y, direction.x) * (radius * _needleWidth);
        Vector2 tail = center - direction * (radius * _hubRadius);
        int first = mesh.currentVertCount;
        mesh.AddVert(tail - side, _needleColor, Vector2.zero);
        mesh.AddVert(tail + side, _needleColor, Vector2.zero);
        mesh.AddVert(center + direction * (radius * _needleLength), _needleColor, Vector2.zero);
        mesh.AddTriangle(first, first + 1, first + 2);
        DrawDisc(mesh, center, radius * _hubRadius, _needleColor);
        DrawDisc(mesh, center, radius * _hubRadius * 0.5f, _dividerColor);
    }

    public void SetValues(float speed, float greenEnd, float yellowEnd)
    {
        speed = Mathf.Clamp01(speed);
        greenEnd = Mathf.Clamp01(greenEnd);
        yellowEnd = Mathf.Clamp(yellowEnd, greenEnd, 1f);

        if (Mathf.Approximately(speed, _speed)
            && Mathf.Approximately(greenEnd, _greenEnd)
            && Mathf.Approximately(yellowEnd, _yellowEnd))
            return;

        _speed = speed;
        _greenEnd = greenEnd;
        _yellowEnd = yellowEnd;
        SetVerticesDirty();
    }

    public void SetScaleMarks(float[] marks, int count)
    {
        _scaleMarkCount = Mathf.Min(count, _scaleMarks.Length);

        for (int index = 0; index < _scaleMarkCount; index++)
            _scaleMarks[index] = marks[index];

        SetVerticesDirty();
    }

    public Vector2 GetScalePoint(float value)
    {
        GetLayout(out Vector2 center, out float radius);
        return center + GetDirection(value) * (radius + _tickOffset + _tickLength + _labelOffset);
    }

    private static Vector2 GetDirection(float value)
    {
        float angle = (1f - Mathf.Clamp01(value)) * SweepRadians;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private static void DrawDisc(VertexHelper mesh, Vector2 center, float radius, Color tint)
    {
        int first = mesh.currentVertCount;
        mesh.AddVert(center, tint, Vector2.zero);

        for (int index = 0; index <= HubSegments; index++)
        {
            float angle = index * Mathf.PI * 2f / HubSegments;
            mesh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, tint, Vector2.zero);

            if (index > 0)
                mesh.AddTriangle(first, first + index, first + index + 1);
        }
    }

    private static void AddQuad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
    {
        int first = mesh.currentVertCount;
        mesh.AddVert(a, tint, Vector2.zero);
        mesh.AddVert(b, tint, Vector2.zero);
        mesh.AddVert(c, tint, Vector2.zero);
        mesh.AddVert(d, tint, Vector2.zero);
        mesh.AddTriangle(first, first + 1, first + 2);
        mesh.AddTriangle(first, first + 2, first + 3);
    }

    private void GetLayout(out Vector2 center, out float radius)
    {
        Rect rect = GetPixelAdjustedRect();
        float availableWidth = Mathf.Max(0f, rect.width * 0.5f - _scalePadding);
        float availableHeight = Mathf.Max(0f, rect.height - _scalePadding);
        radius = Mathf.Min(availableWidth, availableHeight / (1f + _hubRadius)) * _radiusScale;
        center = new Vector2(rect.center.x, rect.yMin + radius * _hubRadius);
    }

    private void DrawMinorTicks(VertexHelper mesh, Vector2 center, float radius)
    {
        int divisions = Mathf.Max(1, _minorDivisions);

        for (int index = 1; index < divisions; index++)
        {
            float value = (float)index / divisions;
            bool nearBoundary = false;

            for (int mark = 0; mark < _scaleMarkCount; mark++)
                nearBoundary |= Mathf.Abs(value - _scaleMarks[mark]) <= _tickMergeFraction;

            if (nearBoundary == false)
                DrawTick(mesh, center, radius, value, _minorTickLength);
        }
    }

    private void DrawTick(VertexHelper mesh, Vector2 center, float radius, float value, float length)
    {
        Vector2 direction = GetDirection(value);
        Vector2 side = new Vector2(-direction.y, direction.x) * (_tickWidth * 0.5f);
        Vector2 inner = center + direction * (radius + _tickOffset);
        Vector2 outer = inner + direction * length;
        AddQuad(mesh, inner - side, outer - side, outer + side, inner + side, _needleColor);
    }

    private void DrawBand(VertexHelper mesh, Vector2 center, float radius, float start, float end, Color tint)
    {
        if (end <= start)
            return;

        int segments = Mathf.Max(1, Mathf.CeilToInt(_segments * (end - start)));
        float innerRadius = radius * (1f - _bandThickness);

        for (int index = 0; index < segments; index++)
        {
            Vector2 from = GetDirection(Mathf.Lerp(start, end, (float)index / segments));
            Vector2 to = GetDirection(Mathf.Lerp(start, end, (float)(index + 1) / segments));
            AddQuad(mesh, center + from * innerRadius, center + from * radius,
                center + to * radius, center + to * innerRadius, tint);
        }
    }

    private void DrawDivider(VertexHelper mesh, Vector2 center, float radius, float value)
    {
        Vector2 direction = GetDirection(value);
        Vector2 side = new Vector2(-direction.y, direction.x) * (_dividerWidth * 0.5f);
        Vector2 inner = center + direction * (radius * (1f - _bandThickness));
        Vector2 outer = center + direction * radius;
        AddQuad(mesh, inner - side, outer - side, outer + side, inner + side, _dividerColor);
    }
}
