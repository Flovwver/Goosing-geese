using UnityEngine;

public class BodyRotation
{
    private const float MinimumSteeringAngleDegrees = 0.001f;
    private readonly Rigidbody _rigidbody;

    public BodyRotation(Rigidbody rigidbody)
    {
        _rigidbody = rigidbody;
    }

    public void Apply(Vector3 direction, Vector3 localWheelDirection, float steeringLimit, float currentSpeed, RacerConfiguration stats, float deltaTime)
    {
        float steeringAngle = Vector3.Angle(Vector3.forward, localWheelDirection);
        float speedFactor = stats.MaxSpeed > 0f
            ? Mathf.Clamp01(currentSpeed / stats.MaxSpeed)
            : 0f;

        if (speedFactor <= 0f || steeringLimit <= 0f || steeringAngle <= MinimumSteeringAngleDegrees)
        {
            Stop();
            return;
        }

        float steeringFactor = steeringAngle / steeringLimit;
        float turnStep = Mathf.Max(0f, stats.TurnSpeed) * speedFactor * steeringFactor * deltaTime;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        Quaternion rotation = Quaternion.RotateTowards(_rigidbody.rotation, targetRotation, turnStep);
        _rigidbody.MoveRotation(rotation);
    }

    public void Stop()
    {
        _rigidbody.angularVelocity = Vector3.zero;
    }
}
