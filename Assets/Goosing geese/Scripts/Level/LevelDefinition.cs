using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LevelDefinition
{
    public LevelDefinition(int seed, RoadPieceType[] layout, RoadConfiguration settings, Vector3 startPosition, Quaternion startRotation, RacerDefinition racer, int number = 1)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        if (layout == null || layout.Length != settings.RoadPieceCount + settings.FinishRunoutPieces)
            throw new ArgumentException("Layout length must match the captured road configuration.");

        Number = number;
        Seed = seed;
        Layout = Array.AsReadOnly((RoadPieceType[])layout.Clone());
        Settings = settings;
        StartPosition = startPosition;
        StartRotation = startRotation;
        Racer = racer ?? throw new ArgumentNullException(nameof(racer));
    }

    public int Number { get; }
    public int Seed { get; }
    public IReadOnlyList<RoadPieceType> Layout { get; }
    public RoadConfiguration Settings { get; }
    public Vector3 StartPosition { get; }
    public Quaternion StartRotation { get; }
    public RacerDefinition Racer { get; }
}
