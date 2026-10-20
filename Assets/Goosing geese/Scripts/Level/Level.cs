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
    [SerializeField, Min(0)] private int _piecesPerLevel = 2;
    [SerializeField, Min(1)] private int _maximumPieces = 60;

    private RoadPieceType[] _currentLayout;
    private int _mainPieceCount;
    private int _curveSamples;

    public event Action<RoadPath, RoadSettings> PathBuilt;

    public int Seed { get; private set; }
    public int Number { get; private set; }

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
        CreateLevel(Environment.TickCount);
    }

    public void CreateLevel(int seed)
    {
        Seed = seed;
        RaceController race = GetComponent<RaceController>();
        int level = race != null ? race.Progress.Level : 1;
        Number = level;
        _mainPieceCount = Mathf.Clamp(_settings.RoadPieceCount + (level - 1) * _piecesPerLevel, 1, _maximumPieces);
        _curveSamples = Mathf.Max(1, _settings.CurveSamplesPerSegment);
        int runoutPieces = Mathf.Max(1, _settings.FinishRunoutPieces);
        UnityEngine.Random.State randomState = UnityEngine.Random.state;

        try
        {
            UnityEngine.Random.InitState(seed);
            _currentLayout = _roadLayoutGenerator.Generate(_mainPieceCount, runoutPieces);
        }
        finally
        {
            UnityEngine.Random.state = randomState;
        }

        if (_currentLayout.Length == 0)
        {
            _currentLayout = new RoadPieceType[_mainPieceCount + runoutPieces];

            for (int i = 0; i < _currentLayout.Length; i++)
                _currentLayout[i] = RoadPieceType.Straight;
        }

        BuildCurrentLayout();
    }

    public void RestartRace()
    {
        BuildCurrentLayout();
        _racer.ResetRace();
    }

    public bool TryGetBuiltPath(out RoadPath path, out RoadSettings settings)
    {
        path = _roadPath;
        settings = _settings;
        return path != null && path.Curve != null;
    }

    private void BuildCurrentLayout()
    {
        _roadPath = _roadBuilder.Build(_currentLayout, _roadStartPoint);
        _roadPath.SetFinishWaypoint(_mainPieceCount);

        _roadPath.BuildCurve(_curveSamples);
        _racer.SetPath(_roadPath);
        PathBuilt?.Invoke(_roadPath, _settings);
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
