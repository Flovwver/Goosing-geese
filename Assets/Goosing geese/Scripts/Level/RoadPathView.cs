using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RoadPathView : MonoBehaviour
{
    [SerializeField] private Level _level;
    [SerializeField] private Material _material;

    private void OnEnable()
    {
        GetComponent<LineRenderer>().enabled = false;

        if (_level == null)
            return;

        _level.PathBuilt += OnPathBuilt;

        if (_level.TryGetBuiltPath(out RoadPath path, out RoadSettings settings))
            Show(path.Curve, settings);
    }

    private void OnDisable()
    {
        if (_level != null)
            _level.PathBuilt -= OnPathBuilt;

        GetComponent<LineRenderer>().enabled = false;
    }

    private void OnPathBuilt(RoadPath path, RoadSettings settings)
    {
        Show(path.Curve, settings);
    }

    private void Show(RoadCurve curve, RoadSettings settings)
    {
        LineRenderer line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.sharedMaterial = _material;
        line.startColor = settings.PathLineColor;
        line.endColor = settings.PathLineColor;
        line.widthMultiplier = settings.PathLineWidth;
        line.enabled = settings.ShowPathLine;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        var points = new Vector3[curve.Points.Count];

        for (int i = 0; i < points.Length; i++)
            points[i] = curve.Points[i] + Vector3.up * settings.PathLineHeight;

        line.positionCount = points.Length;
        line.SetPositions(points);
    }
}
