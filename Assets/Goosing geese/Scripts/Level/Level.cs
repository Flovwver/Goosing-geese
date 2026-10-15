using System;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{

    [SerializeField] private Transform _roadStartPoint;
    [SerializeField] private RoadSettings _settings;
    [SerializeField] private Racer _racer;
    [SerializeField] private FinishLine _finishLine;
    [SerializeField] private RaceController _race;

    private RoadBuilder _roadBuilder;
    private RoadPath _roadPath;
    private Vector3 _racerStartPosition;
    private Quaternion _racerStartRotation;

    public event Action<RoadPath, RoadSettings> PathBuilt;

    public LevelDefinition CurrentDefinition { get; private set; }

    private void Awake()
    {
        _racerStartPosition = _racer.transform.position;
        _racerStartRotation = _racer.transform.rotation;
    }

    private void OnEnable()
    {
        _finishLine.Crossed += OnFinishCrossed;
    }

    private void Start()
    {
        BuildRoad();
    }

    private void OnDrawGizmos()
    {
        RoadSettings settings = CurrentDefinition?.Settings;

        if (settings == null || settings.DrawPathGizmos == false || _roadPath == null)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < _roadPath.Count; i++)
        {
            if (_roadPath.TryGetWaypoint(i, out Transform waypoint))
                Gizmos.DrawWireSphere(waypoint.position, settings.WaypointGizmoRadius);
        }
    }

    private void OnDisable()
    {
        _finishLine.Crossed -= OnFinishCrossed;
    }

    private void OnDestroy()
    {
        _roadBuilder?.Clear();

        if (CurrentDefinition != null)
            Destroy(CurrentDefinition.Settings);
    }

    public void BuildRoad()
    {
        if (TryBuildRoad(out string error) == false)
            Debug.LogError($"Level creation failed: {error}", this);
    }

    public bool TryBuildRoad(out string error)
    {
        RoadSettings settings = null;
        RoadBuilder builder = null;
        LevelDefinition definition;
        RoadPath path;

        try
        {
            if (_settings == null || _roadStartPoint == null)
                throw new InvalidOperationException("Road settings and a start point must be assigned.");

            settings = Instantiate(_settings);
            settings.hideFlags = HideFlags.HideAndDontSave;

            if (settings.RoadPieceCount < 1 || settings.FinishRunoutPieces < 1 || settings.CurveSamplesPerSegment < 1)
                throw new ArgumentException("Road length, finish runout and curve sample count must be positive.");

            Dictionary<RoadPieceType, RoadPiece> prefabs = BuildPiecePrefabsByType(settings);
            int seed = Guid.NewGuid().GetHashCode();
            var generator = new RoadLayoutGenerator(settings.AvailablePieceTypes, settings.MaxGenerationAttempts, seed);
            RoadPieceType[] layout = generator.Generate(settings.RoadPieceCount, settings.FinishRunoutPieces);
            definition = new LevelDefinition(seed, layout, settings, _roadStartPoint.position, _roadStartPoint.rotation);
            builder = new RoadBuilder(prefabs);
            path = builder.Build(definition.Layout, definition.StartPosition, definition.StartRotation);
            path.SetFinishWaypoint(layout.Length - settings.FinishRunoutPieces);
            path.BuildCurve(settings.CurveSamplesPerSegment);

            if (path.FinishPoint == null || path.Curve.Length <= 0f || float.IsNaN(path.Curve.Length) || float.IsInfinity(path.Curve.Length))
                throw new InvalidOperationException("The built road has invalid geometry or no finish.");
        }
        catch (Exception exception)
        {
            builder?.Clear();

            if (settings != null)
                Destroy(settings);

            error = exception.Message;
            return false;
        }

        _roadBuilder?.Clear();

        if (CurrentDefinition != null)
            Destroy(CurrentDefinition.Settings);

        CurrentDefinition = definition;
        _roadBuilder = builder;
        _roadPath = path;
        builder.Activate();
        PlaceFinishLine();
        PrepareRace();
        PathBuilt?.Invoke(_roadPath, settings);
        error = null;
        return true;
    }

    public void RestartRace()
    {
        if (_roadPath == null)
            return;

        PrepareRace();
    }

    public bool TryGetBuiltPath(out RoadPath path, out RoadSettings settings)
    {
        path = _roadPath;
        settings = CurrentDefinition?.Settings;
        return path != null && path.Curve != null;
    }

    private Dictionary<RoadPieceType, RoadPiece> BuildPiecePrefabsByType(RoadSettings settings)
    {
        var piecePrefabsByType = new Dictionary<RoadPieceType, RoadPiece>();

        if (settings.PiecePrefabs == null)
            throw new ArgumentException("Road prefabs must be assigned.");

        foreach (RoadSettings.PiecePrefabEntry entry in settings.PiecePrefabs)
        {
            if (entry == null || entry.Prefab == null || piecePrefabsByType.ContainsKey(entry.Type))
                throw new ArgumentException("Road prefab entries must be unique and assigned.");

            piecePrefabsByType.Add(entry.Type, entry.Prefab);
        }

        return piecePrefabsByType;
    }

    private void OnFinishCrossed(Racer racer)
    {
        if (racer == _racer && _race.isActiveAndEnabled && _race.State == RaceState.Race)
            racer.FinishRace();
    }

    private void PlaceFinishLine()
    {
        Transform finishPoint = _roadPath.FinishPoint;
        _finishLine.gameObject.SetActive(finishPoint != null);

        if (finishPoint != null)
            _finishLine.transform.SetPositionAndRotation(finishPoint.position, finishPoint.rotation);
    }

    private void PrepareRace()
    {
        _racer.Prepare(_roadPath, _racerStartPosition, _racerStartRotation);
        _race.Prepare();
    }
}
