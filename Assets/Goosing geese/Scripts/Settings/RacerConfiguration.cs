using System;
using GoosingGeese.Racing;
using UnityEngine;

namespace GoosingGeese.Settings
{
    public sealed class RacerConfiguration
    {
        private readonly string _sourceJson;

        public RacerConfiguration(RacerStats settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _sourceJson = JsonUtility.ToJson(settings);
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
            CoolingRate = settings.CoolingRate;

            foreach (float value in new[]
            {
                MaxSpeed,
                Acceleration,
                Drag,
                CoastingDrag,
                FinishDrag,
                TurnSpeed,
                MaxSteeringAngle,
                DriftAlignmentSpeed,
                GripTransitionSpeed,
                YellowZoneThreshold,
                RedZoneThreshold,
                TimeToOverheat,
                CoolingRate
            }

            )
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    throw new ArgumentException("Racer configuration values must be finite.");
            }

            if (MaxSpeed <= 0f || Acceleration < 0f || Drag < 0f || CoastingDrag < 0f || FinishDrag < 0f || TimeToOverheat < 0f || CoolingRate < 0f)
                throw new ArgumentException("Racer speed must be positive and acceleration, drag and overheat time must be non-negative.");

            if (YellowZoneThreshold < 0f || RedZoneThreshold < YellowZoneThreshold || RedZoneThreshold > 1f)
                throw new ArgumentException("Speed zone thresholds must be ordered between zero and one.");

            if (TurnSpeed < 0f
                || MaxSteeringAngle < 0f
                || MaxSteeringAngle > RacerStats.MaximumAllowedSteeringAngle
                || DriftAlignmentSpeed < 0f
                || GripTransitionSpeed < 0f)
                throw new ArgumentException("Racer steering and grip limits must be valid and non-negative.");
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
        public float CoolingRate { get; }

        internal string ToJson()
        {
            return _sourceJson;
        }
    }
}
