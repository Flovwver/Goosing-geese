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

    private bool _isPathCompletedRaised;
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    public float NormalizedSpeed => _stats.MaxSpeed > 0f ? GetCurrentSpeed() / _stats.MaxSpeed : 0f;

    public event Action<float> OverheatProgressChanged;
    public event Action Exploded;
    public event Action PathCompleted;
    public event Action RaceRestarted;

    private void Awake()
    {
        _overheatTracker = GetComponent<OverheatTracker>();
        _mover = GetComponent<Mover>();
        _mover.Configure(_stats);
        _roadNavigator = GetComponent<RoadNavigator>();
        _startPosition = transform.position;
        _startRotation = transform.rotation;
    }

    private void OnEnable()
    {
        _overheatTracker.ProgressChanged += OnOverheatProgressChanged;
        _overheatTracker.Exploded += OnExploded;
    }

    private void OnDisable()
    {
        _overheatTracker.ProgressChanged -= OnOverheatProgressChanged;
        _overheatTracker.Exploded -= OnExploded;
    }

    public float GetCurrentSpeed()
    {
        return _mover.GetCurrentSpeed();
    }

    public void SetPath(RoadPath path)
    {
        _isPathCompletedRaised = false;
        _roadNavigator.SetPath(path);
    }

    public void Accelerate()
    {
        Advance(true);
    }

    public void ResetRace()
    {
        _isPathCompletedRaised = false;
        _mover.TeleportTo(_startPosition, _startRotation);
        _overheatTracker.ResetOverheat();
        RaceRestarted?.Invoke();
    }

    public void Cooldown()
    {
        Advance(false);
    }

    private void Advance(bool isAccelerating)
    {
        if (_overheatTracker.IsExploded || _isPathCompletedRaised)
            return;

        _overheatTracker.Tick(GetCurrentSpeed(), _stats);

        if (_overheatTracker.IsExploded)
            return;

        _roadNavigator.UpdateTarget(transform.position);

        if (_roadNavigator.IsPathCompleted)
        {
            RaisePathCompleted();
            return;
        }

        _mover.MoveTowards(_roadNavigator.TargetPosition, isAccelerating);
    }

    private void RaisePathCompleted()
    {
        if (_isPathCompletedRaised)
            return;

        _isPathCompletedRaised = true;
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
