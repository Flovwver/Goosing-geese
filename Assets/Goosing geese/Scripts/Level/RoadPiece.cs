using System.Collections.Generic;
using UnityEngine;

public class RoadPiece : MonoBehaviour
{
    private const int PreviewSamples = 24;
    [SerializeField] private RoadPieceType _type;
    [SerializeField] private Transform _entryPoint;
    [SerializeField] private Transform _exitPoint;
    [Header("Local path curve")]
    [SerializeField] private Transform _curveStartControl;
    [SerializeField] private Transform _curveEndControl;
    [SerializeField] private float _curvePreviewHeight = 2.1f;

    public RoadPieceType Type => _type;
    public Transform EntryPoint => _entryPoint;
    public Transform ExitPoint => _exitPoint;

    private void OnDrawGizmos()
    {
        if (_entryPoint == null || _exitPoint == null)
            return;

        Gizmos.color = Color.cyan;
        Vector3 offset = Vector3.up * _curvePreviewHeight;
        Vector3 previous = EvaluateCurve(0f) + offset;

        for (int i = 1; i <= PreviewSamples; i++)
        {
            Vector3 point = EvaluateCurve((float)i / PreviewSamples) + offset;
            Gizmos.DrawLine(previous, point);
            previous = point;
        }

        Gizmos.color = Color.yellow;

        if (_curveStartControl != null)
            Gizmos.DrawLine(_entryPoint.position + offset, _curveStartControl.position + offset);

        if (_curveEndControl != null)
            Gizmos.DrawLine(_exitPoint.position + offset, _curveEndControl.position + offset);
    }

    public List<Vector3> GetCurvePoints(int samples)
    {
        samples = Mathf.Max(1, samples);
        var points = new List<Vector3>(samples + 1);

        for (int i = 0; i <= samples; i++)
            points.Add(EvaluateCurve((float)i / samples));

        return points;
    }

    private Vector3 EvaluateCurve(float t)
    {
        Vector3 start = _entryPoint.position;
        Vector3 end = _exitPoint.position;

        if (_curveStartControl == null || _curveEndControl == null)
            return Vector3.Lerp(start, end, t);

        float u = 1f - t;
        return u * u * u * start + 3f * u * u * t * _curveStartControl.position
            + 3f * u * t * t * _curveEndControl.position + t * t * t * end;
    }

}
