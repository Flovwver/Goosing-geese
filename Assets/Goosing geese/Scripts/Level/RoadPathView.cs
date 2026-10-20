using System.Collections.Generic;
using UnityEngine;

public class RoadPathView : MonoBehaviour
{
    private const float GuidanceSectionLength = 5f;

    [SerializeField] private Level _level;
    [SerializeField] private Material _material;
    [SerializeField, Min(0f)] private float _previewDistance = 45f;

    private GameObject _lines;

    private void OnEnable()
    {
        if (_level == null)
            return;

        _level.PathBuilt += OnPathBuilt;

        if (_level.TryGetBuiltPath(out RoadPath path, out RoadSettings settings))
            Show(path, settings);
    }

    private void OnDisable()
    {
        if (_level != null)
            _level.PathBuilt -= OnPathBuilt;

        Clear();
    }

    private Color GetRecommendedColor(RoadPath path, int segment, float distance)
    {
        bool corner = path.Pieces[segment].Type != RoadPieceType.Straight;

        if (segment + 1 < path.Pieces.Count && path.Curve.GetSegmentEndDistance(segment) - distance <= _previewDistance)
            corner |= path.Pieces[segment + 1].Type != RoadPieceType.Straight;

        return corner ? Color.green : Color.yellow;
    }

    private void OnPathBuilt(RoadPath path, RoadSettings settings)
    {
        Show(path, settings);
    }

    private void Show(RoadPath path, RoadSettings settings)
    {
        Clear();

        if (settings.ShowPathLine == false)
            return;

        _lines = new GameObject("Speed guidance");
        _lines.transform.SetParent(transform, false);
        var points = new List<Vector3>();
        Color color = Color.clear;
        Vector3 heightOffset = Vector3.up * settings.PathLineHeight;
        float start = 0f;

        for (int segment = 0; segment < path.Curve.SegmentCount; segment++)
        {
            float end = path.Curve.GetSegmentEndDistance(segment);

            for (float distance = start; distance < end; distance += GuidanceSectionLength)
            {
                Color sectionColor = GetRecommendedColor(path, segment, distance);

                if (points.Count > 0 && sectionColor != color)
                {
                    AddLine(points, color, settings);
                    points.Clear();
                }

                color = sectionColor;

                if (points.Count == 0)
                    points.Add(path.Curve.GetPointAtDistance(distance) + heightOffset);

                points.Add(path.Curve.GetPointAtDistance(Mathf.Min(end, distance + GuidanceSectionLength)) + heightOffset);
            }

            start = end;
        }

        if (points.Count > 1)
            AddLine(points, color, settings);
    }

    private void AddLine(List<Vector3> points, Color color, RoadSettings settings)
    {
        var go = new GameObject("Speed section");
        go.transform.SetParent(_lines.transform, false);
        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.sharedMaterial = _material;
        line.startWidth = settings.PathLineWidth;
        line.endWidth = settings.PathLineWidth;
        line.startColor = color;
        line.endColor = color;
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    private void Clear()
    {
        if (_lines == null)
            return;

        _lines.SetActive(false);
        Destroy(_lines);
        _lines = null;
    }
}
