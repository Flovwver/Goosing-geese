using System;
using UnityEngine;

public sealed class NavigationConfiguration
{
    public NavigationConfiguration(NavigationSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        ArrivalDistance = settings.ArrivalDistance;
        LookAheadDistance = settings.LookAheadDistance;
        WaypointPassTolerance = settings.WaypointPassTolerance;
        DebugPointRadius = settings.DebugPointRadius;

        foreach (float value in new[] { ArrivalDistance, LookAheadDistance, WaypointPassTolerance, DebugPointRadius })
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentException("Navigation configuration values must be finite.");
        }

        if (ArrivalDistance < 0f || LookAheadDistance < 0f || WaypointPassTolerance < 0f || DebugPointRadius < 0f)
            throw new ArgumentException("Navigation distances must be non-negative.");
    }

    public float ArrivalDistance { get; }
    public float LookAheadDistance { get; }
    public float WaypointPassTolerance { get; }
    public float DebugPointRadius { get; }
}
