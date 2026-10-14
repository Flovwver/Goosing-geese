using UnityEngine;

[CreateAssetMenu(fileName = "RacerStats", menuName = "Racer/Stats")]
public class RacerStats : ScriptableObject
{
    public const float MaximumAllowedSteeringAngle = 80f;
    private const float MinimumTimeToOverheat = 0.01f;

    [Header("Movement")]
    public float MaxSpeed = 10f;
    public float Acceleration = 5f;
    public float Drag = 0.2f;
    [Tooltip("Body turn speed at maximum movement speed and full steering, in degrees per second.")]
    public float TurnSpeed = 240f;
    [Tooltip("Maximum wheel direction angle relative to the racer body, in degrees.")]
    [Range(0f, MaximumAllowedSteeringAngle)] public float MaxSteeringAngle = 30f;
    [Tooltip("Velocity alignment speed while drifting, in degrees per second.")]
    [Min(0f)] public float DriftAlignmentSpeed = 90f;
    [Tooltip("How quickly grip enters or leaves drift, in normalized units per second.")]
    [Min(0f)] public float GripTransitionSpeed = 3f;

    [Header("Overheat")]
    [Range(0f, 1f)] public float YellowZoneThreshold = 0.5f;
    [Range(0f, 1f)] public float RedZoneThreshold = 0.8f;
    [Min(MinimumTimeToOverheat)] public float TimeToOverheat = 3f;

    private void OnValidate()
    {
        YellowZoneThreshold = Mathf.Clamp01(YellowZoneThreshold);
        RedZoneThreshold = Mathf.Clamp(RedZoneThreshold, YellowZoneThreshold, 1f);
        DriftAlignmentSpeed = Mathf.Max(0f, DriftAlignmentSpeed);
        GripTransitionSpeed = Mathf.Max(0f, GripTransitionSpeed);
        TimeToOverheat = Mathf.Max(MinimumTimeToOverheat, TimeToOverheat);
    }
}
