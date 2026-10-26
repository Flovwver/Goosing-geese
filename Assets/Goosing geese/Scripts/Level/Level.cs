using System;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{

    [SerializeField] private Transform _roadStartPoint;
    [SerializeField] private RoadSettings _settings;
    [SerializeField] private Racer _racer;

    [SerializeField, Min(0f)] private float _minimumRoadSpacing = 24f;
    [SerializeField, Min(1)] private int _layoutValidationAttempts = 20;

    private RoadLayoutGenerator _roadLayoutGenerator;
    private RoadBuilder _roadBuilder;
    private RoadLayoutGeometry _layoutGeometry;
    private RoadPath _roadPath;

    private List<RoadPiece> _builtPieces;
    private bool _restoreAfterReload;
    private RoadPieceType[] _currentLayout;
    private int _mainPieceCount;
    private int _curveSamples;

    public event Action<RoadPath, RoadSettings> PathBuilt;

    public int Seed { get; private set; }
    public int Number { get; private set; }

    private void Awake()
    {
        InitializeComponents();
    }

    private void OnEnable()
    {
        InitializeComponents();
        _restoreAfterReload = _roadPath == null && _builtPieces.Count > 0;
    }

    private void Start()
    {
        BuildRoad();
    }

    private void Update()
    {
        if (_restoreAfterReload == false)
            return;

        _restoreAfterReload = false;
        _roadPath = new RoadPath(_builtPieces);
        ConfigurePath();
        _racer.ResetRace();
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
        CreateLevel(GetComponent<RaceController>().CurrentStage.Seed);
    }

    public void CreateLevel(int seed)
    {
        InitializeComponents();
        Seed = seed;
        RaceController race = GetComponent<RaceController>();
        int level = race != null ? race.Progress.Level : 1;
        Number = level;
        CampaignStage stage = race.CurrentStage;
        _mainPieceCount = Mathf.Max(1, stage.PieceCount);
        _curveSamples = Mathf.Max(1, _settings.CurveSamplesPerSegment);
        int runoutPieces = Mathf.Max(1, _settings.FinishRunoutPieces);
        if (seed == stage.Seed && stage.Layout != null && stage.Layout.Length >= _mainPieceCount + runoutPieces)
        {
            _currentLayout = (RoadPieceType[])stage.Layout.Clone();
            BuildCurrentLayout();
            return;
        }

        UnityEngine.Random.State randomState = UnityEngine.Random.state;

        try
        {
            UnityEngine.Random.InitState(seed);
            _currentLayout = new RoadPieceType[0];

            for (int attempt = 0; attempt < Mathf.Max(1, _layoutValidationAttempts); attempt++)
            {
                RoadPieceType[] candidate = _roadLayoutGenerator.Generate(_mainPieceCount, runoutPieces);

                if (_layoutGeometry.IsValid(candidate, _roadStartPoint, _minimumRoadSpacing, _curveSamples))
                {
                    _currentLayout = candidate;
                    break;
                }
            }
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
        InitializeComponents();
        _restoreAfterReload = false;
        _roadPath = _roadBuilder.Build(_currentLayout, _roadStartPoint);
        ConfigurePath();
    }

    private void ConfigurePath()
    {
        _roadPath.SetFinishWaypoint(_mainPieceCount);

        _roadPath.BuildCurve(_curveSamples);
        _racer.SetPath(_roadPath);
        PathBuilt?.Invoke(_roadPath, _settings);
    }

    private void InitializeComponents()
    {
        if (_roadBuilder != null)
            return;

        if (_builtPieces == null)
            _builtPieces = new List<RoadPiece>();

        _roadLayoutGenerator = new RoadLayoutGenerator(_settings.AvailablePieceTypes, _settings.MaxGenerationAttempts);
        var prefabs = BuildPiecePrefabsByType();
        _roadBuilder = new RoadBuilder(prefabs, _builtPieces);
        _layoutGeometry = new RoadLayoutGeometry(prefabs);
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
