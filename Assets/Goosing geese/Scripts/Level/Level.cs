using System;
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

    public event Action<RoadPath, RoadSettings> PathBuilt;

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

        for (int i = 0; i < _roadPath.Count; i++)
        {
            if (_roadPath.TryGetWaypoint(i, out Transform waypoint))
                Gizmos.DrawWireSphere(waypoint.position, _settings.WaypointGizmoRadius);
        }
    }

    public void BuildRoad()
    {
        RoadPieceType[] roadLayout = _roadLayoutGenerator.Generate(_settings.RoadPieceCount);
        _roadPath = _roadBuilder.Build(roadLayout, _roadStartPoint);

        _roadPath.BuildCurve(_settings.CurveSamplesPerSegment);
        _racer.SetPath(_roadPath);
        PathBuilt?.Invoke(_roadPath, _settings);
    }

    public void RestartRace()
    {
        BuildRoad();
        _racer.ResetRace();
    }

    public bool TryGetBuiltPath(out RoadPath path, out RoadSettings settings)
    {
        path = _roadPath;
        settings = _settings;
        return path != null && path.Curve != null;
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
