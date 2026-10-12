using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Mover : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Vector3 _targetPosition;
    private RacerStats _stats;
    private bool _hasMovementTarget;
    private bool _isAccelerating;

    public float CurrentSpeed
    {
        get
        {
            Vector3 velocity = _rigidbody.linearVelocity;
            velocity.y = 0f;
            return velocity.magnitude;
        }
    }

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    public void Configure(RacerStats stats)
    {
        _stats = stats;
    }

    public void MoveTowards(Vector3 targetPosition, bool isAccelerating)
    {
        _targetPosition = targetPosition;
        _hasMovementTarget = true;
        _isAccelerating = isAccelerating;
    }

    public void Stop()
    {
        _hasMovementTarget = false;
        _isAccelerating = false;

        if (_rigidbody == null)
            return;

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        Stop();
        _rigidbody.position = position;
        _rigidbody.rotation = rotation;
        transform.SetPositionAndRotation(position, rotation);
    }

    private void ApplyMovement()
    {
        _rigidbody.linearDamping = Mathf.Max(0f, _stats.Drag);

        if (_hasMovementTarget == false)
            return;

        Vector3 forward = _rigidbody.rotation * Vector3.forward;
        ApplyRotation();

        if (_isAccelerating && CurrentSpeed < _stats.MaxSpeed)
            _rigidbody.AddForce(forward * Mathf.Max(0f, _stats.Acceleration), ForceMode.Acceleration);
    }

    private void ApplyRotation()
    {
        Vector3 direction = _targetPosition - _rigidbody.position;
        direction.y = 0f;

        if (_isAccelerating == false || direction.sqrMagnitude <= Mathf.Epsilon)
        {
            _rigidbody.angularVelocity = Vector3.zero;
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        Quaternion rotation = Quaternion.RotateTowards(_rigidbody.rotation, targetRotation, Mathf.Max(0f, _stats.TurnSpeed) * Time.fixedDeltaTime);
        _rigidbody.MoveRotation(rotation);
    }
}
