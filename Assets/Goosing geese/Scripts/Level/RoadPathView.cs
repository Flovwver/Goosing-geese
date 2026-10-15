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

    private static Color GetSegmentColor(RoadSpeedProfile profile, int index)
    {
        float speed = Mathf.Min(profile.Limits[index], profile.Limits[index + 1]);
        return SpeedZones.GetColor(speed, profile.Stats);
    }

    private void OnPathBuilt(RoadPath path, RoadConfiguration settings)
    {
        Show(path, settings);
    }

    private void Show(RoadPath path, RoadConfiguration settings)
    {
        ClearHints();
        LineRenderer line = GetComponent<LineRenderer>();
        ConfigureLine(line, settings, settings.PathLineColor);
        line.enabled = settings.ShowPathLine && settings.ShowSpeedHints == false;
        var points = new Vector3[path.Curve.Points.Count];

        for (int index = 0; index < points.Length; index++)
            points[index] = path.Curve.Points[index] + Vector3.up * settings.PathLineHeight;

        line.positionCount = points.Length;
        line.SetPositions(points);

        if (settings.ShowPathLine == false || settings.ShowSpeedHints == false)
            return;

        RoadSpeedProfile profile = _level.SpeedProfile;
        var run = new List<Vector3> { points[0] };
        Color color = GetSegmentColor(profile, 0);

        for (int index = 0; index < points.Length - 1; index++)
        {
            Color nextColor = GetSegmentColor(profile, index);

            if (nextColor != color)
            {
                AddHint(run, color, settings);
                run.Clear();
                run.Add(points[index]);
                color = nextColor;
            }

            run.Add(points[index + 1]);
        }

        AddHint(run, color, settings);
    }

    private void AddHint(List<Vector3> points, Color color, RoadConfiguration settings)
    {
        var hint = new GameObject($"Speed hint {_hintLines.Count}");
        hint.transform.SetParent(transform, false);
        LineRenderer line = hint.AddComponent<LineRenderer>();
        ConfigureLine(line, settings, color);
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        _hintLines.Add(line);
    }

    private void ConfigureLine(LineRenderer line, RoadConfiguration settings, Color color)
    {
        line.useWorldSpace = true;
        line.sharedMaterial = _material;
        line.startColor = color;
        line.endColor = color;
        line.widthMultiplier = settings.PathLineWidth;
        line.enabled = true;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
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
