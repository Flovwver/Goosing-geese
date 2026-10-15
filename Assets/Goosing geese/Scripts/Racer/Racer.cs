using System;
using UnityEngine;

[RequireComponent(typeof(OverheatTracker))]
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(RoadNavigator))]
public class Racer : MonoBehaviour
{
    private readonly GripModel _gripModel = new GripModel();

    [SerializeField] private RacerStats _stats;

    private OverheatTracker _overheatTracker;
    private Mover _mover;
    private RoadNavigator _roadNavigator;
    private RacerConfiguration _configuration;

    public event Action<float> OverheatProgressChanged;
    public event Action Exploded;
    public event Action PathCompleted;
    public event Action RaceRestarted;
    public event Action Finished;

    public bool IsFinished { get; private set; }
    public bool CanRace => _overheatTracker.IsExploded == false && _roadNavigator.IsPathCompleted == false;
    public RacerConfiguration Configuration => _configuration;

    public float YellowZoneThreshold => _configuration?.YellowZoneThreshold ?? _stats.YellowZoneThreshold;
    public float RedZoneThreshold => _configuration?.RedZoneThreshold ?? _stats.RedZoneThreshold;

    public float NormalizedSpeed => _configuration != null && _configuration.MaxSpeed > 0f ? GetCurrentSpeed() / _configuration.MaxSpeed : 0f;

    private void Awake()
    {
        _overheatTracker = GetComponent<OverheatTracker>();
        _mover = GetComponent<Mover>();
        _configuration = new RacerConfiguration(_stats);
        _mover.Configure(_configuration);
        _roadNavigator = GetComponent<RoadNavigator>();
    }

    private void OnEnable()
    {
        _overheatTracker.ProgressChanged += OnOverheatProgressChanged;
        _overheatTracker.Exploded += OnExploded;
        _roadNavigator.PathCompleted += OnPathCompleted;
    }

    private void OnDisable()
    {
        _overheatTracker.ProgressChanged -= OnOverheatProgressChanged;
        _overheatTracker.Exploded -= OnExploded;
        _roadNavigator.PathCompleted -= OnPathCompleted;
    }

    public float GetCurrentSpeed()
    {
        return _mover.GetCurrentSpeed();
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
        return new RacerDefinition(new RacerConfiguration(_stats), _roadNavigator.CaptureConfiguration(), position, rotation);
    }

    public void StopMovement()
    {
        _mover.Stop();
    }

    public void FinishRace()
    {
        if (IsFinished || _overheatTracker.IsExploded || _roadNavigator.IsPathCompleted)
            return;

        IsFinished = true;
        _overheatTracker.ResetOverheat();
        _mover.SetDrag(_configuration.FinishDrag);
        _mover.MoveTowards(_roadNavigator.TargetPosition, false, 0f);
        Finished?.Invoke();
    }

    public void Tick(bool isAccelerate, float deltaTime)
    {
        if (_overheatTracker.IsExploded || _roadNavigator.IsPathCompleted)
            return;

        if (IsFinished == false)
        {
            _overheatTracker.Tick(GetCurrentSpeed(), _configuration, deltaTime);

            if (_overheatTracker.IsExploded)
                return;
        }

        _roadNavigator.UpdateTarget(transform.position);

        if (_roadNavigator.IsPathCompleted)
            return;

        float slipFactor = IsFinished ? 0f : _gripModel.CalculateSlipFactor(GetCurrentSpeed(), _configuration);
        _mover.MoveTowards(_roadNavigator.TargetPosition, isAccelerate && IsFinished == false, slipFactor);
    }

    private void OnPathCompleted()
    {
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
