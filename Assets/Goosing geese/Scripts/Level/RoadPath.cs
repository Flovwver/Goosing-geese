using System.Collections.Generic;
using UnityEngine;

public class RoadPath
{
    private readonly List<RoadPiece> _pieces = new List<RoadPiece>();
    private readonly List<Transform> _waypoints = new List<Transform>();

    public RoadPath(IReadOnlyList<RoadPiece> pieces)
    {
        foreach (RoadPiece piece in pieces)
        {
            _pieces.Add(piece);

            if (_waypoints.Count == 0)
                _waypoints.Add(piece.EntryPoint);

            _waypoints.Add(piece.ExitPoint);
        }
    }

    public IReadOnlyList<RoadPiece> Pieces => _pieces;

    public int FinishWaypointIndex { get; private set; }
    public Transform FinishPoint { get; private set; }
    public RoadCurve Curve { get; private set; }
    public int Count => _waypoints.Count;

    public void BuildCurve(int samplesPerSegment)
    {
        var points = new List<Vector3>();
        var waypointIndices = new List<int>();

        foreach (RoadPiece piece in _pieces)
        {
            var localCurve = piece.GetCurvePoints(samplesPerSegment);

            if (points.Count == 0)
            {
                points.Add(localCurve[0]);
                waypointIndices.Add(0);
            }

            for (int i = 1; i < localCurve.Count; i++)
                points.Add(localCurve[i]);

            waypointIndices.Add(points.Count - 1);
        }

        Curve = new RoadCurve(points, waypointIndices);
    }

    public void SetFinishWaypoint(int index)
    {
        FinishPoint = TryGetWaypoint(index, out Transform point) ? point : null;
        FinishWaypointIndex = FinishPoint != null ? index : 0;
    }

    public RoadPieceType GetPieceType(int index)
    {
        return _pieces[index].Type;
    }

    public bool TryGetWaypoint(int index, out Transform waypoint)
    {
        if (index < 0 || index >= _waypoints.Count)
        {
            waypoint = null;
            return false;
        }

        waypoint = _waypoints[index];
        return true;
    }
}
