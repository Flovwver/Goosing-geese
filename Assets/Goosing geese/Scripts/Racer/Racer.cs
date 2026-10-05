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

    private float _currentSpeed;
    private bool _isPathCompletedRaised;

    public float CurrentSpeed => _currentSpeed;
    public float NormalizedSpeed => _currentSpeed / _stats.MaxSpeed;

    public event Action<float> OverheatProgressChanged;
    public event Action Exploded;
    public event Action PathCompleted;

    private void Awake()
    {
        _overheatTracker = GetComponent<OverheatTracker>();
        _mover = GetComponent<Mover>();
        _roadNavigator = GetComponent<RoadNavigator>();
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

    public void SetPath(RoadPath path)
    {
        _isPathCompletedRaised = false;
        _roadNavigator.SetPath(path);
    }

    public void Accelerate()
    {
        Advance(_stats.MaxSpeed);
    }

    public void Cooldown()
    {
        Advance(0f);
    }

    private void Advance(float targetSpeed)
    {
        if (_overheatTracker.IsExploded)
            return;

        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, _stats.Acceleration * Time.deltaTime);
        _overheatTracker.Tick(_currentSpeed, _stats);

        if (_overheatTracker.IsExploded)
            return;

        _roadNavigator.UpdateTarget(transform.position);

        if (_roadNavigator.IsPathCompleted)
        {
            RaisePathCompleted();
            return;
        }

        _mover.MoveTowards(_roadNavigator.TargetPosition, _currentSpeed, _stats);
    }

    private void RaisePathCompleted()
    {
        if (_isPathCompletedRaised)
            return;

        _isPathCompletedRaised = true;
        PathCompleted?.Invoke();
    }

    private void OnOverheatProgressChanged(float progress)
    {
        OverheatProgressChanged?.Invoke(progress);
    }

    private void OnExploded()
    {
        _currentSpeed = 0f;
        _mover.Stop();
        Exploded?.Invoke();
    }
}
