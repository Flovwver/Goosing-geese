using System;
using UnityEngine;

public sealed class RacerDefinition
{
    public RacerDefinition(RacerConfiguration stats, NavigationConfiguration navigation, Vector3 position, Quaternion rotation)
    {
        Stats = stats ?? throw new ArgumentNullException(nameof(stats));
        Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        Position = position;
        Rotation = rotation;
    }

    public RacerConfiguration Stats { get; }
    public NavigationConfiguration Navigation { get; }
    public Vector3 Position { get; }
    public Quaternion Rotation { get; }
}
