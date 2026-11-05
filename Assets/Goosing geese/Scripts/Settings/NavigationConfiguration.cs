using System;
using UnityEngine;

namespace GoosingGeese.Settings
{
    public sealed class NavigationConfiguration
    {
        private readonly string _sourceJson;

        public NavigationConfiguration(NavigationSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _sourceJson = JsonUtility.ToJson(settings);
            ArrivalDistance = settings.ArrivalDistance;
            LookAheadDistance = settings.LookAheadDistance;
            WaypointPassTolerance = settings.WaypointPassTolerance;
            DebugPointRadius = settings.DebugPointRadius;
            FinishHalfWidth = settings.FinishHalfWidth;

            foreach (float value in new[]
            {
                ArrivalDistance,
                LookAheadDistance,
                WaypointPassTolerance,
                DebugPointRadius,
                FinishHalfWidth
            }

            )
            {
                if (float.IsNaN(value) || float.IsInfinity(value))
                    throw new ArgumentException("Navigation configuration values must be finite.");
            }

            if (ArrivalDistance < 0f || LookAheadDistance < 0f || WaypointPassTolerance < 0f || DebugPointRadius < 0f || FinishHalfWidth < 0f)
                throw new ArgumentException("Navigation distances must be non-negative.");
        }

        public float ArrivalDistance { get; }
        public float LookAheadDistance { get; }
        public float WaypointPassTolerance { get; }
        public float DebugPointRadius { get; }
        public float FinishHalfWidth { get; }

        internal string ToJson()
        {
            return _sourceJson;
        }
    }
}
