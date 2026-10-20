using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Mover : MonoBehaviour
{
    private Steering _steering;
    private Rigidbody _rigidbody;
    private Movement _movement;
    private BodyRotation _bodyRotation;
    private RacerStats _stats;
    private float? _dragOverride;
    private Vector3 _targetPosition;
    private float _slipFactor;
    private bool _hasMovementTarget;
    private bool _isAccelerating;

    private void Awake()
    {
        InitializeComponents();
    }

    private void OnEnable()
    {
        InitializeComponents();
    }

    private void FixedUpdate()
    {
        if (_stats == null)
            return;

        float drag = _dragOverride ?? (_isAccelerating ? _stats.Drag : _stats.CoastingDrag);
        _movement.ApplyDrag(drag);

        if (_hasMovementTarget == false)
            return;

        _steering.UpdateDirection(_rigidbody.position, _rigidbody.rotation, _targetPosition, _stats.MaxSteeringAngle);
        Vector3 movementDirection = _steering.GetMovementDirection(_rigidbody.rotation);
        float currentSpeed = _movement.GetCurrentSpeed();
        _movement.Apply(movementDirection, currentSpeed, _isAccelerating, _slipFactor, _stats, Time.fixedDeltaTime);
        _bodyRotation.Apply(movementDirection, _steering.LocalWheelDirection, _steering.AngleLimit, currentSpeed, _stats, Time.fixedDeltaTime);
    }

    public float GetCurrentSpeed()
    {
        InitializeComponents();
        return _movement.GetCurrentSpeed();
    }

    public void Configure(RacerStats stats)
    {
        InitializeComponents();
        _stats = stats;
    }

    public void MoveTowards(Vector3 targetPosition, bool isAccelerating, float slipFactor)
    {
        _slipFactor = Mathf.Clamp01(slipFactor);
        _targetPosition = targetPosition;
        _hasMovementTarget = true;
        _isAccelerating = isAccelerating;
    }

    public void SetDrag(float drag)
    {
        _dragOverride = Mathf.Max(0f, drag);
    }

    public void Stop()
    {
        _dragOverride = null;
        _slipFactor = 0f;
        _steering?.Reset();
        _hasMovementTarget = false;
        _isAccelerating = false;
        _movement?.Stop();
        _bodyRotation?.Stop();
    }

    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        Stop();
        _rigidbody.position = position;
        _rigidbody.rotation = rotation;
        transform.SetPositionAndRotation(position, rotation);
    }

    private void InitializeComponents()
    {
        if (_movement != null)
            return;

        _rigidbody = GetComponent<Rigidbody>();
        _steering = new Steering();
        _movement = new Movement(_rigidbody);
        _bodyRotation = new BodyRotation(_rigidbody);
    }
}
