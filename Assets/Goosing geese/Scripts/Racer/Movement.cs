using UnityEngine;

public class Movement
{
    private const float MaximumHeadingAngleDegrees = 180f;

    private readonly Rigidbody _rigidbody;
    private float _slipFactor;

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

    public void Apply(Vector3 direction, float currentSpeed, bool isAccelerating, float slipFactor, RacerStats stats, float deltaTime)
    {
        _slipFactor = Mathf.MoveTowards(_slipFactor, Mathf.Clamp01(slipFactor),
            Mathf.Max(0f, stats.GripTransitionSpeed) * deltaTime);
        float alignmentStep = Mathf.Lerp(MaximumHeadingAngleDegrees,
            Mathf.Max(0f, stats.DriftAlignmentSpeed) * deltaTime, _slipFactor);
        AlignVelocity(direction, currentSpeed, alignmentStep);

        if (isAccelerating && currentSpeed < stats.MaxSpeed)
            _rigidbody.AddForce(direction * Mathf.Max(0f, stats.Acceleration), ForceMode.Acceleration);
    }

    public void Stop()
    {
        _slipFactor = 0f;
        _rigidbody.linearVelocity = Vector3.zero;
    }

    private void AlignVelocity(Vector3 direction, float currentSpeed, float alignmentStep)
    {
        Vector3 velocity = _rigidbody.linearVelocity;
        Vector3 alignedVelocity = direction * currentSpeed;

        if (currentSpeed > 0f)
        {
            Vector3 currentDirection = new Vector3(velocity.x, 0f, velocity.z) / currentSpeed;
            float angle = Vector3.SignedAngle(currentDirection, direction, Vector3.up);
            float turnAngle = Mathf.Clamp(angle, -alignmentStep, alignmentStep);
            alignedVelocity = Quaternion.AngleAxis(turnAngle, Vector3.up) * currentDirection * currentSpeed;
        }

        alignedVelocity.y = velocity.y;
        _rigidbody.linearVelocity = alignedVelocity;
    }
}
