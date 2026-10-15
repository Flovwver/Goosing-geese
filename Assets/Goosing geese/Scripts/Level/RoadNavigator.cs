using System;
using UnityEngine;

public class RoadNavigator : MonoBehaviour
{
    [SerializeField] private NavigationSettings _settings;

    private RoadPath _path;
    private NavigationConfiguration _configuration;
    private int _currentSegment;
    private float _distanceAlongPath;

    public event Action PathCompleted;

    public Vector3 TargetPosition { get; private set; }
    public Vector3 ClosestPosition { get; private set; }
    public bool IsPathCompleted { get; private set; } = true;
    public NavigationConfiguration Configuration => _configuration;

    private void Awake()
    {
        _configuration = CaptureConfiguration();
    }

    private void OnDrawGizmosSelected()
    {
        if (_path == null || IsPathCompleted)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(ClosestPosition, _settings.DebugPointRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(TargetPosition, _settings.DebugPointRadius);
        Gizmos.DrawLine(transform.position, TargetPosition);
    }

    public void SetPath(RoadPath path)
    {
        _path = path;
        _currentSegment = 0;
        _distanceAlongPath = 0f;
        IsPathCompleted = path == null || path.Curve == null || path.Curve.Points.Count == 0;

        if (IsPathCompleted == false)
        {
            ClosestPosition = path.Curve.GetPointAtDistance(0f);
            TargetPosition = path.Curve.GetPointAtDistance(_configuration.LookAheadDistance);
        }
    }

    public void UpdateTarget(Vector3 currentPosition)
    {
        if (IsPathCompleted)
            return;

        RoadCurve curve = _path.Curve;

        while (_currentSegment < curve.SegmentCount)
        {
            _distanceAlongPath = curve.FindClosestDistance(currentPosition, _currentSegment, _distanceAlongPath);
            float endDistance = curve.GetSegmentEndDistance(_currentSegment);

            if (endDistance - _distanceAlongPath > _configuration.WaypointPassTolerance)
                break;

            _distanceAlongPath = endDistance;
            _currentSegment++;
        }

        ClosestPosition = curve.GetPointAtDistance(_distanceAlongPath);
        TargetPosition = curve.GetPointAtDistance(_distanceAlongPath + Mathf.Max(0f, _configuration.LookAheadDistance));

        if (_currentSegment >= curve.SegmentCount)
        {
            Vector3 offset = currentPosition - curve.GetPointAtDistance(curve.Length);
            offset.y = 0f;
            IsPathCompleted = offset.sqrMagnitude <= _configuration.ArrivalDistance * _configuration.ArrivalDistance;

            if (IsPathCompleted)
                PathCompleted?.Invoke();
        }
    }

    public NavigationConfiguration CaptureConfiguration()
    {
        return new NavigationConfiguration(_settings);
    }

    public void Configure(NavigationConfiguration configuration)
    {
        _configuration = configuration;
    }

}
