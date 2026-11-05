using System.Collections.Generic;
using UnityEngine;

namespace GoosingGeese.Roads
{
    public class RoadCurve
    {
        private const float MinimumSegmentLength = 0.0001f;
        private readonly List<Vector3> _points = new List<Vector3>();
        private readonly List<float> _distances = new List<float>();
        private readonly List<int> _waypointIndices = new List<int>();
        private IReadOnlyList<Vector3> _readOnlyView;

        public RoadCurve(IReadOnlyList<Vector3> points, IReadOnlyList<int> waypointIndices)
        {
            foreach (Vector3 point in points)
                AddPoint(point);

            foreach (int index in waypointIndices)
                _waypointIndices.Add(index);
        }

        public IReadOnlyList<Vector3> Points => _readOnlyView ?? (_readOnlyView = _points.AsReadOnly());
        public int SegmentCount => Mathf.Max(0, _waypointIndices.Count - 1);
        public float Length => _distances.Count > 0 ? _distances[_distances.Count - 1] : 0f;

        public float GetSegmentEndDistance(int segment)
        {
            return _distances[_waypointIndices[segment + 1]];
        }

        public int GetSegmentStartIndex(int segment)
        {
            return _waypointIndices[segment];
        }

        public int GetSegmentEndIndex(int segment)
        {
            return _waypointIndices[segment + 1];
        }

        public float FindClosestDistance(Vector3 position, int segment, float minimumDistance)
        {
            float bestDistance = Mathf.Clamp(minimumDistance, _distances[_waypointIndices[segment]], GetSegmentEndDistance(segment));
            float bestSquaredDistance = float.PositiveInfinity;

            for (int i = _waypointIndices[segment]; i < _waypointIndices[segment + 1]; i++)
            {
                if (_distances[i + 1] < minimumDistance)
                    continue;

                Vector3 offset = _points[i + 1] - _points[i];
                offset.y = 0f;
                Vector3 toPosition = position - _points[i];
                toPosition.y = 0f;
                float length = _distances[i + 1] - _distances[i];
                float minT = length > MinimumSegmentLength ? Mathf.Clamp01((minimumDistance - _distances[i]) / length) : 0f;
                float t = offset.sqrMagnitude > MinimumSegmentLength * MinimumSegmentLength ? Mathf.Clamp(Vector3.Dot(toPosition,
                    offset) / offset.sqrMagnitude,
                    minT,
                    1f) : minT;
                Vector3 delta = position - Vector3.Lerp(_points[i], _points[i + 1], t);
                delta.y = 0f;

                if (delta.sqrMagnitude < bestSquaredDistance)
                {
                    bestSquaredDistance = delta.sqrMagnitude;
                    bestDistance = Mathf.Lerp(_distances[i], _distances[i + 1], t);
                }
            }

            return bestDistance;
        }

        public Vector3 GetPointAtDistance(float distance)
        {
            if (_points.Count == 0)
                return Vector3.zero;

            distance = Mathf.Clamp(distance, 0f, Length);
            int low = 0;
            int high = _distances.Count - 1;

            while (low < high)
            {
                int middle = (low + high) / 2;

                if (_distances[middle] < distance)
                    low = middle + 1;
                else
                    high = middle;
            }

            if (low == 0)
                return _points[0];

            float length = _distances[low] - _distances[low - 1];
            float t = length > MinimumSegmentLength ? (distance - _distances[low - 1]) / length : 0f;
            return Vector3.Lerp(_points[low - 1], _points[low], t);
        }

        private void AddPoint(Vector3 point)
        {
            float distance = _points.Count > 0 ? _distances[_distances.Count - 1] + Vector3.Distance(_points[_points.Count - 1], point) : 0f;
            _points.Add(point);
            _distances.Add(distance);
        }
    }
}
