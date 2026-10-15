using System;
using UnityEngine;

[RequireComponent(typeof(OverheatTracker))]
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(RoadNavigator))]
public class Racer : MonoBehaviour
{
    [SerializeField] private RacerStats _stats;

    private OverheatTracker _overheatTracker;
    private Mover _mover;
    private RoadNavigator _roadNavigator;

    public event Action<float> OverheatProgressChanged;
    public event Action Exploded;
    public event Action PathCompleted;
    public event Action RaceRestarted;
    public event Action Finished;

    public bool IsFinished { get; private set; }
    public bool CanRace => _overheatTracker.IsExploded == false && _roadNavigator.IsPathCompleted == false;

    public float YellowZoneThreshold => _stats.YellowZoneThreshold;
    public float RedZoneThreshold => _stats.RedZoneThreshold;

    public float NormalizedSpeed => _stats.MaxSpeed > 0f ? GetCurrentSpeed() / _stats.MaxSpeed : 0f;

    private void Awake()
    {
        _overheatTracker = GetComponent<OverheatTracker>();
        _mover = GetComponent<Mover>();
        _mover.Configure(_stats);
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

    public void Prepare(RoadPath path, Vector3 position, Quaternion rotation)
    {
        IsFinished = false;
        _mover.TeleportTo(position, rotation);
        _roadNavigator.SetPath(path);
        _overheatTracker.ResetOverheat();
        RaceRestarted?.Invoke();
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
        _mover.SetDrag(_stats.FinishDrag);
        _mover.MoveTowards(_roadNavigator.TargetPosition, false, 0f);
        Finished?.Invoke();
    }

    public void Tick(bool isAccelerate)
    {
        if (_overheatTracker.IsExploded || _roadNavigator.IsPathCompleted)
            return;

        if (IsFinished == false)
        {
            _overheatTracker.Tick(GetCurrentSpeed(), _stats);

            if (_overheatTracker.IsExploded)
                return;
        }

        _roadNavigator.UpdateTarget(transform.position);

        if (_roadNavigator.IsPathCompleted)
            return;

        _mover.MoveTowards(_roadNavigator.TargetPosition, isAccelerate && IsFinished == false, _overheatTracker.SlipFactor);
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
