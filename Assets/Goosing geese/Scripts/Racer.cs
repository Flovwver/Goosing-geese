using UnityEngine;

public class Racer : MonoBehaviour
{
    [SerializeField] private RacerStats _stats;
    [SerializeField] private Overheat _overheat;

    private float _currentSpeed;

    public float CurrentSpeed => _currentSpeed;

    public void Accelerate()
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, _stats.MaxSpeed, _stats.Acceleration * Time.deltaTime);
        _overheat.Tick(_currentSpeed, _stats);
    }

    public void Cooldown()
    {
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, _stats.Acceleration * Time.deltaTime);
        _overheat.Tick(_currentSpeed, _stats);
    }
}
