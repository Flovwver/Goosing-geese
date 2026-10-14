using UnityEngine;

public class Movement
{
    private readonly Rigidbody _rigidbody;

    public Movement(Rigidbody rigidbody)
    {
        _rigidbody = rigidbody;
    }

    public float GetCurrentSpeed()
    {
        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.y = 0f;
        return velocity.magnitude;
    }

    public void ApplyDrag(float drag)
    {
        _rigidbody.linearDamping = Mathf.Max(0f, drag);
    }

    public void Apply(Vector3 direction, float currentSpeed, bool isAccelerating, RacerStats stats)
    {
        AlignVelocity(direction, currentSpeed);

        if (isAccelerating && currentSpeed < stats.MaxSpeed)
            _rigidbody.AddForce(direction * Mathf.Max(0f, stats.Acceleration), ForceMode.Acceleration);
    }

    public void Stop()
    {
        _rigidbody.linearVelocity = Vector3.zero;
    }

    private void AlignVelocity(Vector3 direction, float currentSpeed)
    {
        Vector3 velocity = _rigidbody.linearVelocity;
        Vector3 alignedVelocity = direction * currentSpeed;

        alignedVelocity.y = velocity.y;
        _rigidbody.linearVelocity = alignedVelocity;
    }
}
