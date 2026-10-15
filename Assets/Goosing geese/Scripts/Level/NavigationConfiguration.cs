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

        if (ArrivalDistance < 0f || LookAheadDistance < 0f || WaypointPassTolerance < 0f || DebugPointRadius < 0f)
            throw new ArgumentException("Navigation distances must be non-negative.");
    }

    public float ArrivalDistance { get; }
    public float LookAheadDistance { get; }
    public float WaypointPassTolerance { get; }
    public float DebugPointRadius { get; }
}
