using System;
using UnityEngine;

[RequireComponent(typeof(OverheatTracker))]
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(RoadNavigator))]
public class Racer : MonoBehaviour
{
    [SerializeField] private RacerStats _stats;

    private GripModel _gripModel;
    private OverheatTracker _overheatTracker;
    private Mover _mover;
    private RoadNavigator _roadNavigator;
    private RacerConfiguration _configuration;

    public event Action<float> OverheatProgressChanged;
    public event Action Exploded;
    public event Action PathCompleted;
    public event Action RaceRestarted;
    public event Action Finished;
    public event Action ConfigurationChanged;

    public bool IsFinished { get; private set; }
    public bool CanRace => IsFinished == false && isActiveAndEnabled && _overheatTracker.IsExploded == false && _roadNavigator.IsPathCompleted == false;
    public RacerConfiguration Configuration => _configuration;
    public RacerStats BaseStats => _stats;
    public float MaxSpeed => _configuration?.MaxSpeed ?? _stats.MaxSpeed;
    public float YellowZoneThreshold => _configuration?.YellowZoneThreshold ?? _stats.YellowZoneThreshold;
    public float RedZoneThreshold => _configuration?.RedZoneThreshold ?? _stats.RedZoneThreshold;
    public float NormalizedSpeed => MaxSpeed > 0f ? GetCurrentSpeed() / MaxSpeed : 0f;

    private void Awake()
    {
        InitializeComponents();
    }

    private void OnEnable()
    {
        InitializeComponents();
        _overheatTracker.ProgressChanged += OnOverheatProgressChanged;
        _overheatTracker.Exploded += OnExploded;
        _roadNavigator.PathCompleted += OnPathCompleted;
    }

    private void OnDisable()
    {
        _mover.Stop();
        _overheatTracker.ProgressChanged -= OnOverheatProgressChanged;
        _overheatTracker.Exploded -= OnExploded;
        _roadNavigator.PathCompleted -= OnPathCompleted;
    }

    public float GetCurrentSpeed()
    {
        return _mover.GetCurrentSpeed();
    }

    public void ConfigureStats(RacerStats stats)
    {
        _configuration = new RacerConfiguration(stats);
        _mover.Configure(_configuration);
        ConfigurationChanged?.Invoke();
    }

    public void Prepare(RoadPath path, RacerDefinition definition)
    {
        IsFinished = false;
        _configuration = definition.Stats;
        _mover.Configure(_configuration);
        _roadNavigator.Configure(definition.Navigation);
        _mover.TeleportTo(definition.Position, definition.Rotation);
        _roadNavigator.SetPath(path);
        _overheatTracker.ResetOverheat();
        RaceRestarted?.Invoke();
    }

    public RacerDefinition CaptureDefinition(Vector3 position, Quaternion rotation)
    {
        return new RacerDefinition(_configuration, _roadNavigator.CaptureConfiguration(), position, rotation);
    }

    public void StopMovement()
    {
        _mover.Stop();
    }

    public void ReleaseThrottle()
    {
        _mover?.ReleaseThrottle();
    }

    public void Accelerate()
    {
        Tick(true, Time.deltaTime);
    }

    public void Cooldown()
    {
        Tick(false, Time.deltaTime);
    }

    public void FinishRace()
    {
        if (IsFinished || _overheatTracker.IsExploded || (_roadNavigator.IsPathCompleted && _roadNavigator.IsFinishReached == false))
            return;

        IsFinished = true;
        _overheatTracker.ResetOverheat();
        _mover.SetDrag(_configuration.FinishDrag);
        _mover.MoveTowards(_roadNavigator.TargetPosition, false, 0f);
        Finished?.Invoke();
    }

    public void Tick(bool isAccelerate, float deltaTime)
    {
        if (isActiveAndEnabled == false || _overheatTracker.IsExploded || _roadNavigator.IsPathCompleted)
            return;

        if (IsFinished == false)
        {
            _overheatTracker.Tick(GetCurrentSpeed(), _configuration, deltaTime);

            if (_overheatTracker.IsExploded)
                return;
        }

        _roadNavigator.UpdateTarget(transform.position);

        if (IsFinished == false && _roadNavigator.IsFinishReached)
            FinishRace();

        if (_roadNavigator.IsPathCompleted)
            return;

        float slip = IsFinished ? 0f : _gripModel.CalculateSlipFactor(GetCurrentSpeed(), _configuration);
        _mover.MoveTowards(_roadNavigator.TargetPosition, isAccelerate && IsFinished == false, slip);
    }

    private void InitializeComponents()
    {
        _overheatTracker = GetComponent<OverheatTracker>();
        _mover = GetComponent<Mover>();
        _roadNavigator = GetComponent<RoadNavigator>();

        if (_gripModel == null)
            _gripModel = new GripModel();

        if (_configuration == null)
            _configuration = new RacerConfiguration(_stats);

        _mover.Configure(_configuration);
    }

    private void OnPathCompleted()
    {
        if (IsFinished == false && _roadNavigator.IsFinishReached)
            FinishRace();

        _mover.Stop();
        PathCompleted?.Invoke();
    }

    private void OnOverheatProgressChanged(float progress)
    {
        OverheatProgressChanged?.Invoke(progress);
    }

    private void OnExploded()
    {
        _mover.Stop();
        Exploded?.Invoke();
    }
}
