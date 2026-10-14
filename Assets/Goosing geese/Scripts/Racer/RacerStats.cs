using UnityEngine;

[CreateAssetMenu(fileName = "RacerStats", menuName = "Racer/Stats")]
public class RacerStats : ScriptableObject
{
    public const float MaximumAllowedSteeringAngle = 80f;

    [Header("Movement")]
    public float MaxSpeed = 10f;
    public float Acceleration = 5f;
    public float Drag = 0.2f;
    [Tooltip("Body turn speed at maximum movement speed and full steering, in degrees per second.")]
    public float TurnSpeed = 240f;
    [Tooltip("Maximum wheel direction angle relative to the racer body, in degrees.")]
    [Range(0f, MaximumAllowedSteeringAngle)] public float MaxSteeringAngle = 30f;
}
