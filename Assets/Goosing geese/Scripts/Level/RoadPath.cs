using System.Collections.Generic;
using UnityEngine;

public class RoadPath
{
    private readonly List<Transform> _waypoints = new ();

    public int Count => _waypoints.Count;

    public RoadPath(IReadOnlyList<RoadPiece> pieces)
    {
        if (pieces.Count == 0)
            return;

        _waypoints.Add(pieces[0].EntryPoint);

        for (int pieceIndex = 0; pieceIndex < pieces.Count; pieceIndex++)
        {
            IReadOnlyList<Transform> pieceWaypoints = pieces[pieceIndex].Waypoints;

            for (int waypointIndex = 0; waypointIndex < pieceWaypoints.Count; waypointIndex++)
            {
                _waypoints.Add(pieceWaypoints[waypointIndex]);
            }
        }

        _waypoints.Add(pieces[pieces.Count - 1].ExitPoint);
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