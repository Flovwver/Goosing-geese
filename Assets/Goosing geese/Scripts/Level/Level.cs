using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{

    [SerializeField] private Transform _roadStartPoint;
    [SerializeField] private RoadSettings _settings;
    [SerializeField] private Racer _racer;

    private RoadLayoutGenerator _roadLayoutGenerator;
    private RoadBuilder _roadBuilder;
    private RoadPath _roadPath;

    private void Awake()
    {
        _roadLayoutGenerator = new RoadLayoutGenerator(_settings.AvailablePieceTypes, _settings.MaxGenerationAttempts);
        _roadBuilder = new RoadBuilder(BuildPiecePrefabsByType());
    }

    private void Start()
    {
        BuildRoad();
    }

    private void OnDrawGizmos()
    {
        if (_settings == null || _settings.DrawPathGizmos == false || _roadPath == null)
            return;

        Gizmos.color = Color.yellow;

        Vector3? previousPosition = null;

        for (int waypointIndex = 0; waypointIndex < _roadPath.Count; waypointIndex++)
        {
            if (_roadPath.TryGetWaypoint(waypointIndex, out Transform waypoint) == false || waypoint == null)
                continue;

            Gizmos.DrawWireSphere(waypoint.position, _settings.WaypointGizmoRadius);

            if (previousPosition.HasValue)
                Gizmos.DrawLine(previousPosition.Value, waypoint.position);

            previousPosition = waypoint.position;
        }
    }

    public void BuildRoad()
    {
        RoadPieceType[] roadLayout = _roadLayoutGenerator.Generate(_settings.RoadPieceCount);
        _roadPath = _roadBuilder.Build(roadLayout, _roadStartPoint);

        _racer.SetPath(_roadPath);
    }

    public void RestartRace()
    {
        BuildRoad();
        _racer.ResetRace();
    }

    private Dictionary<RoadPieceType, RoadPiece> BuildPiecePrefabsByType()
    {
        var piecePrefabsByType = new Dictionary<RoadPieceType, RoadPiece>();

        foreach (RoadSettings.PiecePrefabEntry entry in _settings.PiecePrefabs)
        {
            piecePrefabsByType.Add(entry.Type, entry.Prefab);
        }

        return piecePrefabsByType;
    }
}
