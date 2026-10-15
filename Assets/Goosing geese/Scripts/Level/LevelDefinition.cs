using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelDefinition
{
    public LevelDefinition(int seed, RoadPieceType[] layout, RoadSettings settings, Vector3 startPosition, Quaternion startRotation)
    {
        Seed = seed;
        Layout = Array.AsReadOnly((RoadPieceType[])layout.Clone());
        Settings = settings;
        StartPosition = startPosition;
        StartRotation = startRotation;
    }

    public int Seed { get; }
    public IReadOnlyList<RoadPieceType> Layout { get; }
    public RoadSettings Settings { get; }
    public Vector3 StartPosition { get; }
    public Quaternion StartRotation { get; }
}
