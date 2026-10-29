using System;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    private readonly EventPublisher _events = new EventPublisher();

    [SerializeField] private Transform _roadStartPoint;
    [SerializeField] private RoadSettings _settings;
    [SerializeField] private Racer _racer;
    [SerializeField] private FinishLine _finishLine;
    private GameSession _session;
    private OpponentSpawner _opponents;
    private string _opponentSnapshot;
    [SerializeField, Min(0f)] private float _minimumRoadSpacing = 24f;
    [SerializeField, Min(1)] private int _layoutValidationAttempts = 20;

    private RoadBuilder _roadBuilder;
    private RoadPath _roadPath;
    private List<RoadPiece> _builtPieces;
    private RoadPieceType[] _currentLayout;
    private Vector3 _racerStartPosition;
    private Quaternion _racerStartRotation;
    private Vector3 _roadStartPosition;
    private Quaternion _roadStartRotation;
    private int _mainPieceCount;
    private string _roadSnapshot;
    private string _racerSnapshot;
    private string _navigationSnapshot;
    private bool _restoreAfterReload;

    public event Action<RoadPath, RoadConfiguration> PathBuilt;

    public LevelDefinition CurrentDefinition { get; private set; }
    public RoadSpeedProfile SpeedProfile { get; private set; }
    public int Seed { get; private set; }
    public int Number { get; private set; }

    private void Awake()
    {
        _racerStartPosition = _racer.transform.position;
        _racerStartRotation = _racer.transform.rotation;
        InitializeReferences();
    }

    private void OnEnable()
    {
        InitializeReferences();
        _finishLine.Crossed += OnFinishCrossed;
        _racer.ConfigurationChanged += OnRacerConfigurationChanged;
        _restoreAfterReload = CurrentDefinition == null && _builtPieces.Count > 0;
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
        RestoreAfterReload();
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
        _racer.ConfigurationChanged -= OnRacerConfigurationChanged;
    }

    private void OnDestroy()
    {
        ClearBuiltRoad();
    }

    public void PrepareOpponent(Racer opponent, RoadPath path)
    {
        if (CurrentDefinition.Opponent != null)
            opponent.Prepare(path, CurrentDefinition.Opponent.Racer);
    }

    public void SetSession(GameSession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    public void BuildRoad()
    {
        if (TryBuildRoad(out string error) == false)
            Debug.LogError("Level creation failed: " + error, this);
    }

    public bool TryBuildRoad(out string error)
    {
        return TryBuildRoad(_session.CurrentStage.Seed, out error);
    }

    public void CreateLevel(int seed)
    {
        if (TryBuildRoad(seed, out string error) == false)
            Debug.LogError("Level creation failed: " + error, this);
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

            if (_racer == null || _finishLine == null || _session == null)
                throw new InvalidOperationException("Level requires a racer, finish line and session.");

            RoadConfiguration settings = definition.Settings;
            builder = new RoadBuilder(settings.PiecePrefabs);
            path = builder.Build(definition.Layout, definition.StartPosition, definition.StartRotation);
            path.SetFinishWaypoint(settings.RoadPieceCount);
            path.BuildCurve(settings.CurveSamplesPerSegment);

            if (path.FinishPoint == null || path.Curve.Length <= 0f || float.IsNaN(path.Curve.Length) || float.IsInfinity(path.Curve.Length))
                throw new InvalidOperationException("The built road has invalid geometry or no finish.");

            _racer.ValidatePreparation(path, definition.Racer);
            _session.ValidatePreparation();
            profile = new RoadSpeedProfile(path.Curve, definition.Racer.Stats);
        }
        catch (Exception exception)
        {
            builder?.Clear();
            error = exception.Message;
            return false;
        }

        ClearBuiltRoad();
        CurrentDefinition = definition;
        Seed = definition.Seed;
        Number = definition.Number;
        _mainPieceCount = definition.Settings.RoadPieceCount;
        _roadStartPosition = definition.StartPosition;
        _roadStartRotation = definition.StartRotation;
        _racerStartPosition = definition.Racer.Position;
        _racerStartRotation = definition.Racer.Rotation;
        _currentLayout = new RoadPieceType[definition.Layout.Count];

        for (int index = 0; index < _currentLayout.Length; index++)
            _currentLayout[index] = definition.Layout[index];

        _roadBuilder = builder;
        _roadPath = path;
        _builtPieces = new List<RoadPiece>(path.Pieces);
        SpeedProfile = profile;
        CaptureReloadSnapshot();
        builder.Activate();
        PlaceFinishLine();
        PrepareRace();
        _events.Publish(PathBuilt, path, definition.Settings, this);
        error = null;
        return true;
    }

    public void RestartRace()
    {
        if (_roadPath != null)
            PrepareRace();
    }

    public bool TryGetBuiltPath(out RoadPath path, out RoadConfiguration settings)
    {
        path = _roadPath;
        settings = CurrentDefinition?.Settings;
        return path != null && path.Curve != null;
    }

    private bool TryBuildRoad(int seed, out string error)
    {
        LevelDefinition definition;

        try
        {
            if (_roadStartPoint == null)
                throw new InvalidOperationException("A road start point must be assigned.");

            CampaignStage stage = _session.CurrentStage;
            var settings = new RoadConfiguration(_settings, stage.PieceCount);
            RoadPieceType[] layout = GenerateLayout(seed, stage, settings);
            RacerDefinition racer = _racer.CaptureDefinition(_racerStartPosition, _racerStartRotation);
            definition = new LevelDefinition(seed, layout, settings, _roadStartPoint.position, _roadStartPoint.rotation, racer, _session.Progress.Level, CaptureOpponent(stage, seed, racer));
        }
        catch (Exception exception)
        {
            error = exception.Message;
            return false;
        }

        return TryRestoreLevel(definition, out error);
    }

    private OpponentDefinition CaptureOpponent(CampaignStage stage, int seed, RacerDefinition player)
    {
        if (_opponents == null)
            return null;

        Vector3 position = player.Position + player.Rotation * Vector3.right * _opponents.LaneOffset;
        return _opponents.CaptureDefinition(stage, seed, position, player.Rotation);
    }

    private RoadPieceType[] GenerateLayout(int seed, CampaignStage stage, RoadConfiguration settings)
    {
        var prefabs = new Dictionary<RoadPieceType, RoadPiece>();

        foreach (KeyValuePair<RoadPieceType, RoadPiece> pair in settings.PiecePrefabs)
            prefabs.Add(pair.Key, pair.Value);

        var geometry = new RoadLayoutGeometry(prefabs);
        int count = settings.RoadPieceCount + settings.FinishRunoutPieces;

        if (seed == stage.Seed && stage.Layout != null && stage.Layout.Length == count
            && geometry.IsValid(stage.Layout, _roadStartPoint, _minimumRoadSpacing, settings.CurveSamplesPerSegment))
            return (RoadPieceType[])stage.Layout.Clone();

        var generator = new RoadLayoutGenerator(settings.AvailablePieceTypes, settings.MaxGenerationAttempts, seed);

        for (int attempt = 0; attempt < Mathf.Max(1, _layoutValidationAttempts); attempt++)
        {
            RoadPieceType[] candidate = generator.Generate(settings.RoadPieceCount, settings.FinishRunoutPieces);

            if (geometry.IsValid(candidate, _roadStartPoint, _minimumRoadSpacing, settings.CurveSamplesPerSegment))
                return candidate;
        }

        var fallback = new RoadPieceType[count];

        for (int index = 0; index < fallback.Length; index++)
            fallback[index] = RoadPieceType.Straight;

        return fallback;
    }

    private void InitializeReferences()
    {
        _opponents = GetComponent<OpponentSpawner>();

        if (_builtPieces == null)
            _builtPieces = new List<RoadPiece>();

        if (_finishLine == null || _racer == null)
            throw new InvalidOperationException("Assign the level racer and finish line in the Inspector.");
    }

    private void ClearBuiltRoad()
    {
        if (_roadBuilder != null)
        {
            _roadBuilder.Clear();
        }
        else if (_builtPieces != null && _builtPieces.Count > 0 && _builtPieces[0] != null)
        {
            GameObject root = _builtPieces[0].transform.parent.gameObject;
            root.SetActive(false);

            if (Application.isPlaying)
                Destroy(root);
            else
                DestroyImmediate(root);
        }

        _builtPieces?.Clear();
    }

    private void OnFinishCrossed(Racer racer)
    {
        if (_session.IsRacing)
            racer.FinishRace();
    }

    private void PlaceFinishLine()
    {
        Transform point = _roadPath.FinishPoint;
        _finishLine.gameObject.SetActive(point != null);

        if (point != null)
            _finishLine.transform.SetPositionAndRotation(point.position, point.rotation);
    }

    private void PrepareRace()
    {
        _racer.Prepare(_roadPath, CurrentDefinition.Racer);
        _session.PrepareAttempt();
    }

    private void OnRacerConfigurationChanged()
    {
        if (CurrentDefinition == null || _session.IsWaiting == false)
            return;

        RacerDefinition racer = _racer.CaptureDefinition(_racerStartPosition, _racerStartRotation);
        CurrentDefinition = new LevelDefinition(Seed, _currentLayout, CurrentDefinition.Settings, _roadStartPosition, _roadStartRotation, racer, Number, CurrentDefinition.Opponent);
        SpeedProfile = new RoadSpeedProfile(_roadPath.Curve, racer.Stats);
        CaptureReloadSnapshot();
    }

    private void CaptureReloadSnapshot()
    {
        _opponentSnapshot = CurrentDefinition.Opponent == null ? null : JsonUtility.ToJson(new OpponentSnapshot(CurrentDefinition.Opponent));
        _roadSnapshot = CurrentDefinition.Settings.ToJson();
        _racerSnapshot = CurrentDefinition.Racer.Stats.ToJson();
        _navigationSnapshot = CurrentDefinition.Racer.Navigation.ToJson();
    }

    private void RestoreAfterReload()
    {
        var road = ScriptableObject.CreateInstance<RoadSettings>();
        var racer = ScriptableObject.CreateInstance<RacerStats>();
        var navigation = ScriptableObject.CreateInstance<NavigationSettings>();

        try
        {
            JsonUtility.FromJsonOverwrite(_roadSnapshot, road);
            JsonUtility.FromJsonOverwrite(_racerSnapshot, racer);
            JsonUtility.FromJsonOverwrite(_navigationSnapshot, navigation);
            var settings = new RoadConfiguration(road, _mainPieceCount);
            var start = new RacerDefinition(new RacerConfiguration(racer), new NavigationConfiguration(navigation), _racerStartPosition, _racerStartRotation);
            var definition = new LevelDefinition(Seed, _currentLayout, settings, _roadStartPosition, _roadStartRotation, start, Number, string.IsNullOrEmpty(_opponentSnapshot) ? null : JsonUtility.FromJson<OpponentSnapshot>(_opponentSnapshot).Restore());

            if (TryRestoreLevel(definition, out string error) == false)
                Debug.LogError("Level reload recovery failed: " + error, this);
        }
        finally
        {
            Destroy(road);
            Destroy(racer);
            Destroy(navigation);
        }
    }
}
