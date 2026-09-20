using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private const float WaypointGizmoRadius = 0.3f;

    [SerializeField] private Transform _roadStartPoint;
    [SerializeField] private RoadPiece _straightPiecePrefab;
    [SerializeField] private RoadPiece _leftTurnPiecePrefab;
    [SerializeField] private RoadPiece _rightTurnPiecePrefab;
    [SerializeField] private int _roadPieceCount = 20;
    [SerializeField] private Racer _racer;
    [SerializeField] private bool _drawPathGizmos = true;

    private RoadLayoutGenerator _roadLayoutGenerator;
    private RoadBuilder _roadBuilder;
    private RoadPath _roadPath;

    private void Awake()
    {
        _roadLayoutGenerator = new RoadLayoutGenerator(new[] { RoadPieceType.Straight, RoadPieceType.LeftTurn, RoadPieceType.RightTurn });
        _roadBuilder = new RoadBuilder(BuildPiecePrefabsByType());
    }

    private void Start()
    {
        BuildRoad();
    }

    private void OnDrawGizmos()
    {
        if (_drawPathGizmos == false || _roadPath == null)
            return;

        Gizmos.color = Color.yellow;

        Vector3? previousPosition = null;

        for (int waypointIndex = 0; waypointIndex < _roadPath.Count; waypointIndex++)
        {
            if (_roadPath.TryGetWaypoint(waypointIndex, out Transform waypoint) == false || waypoint == null)
                continue;

            Gizmos.DrawWireSphere(waypoint.position, WaypointGizmoRadius);

            if (previousPosition.HasValue)
                Gizmos.DrawLine(previousPosition.Value, waypoint.position);

            previousPosition = waypoint.position;
        }
    }

    [ContextMenu("Build road")]
    public void BuildRoad()
    {
        RoadPieceType[] roadLayout = _roadLayoutGenerator.Generate(_roadPieceCount);
        _roadPath = _roadBuilder.Build(roadLayout, _roadStartPoint);

        _racer.SetPath(_roadPath);
    }

    private Dictionary<RoadPieceType, RoadPiece> BuildPiecePrefabsByType()
    {
        return new Dictionary<RoadPieceType, RoadPiece>
        {
            { RoadPieceType.Straight, _straightPiecePrefab },
            { RoadPieceType.LeftTurn, _leftTurnPiecePrefab },
            { RoadPieceType.RightTurn, _rightTurnPiecePrefab }
        };
    }
}