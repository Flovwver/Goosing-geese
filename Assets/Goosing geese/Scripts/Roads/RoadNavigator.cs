using System;
using GoosingGeese.Infrastructure;
using GoosingGeese.Settings;
using UnityEngine;

namespace GoosingGeese.Roads
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "RoadNavigator")]
    public class RoadNavigator : MonoBehaviour
    {
        private readonly EventPublisher _events = new EventPublisher();
        [SerializeField]
        private NavigationSettings _settings;
        private RoadPath _path;
        private NavigationConfiguration _configuration;
        private int _currentSegment;
        private float _distanceAlongPath;
        private float _laneOffset;

        public event Action PathCompleted;

        public NavigationConfiguration Configuration => _configuration;
        public int CurrentSegment => _currentSegment;
        public float DistanceAlongPath => _distanceAlongPath;
        public Vector3 TargetPosition { get; private set; }
        public Vector3 ClosestPosition { get; private set; }
        public bool IsPathCompleted { get; private set; } = true;
        public bool IsFinishReached { get; private set; }

        private void Awake()
        {
            _configuration = CaptureConfiguration();
        }

        private void OnEnable()
        {
            if (_configuration == null)
                _configuration = CaptureConfiguration();
        }

        private void OnDrawGizmosSelected()
        {
            if (_path == null || IsPathCompleted)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(ClosestPosition, _configuration.DebugPointRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(TargetPosition, _configuration.DebugPointRadius);
            Gizmos.DrawLine(transform.position, TargetPosition);
        }

        public NavigationConfiguration CaptureConfiguration()
        {
            return new NavigationConfiguration(_settings);
        }

        public void Configure(NavigationConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SetLaneOffset(float offset)
        {
            _laneOffset = offset;
        }

        public void SetPath(RoadPath path)
        {
            _path = path;
            _currentSegment = 0;
            _distanceAlongPath = 0f;
            IsFinishReached = false;
            IsPathCompleted = path == null || path.Curve == null || path.Curve.Points.Count == 0;

            if (IsPathCompleted == false)
            {
                ClosestPosition = path.Curve.GetPointAtDistance(0f);
                TargetPosition = path.Curve.GetPointAtDistance(_configuration.LookAheadDistance);
            }
        }

        public void UpdateTarget(Vector3 currentPosition)
        {
            if (IsPathCompleted || _path == null || _path.Curve == null)
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

            if (IsFinishReached == false && _path.FinishWaypointIndex > 0 && _distanceAlongPath >= curve.GetSegmentEndDistance(_path.FinishWaypointIndex - 1))
            {
                Vector3 offsetFromRoute = currentPosition - ClosestPosition;
                offsetFromRoute.y = 0f;
                IsFinishReached = offsetFromRoute.sqrMagnitude <= _configuration.FinishHalfWidth * _configuration.FinishHalfWidth;
            }

            TargetPosition = curve.GetPointAtDistance(_distanceAlongPath + Mathf.Max(0f, _configuration.LookAheadDistance));
            Vector3 tangent = curve.GetPointAtDistance(_distanceAlongPath + _configuration.LookAheadDistance + 1f) - TargetPosition;
            TargetPosition += Vector3.Cross(Vector3.up, tangent.normalized) * _laneOffset;

            if (_currentSegment >= curve.SegmentCount)
            {
                Vector3 offset = currentPosition - curve.GetPointAtDistance(curve.Length);
                offset.y = 0f;
                IsPathCompleted = offset.sqrMagnitude <= _configuration.ArrivalDistance * _configuration.ArrivalDistance;

                if (IsPathCompleted)
                    _events.Publish(PathCompleted, this);
            }
        }
    }
}
