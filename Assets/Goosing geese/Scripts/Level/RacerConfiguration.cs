using System;
using UnityEngine;

public sealed class RacerConfiguration
{
    public RacerConfiguration(RacerStats settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        MaxSpeed = settings.MaxSpeed;
        Acceleration = settings.Acceleration;
        Drag = settings.Drag;
        CoastingDrag = settings.CoastingDrag;
        FinishDrag = settings.FinishDrag;
        TurnSpeed = settings.TurnSpeed;
        MaxSteeringAngle = settings.MaxSteeringAngle;
        DriftAlignmentSpeed = settings.DriftAlignmentSpeed;
        GripTransitionSpeed = settings.GripTransitionSpeed;
        YellowZoneThreshold = settings.YellowZoneThreshold;
        RedZoneThreshold = settings.RedZoneThreshold;
        TimeToOverheat = settings.TimeToOverheat;

        if (MaxSpeed <= 0f || Acceleration < 0f || Drag < 0f || CoastingDrag < 0f || FinishDrag < 0f || TimeToOverheat < 0f)
            throw new ArgumentException("Racer speed must be positive and acceleration, drag and overheat time must be non-negative.");

        if (YellowZoneThreshold < 0f || RedZoneThreshold < YellowZoneThreshold || RedZoneThreshold > 1f)
            throw new ArgumentException("Speed zone thresholds must be ordered between zero and one.");
    }

    public float MaxSpeed { get; }
    public float Acceleration { get; }
    public float Drag { get; }
    public float CoastingDrag { get; }
    public float FinishDrag { get; }
    public float TurnSpeed { get; }
    public float MaxSteeringAngle { get; }
    public float DriftAlignmentSpeed { get; }
    public float GripTransitionSpeed { get; }
    public float YellowZoneThreshold { get; }
    public float RedZoneThreshold { get; }
    public float TimeToOverheat { get; }
}
