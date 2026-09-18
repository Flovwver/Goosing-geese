using UnityEngine;

[RequireComponent(typeof(Heat))]
[RequireComponent(typeof(Mover))]
public class Racer : MonoBehaviour
{
    [SerializeField] private RacerStats _stats;
    
    private Heat _heat;
    private Mover _mover;

    private float _currentSpeed;

    public float CurrentSpeed => _currentSpeed;

    private void Awake()
    {
        _heat = GetComponent<Heat>();
        _mover = GetComponent<Mover>();
    }

    public void Accelerate()
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, _stats.MaxSpeed, _stats.Acceleration * Time.deltaTime);
        _heat.Tick(_currentSpeed, _stats);
        _mover.Move(_currentSpeed);
    }

    public void Cooldown()
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, _stats.Acceleration * Time.deltaTime);
        _heat.Tick(_currentSpeed, _stats);
        _mover.Move(_currentSpeed);
    }
}