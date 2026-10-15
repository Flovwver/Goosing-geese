using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RoadPathView : MonoBehaviour
{
    private readonly List<LineRenderer> _hintLines = new List<LineRenderer>();

    [SerializeField] private Level _level;
    [SerializeField] private Material _material;

    private void OnEnable()
    {
        GetComponent<LineRenderer>().enabled = false;

        if (_level == null)
            return;

        _level.PathBuilt += OnPathBuilt;

        if (_level.TryGetBuiltPath(out RoadPath path, out RoadConfiguration settings))
            Show(path, settings);
    }

    private void OnDisable()
    {
        if (_level != null)
            _level.PathBuilt -= OnPathBuilt;

        GetComponent<LineRenderer>().enabled = false;
        ClearHints();
    }

    private void OnPathBuilt(RoadPath path, RoadConfiguration settings)
    {
        Show(path, settings);
    }

    private void Show(RoadPath path, RoadConfiguration settings)
    {
        ClearHints();
        RoadCurve curve = path.Curve;
        LineRenderer line = GetComponent<LineRenderer>();
        ConfigureLine(line, settings);
        line.enabled = settings.ShowPathLine && settings.ShowSpeedHints == false;
        var points = new Vector3[curve.Points.Count];

        for (int i = 0; i < points.Length; i++)
            points[i] = curve.Points[i] + Vector3.up * settings.PathLineHeight;

        line.positionCount = points.Length;
        line.SetPositions(points);

        if (settings.ShowPathLine == false || settings.ShowSpeedHints == false)
            return;

        for (int segment = 0; segment < curve.SegmentCount; segment++)
        {
            var hint = new GameObject($"Speed hint {segment}");
            hint.transform.SetParent(transform, false);
            LineRenderer hintLine = hint.AddComponent<LineRenderer>();
            ConfigureLine(hintLine, settings);
            int startIndex = curve.GetSegmentStartIndex(segment);
            int endIndex = curve.GetSegmentEndIndex(segment);
            var segmentPoints = new Vector3[endIndex - startIndex + 1];
            System.Array.Copy(points, startIndex, segmentPoints, 0, segmentPoints.Length);
            hintLine.positionCount = segmentPoints.Length;
            hintLine.SetPositions(segmentPoints);
            hintLine.colorGradient = BuildSpeedGradient(path, settings, segment);
            _hintLines.Add(hintLine);
        }
    }

    private void ConfigureLine(LineRenderer line, RoadConfiguration settings)
    {
        line.useWorldSpace = true;
        line.sharedMaterial = _material;
        line.startColor = settings.PathLineColor;
        line.endColor = settings.PathLineColor;
        line.widthMultiplier = settings.PathLineWidth;
        line.enabled = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    private Gradient BuildSpeedGradient(RoadPath path, RoadConfiguration settings, int segment)
    {
        Color color = path.GetPieceType(segment) == RoadPieceType.Straight ? settings.FastPathColor : settings.SlowPathColor;
        var keys = new List<GradientColorKey> { new GradientColorKey(color, 0f) };
        float nextTurnDistance = float.PositiveInfinity;

        for (int next = segment + 1; next < path.Curve.SegmentCount; next++)
        {
            if (path.GetPieceType(next) == RoadPieceType.Straight)
                continue;

            nextTurnDistance = path.Curve.GetSegmentEndDistance(next - 1);
            break;
        }

        if (path.GetPieceType(segment) == RoadPieceType.Straight && float.IsPositiveInfinity(nextTurnDistance) == false && settings.TurnApproachDistance > 0f)
        {
            float startDistance = segment == 0 ? 0f : path.Curve.GetSegmentEndDistance(segment - 1);
            float length = path.Curve.GetSegmentEndDistance(segment) - startDistance;
            float approach = length > 0f ? (nextTurnDistance - settings.TurnApproachDistance - startDistance) / length : 0f;

            if (approach <= 0f)
            {
                keys[0] = new GradientColorKey(settings.SlowPathColor, 0f);
            }
            else if (approach < 1f)
            {
                if (approach > 0.02f)
                    keys.Add(new GradientColorKey(color, approach - 0.02f));

                keys.Add(new GradientColorKey(settings.SlowPathColor, approach));
            }
        }

        keys.Add(new GradientColorKey(keys[keys.Count - 1].color, 1f));
        var gradient = new Gradient { mode = GradientMode.Blend };
        gradient.SetKeys(keys.ToArray(), new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return gradient;
    }

    private void ClearHints()
    {
        foreach (LineRenderer line in _hintLines)
        {
            if (line == null)
                continue;

            line.gameObject.SetActive(false);
            Destroy(line.gameObject);
        }

        _hintLines.Clear();
    }
}
