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
        RoadSettings settings = Instantiate(_settings);
        settings.hideFlags = HideFlags.HideAndDontSave;
        int seed = Guid.NewGuid().GetHashCode();
        int runoutPieces = Mathf.Max(1, settings.FinishRunoutPieces);
        var generator = new RoadLayoutGenerator(settings.AvailablePieceTypes, settings.MaxGenerationAttempts, seed);
        RoadPieceType[] layout = generator.Generate(settings.RoadPieceCount, runoutPieces);

        if (layout.Length == 0)
        {
            Destroy(settings);
            return;
        }

        _roadBuilder?.Clear();

        if (CurrentDefinition != null)
            Destroy(CurrentDefinition.Settings);

        CurrentDefinition = new LevelDefinition(seed, layout, settings, _roadStartPoint.position, _roadStartPoint.rotation);
        _roadBuilder = new RoadBuilder(BuildPiecePrefabsByType(settings));
        _roadPath = _roadBuilder.Build(CurrentDefinition.Layout, CurrentDefinition.StartPosition, CurrentDefinition.StartRotation);
        _roadPath.SetFinishWaypoint(layout.Length - runoutPieces);
        _roadPath.BuildCurve(settings.CurveSamplesPerSegment);
        PlaceFinishLine();
        PrepareRace();
        PathBuilt?.Invoke(_roadPath, settings);
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

        foreach (RoadSettings.PiecePrefabEntry entry in settings.PiecePrefabs)
        {
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
