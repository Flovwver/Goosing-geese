using UnityEngine;

public class Steering
{
    private const float MinimumTargetDirectionSqrMagnitude = 0.0001f;

    public Vector3 LocalWheelDirection { get; private set; } = Vector3.forward;
    public float AngleLimit { get; private set; }

    public void UpdateDirection(Vector3 position, Quaternion bodyRotation, Vector3 targetPosition, float maxSteeringAngle)
    {
        AngleLimit = Mathf.Clamp(maxSteeringAngle, 0f, RacerStats.MaximumAllowedSteeringAngle);
        Vector3 direction = targetPosition - position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= MinimumTargetDirectionSqrMagnitude)
        {
            Reset();
            return;
        }

        Vector3 forward = bodyRotation * Vector3.forward;
        forward.y = 0f;
        float steeringAngle = Vector3.SignedAngle(forward, direction, Vector3.up);
        steeringAngle = Mathf.Clamp(steeringAngle, -AngleLimit, AngleLimit);
        LocalWheelDirection = Quaternion.AngleAxis(steeringAngle, Vector3.up) * Vector3.forward;
    }

    public Vector3 GetMovementDirection(Quaternion bodyRotation)
    {
        Vector3 direction = bodyRotation * LocalWheelDirection;
        direction.y = 0f;
        return direction.normalized;
    }

    public void Reset()
    {
        LocalWheelDirection = Vector3.forward;
    }
}
