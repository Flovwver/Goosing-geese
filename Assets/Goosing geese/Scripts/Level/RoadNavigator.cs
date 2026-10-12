using UnityEngine;

public class RoadNavigator : MonoBehaviour
{
    [SerializeField] private NavigationSettings _settings;

    private RoadPath _path;
    private int _targetWaypointIndex;

    public Vector3 TargetPosition { get; private set; }
    public bool IsPathCompleted { get; private set; } = true;

    public void SetPath(RoadPath path)
    {
        _path = path;
        _targetWaypointIndex = 0;

        IsPathCompleted = _path.TryGetWaypoint(_targetWaypointIndex, out Transform firstWaypoint) == false;

        if (IsPathCompleted == false)
            TargetPosition = firstWaypoint.position;
    }

    public void UpdateTarget(Vector3 currentPosition)
    {
        if (IsPathCompleted)
            return;

        Vector3 offsetToTarget = TargetPosition - currentPosition;
        offsetToTarget.y = 0f;

        float squaredDistance = offsetToTarget.sqrMagnitude;

        if (squaredDistance > _settings.ArrivalDistance * _settings.ArrivalDistance)
            return;

        _targetWaypointIndex++;

        if (_path.TryGetWaypoint(_targetWaypointIndex, out Transform nextWaypoint))
            TargetPosition = nextWaypoint.position;
        else
            IsPathCompleted = true;
    }
}