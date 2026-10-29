using System;
using UnityEngine;

namespace GoosingGeese.Racing
{
    public sealed class OpponentDefinition
    {
        public OpponentDefinition(RacerDefinition racer,
            float cruiseSpeed,
            float cornerSpeed,
            float laneOffset,
            float brakingPreviewDistance,
            GameObject vehicle)
        {
            Racer = racer ?? throw new ArgumentNullException(nameof(racer));

            foreach (float value in new[]
            {
                cruiseSpeed,
                cornerSpeed,
                laneOffset,
                brakingPreviewDistance
            }

            )
            {
                if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                    throw new ArgumentException("Opponent parameters must be finite and non-negative.");
            }

            if (cruiseSpeed > 1f || cornerSpeed > 1f)
                throw new ArgumentException("Opponent speed fractions must not exceed one.");

            CruiseSpeed = cruiseSpeed;
            CornerSpeed = cornerSpeed;
            LaneOffset = laneOffset;
            BrakingPreviewDistance = brakingPreviewDistance;
            Vehicle = vehicle;
        }

        public RacerDefinition Racer { get; }
        public float CruiseSpeed { get; }
        public float CornerSpeed { get; }
        public float LaneOffset { get; }
        public float BrakingPreviewDistance { get; }
        public GameObject Vehicle { get; }
    }
}
