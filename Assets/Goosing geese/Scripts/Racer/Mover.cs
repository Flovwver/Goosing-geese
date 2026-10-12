using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Mover : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Vector3 _desiredVelocity;
    private float _maxAcceleration;
    private bool _hasMovementTarget;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void MoveTowards(Vector3 targetPosition, float allowedSpeed, float maxAcceleration)
    {
        Vector3 offsetToTarget = targetPosition - _rigidbody.position;
        offsetToTarget.y = 0f;
        _desiredVelocity = offsetToTarget.normalized * Mathf.Max(0f, allowedSpeed);
        _maxAcceleration = Mathf.Max(0f, maxAcceleration);
        _hasMovementTarget = true;
    }

    private void FixedUpdate()
    {
        if (_hasMovementTarget == false)
            return;

        if (_desiredVelocity.sqrMagnitude > Mathf.Epsilon)
            _rigidbody.MoveRotation(Quaternion.LookRotation(_desiredVelocity.normalized, Vector3.up));

        Vector3 planarVelocity = _rigidbody.linearVelocity;
        planarVelocity.y = 0f;
        Vector3 acceleration = (_desiredVelocity - planarVelocity) / Time.fixedDeltaTime;
        acceleration = Vector3.ClampMagnitude(acceleration, _maxAcceleration);
        _rigidbody.AddForce(acceleration, ForceMode.Acceleration);
    }

    public void Stop()
    {
        _desiredVelocity = Vector3.zero;
        _hasMovementTarget = false;
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

}
