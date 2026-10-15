using System;
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

    public event Action<RoadPath, RoadConfiguration> PathBuilt;

    public LevelDefinition CurrentDefinition { get; private set; }
    public RoadSpeedProfile SpeedProfile { get; private set; }

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
        RoadConfiguration settings = CurrentDefinition?.Settings;

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
    }

    public void BuildRoad()
    {
        if (TryBuildRoad(out string error) == false)
            Debug.LogError($"Level creation failed: {error}", this);
    }

    public bool TryBuildRoad(out string error)
    {
        LevelDefinition definition;

        try
        {
            if (_roadStartPoint == null)
                throw new InvalidOperationException("A road start point must be assigned.");

            var settings = new RoadConfiguration(_settings);
            int seed = Guid.NewGuid().GetHashCode();
            var generator = new RoadLayoutGenerator(settings.AvailablePieceTypes, settings.MaxGenerationAttempts, seed);
            RoadPieceType[] layout = generator.Generate(settings.RoadPieceCount, settings.FinishRunoutPieces);
            RacerDefinition racer = _racer.CaptureDefinition(_racerStartPosition, _racerStartRotation);
            definition = new LevelDefinition(seed, layout, settings, _roadStartPoint.position, _roadStartPoint.rotation, racer);
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }

        return TryRestoreLevel(definition, out error);
    }

    public bool TryRestoreLevel(LevelDefinition definition, out string error)
    {
        RoadBuilder builder = null;
        RoadPath path;
        RoadSpeedProfile profile;

        try
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            RoadConfiguration settings = definition.Settings;
            builder = new RoadBuilder(settings.PiecePrefabs);
            path = builder.Build(definition.Layout, definition.StartPosition, definition.StartRotation);
            path.SetFinishWaypoint(definition.Layout.Count - settings.FinishRunoutPieces);
            path.BuildCurve(settings.CurveSamplesPerSegment);

            if (path.FinishPoint == null || path.Curve.Length <= 0f || float.IsNaN(path.Curve.Length) || float.IsInfinity(path.Curve.Length))
                throw new InvalidOperationException("The built road has invalid geometry or no finish.");

            profile = new RoadSpeedProfile(path.Curve, definition.Racer.Stats);
        }
        catch (Exception exception)
        {
            builder?.Clear();
            error = exception.Message;
            return false;
        }

        _roadBuilder?.Clear();
        CurrentDefinition = definition;
        _roadBuilder = builder;
        _roadPath = path;
        SpeedProfile = profile;
        builder.Activate();
        PlaceFinishLine();
        PrepareRace();
        PathBuilt?.Invoke(_roadPath, definition.Settings);
        error = null;
        return true;
    }

    public void RestartRace()
    {
        if (_roadPath == null)
            return;

        PrepareRace();
    }

    public bool TryGetBuiltPath(out RoadPath path, out RoadConfiguration settings)
    {
        path = _roadPath;
        settings = CurrentDefinition?.Settings;
        return path != null && path.Curve != null;
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
        _racer.Prepare(_roadPath, CurrentDefinition.Racer);
        _race.Prepare();
    }
}
