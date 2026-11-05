using System;
using System.Collections.Generic;
using GoosingGeese.Racing;
using GoosingGeese.Roads;
using GoosingGeese.Settings;
using UnityEngine;

namespace GoosingGeese.EditorTools
{
    public sealed class RoadSpeedProfile
    {
        private const float RecommendedSpeedMargin = 0.95f;
        private const float MinimumCurvature = 0.00001f;

        public RoadSpeedProfile(RoadCurve curve, RacerConfiguration stats)
        {
            if (curve == null || curve.Points.Count < 2)
                throw new ArgumentException("A speed profile requires at least two curve points.");

            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            var limits = new float[curve.Points.Count];
            float straightSpeed = stats.MaxSpeed * stats.RedZoneThreshold * RecommendedSpeedMargin;
            float turnSpeed = stats.MaxSpeed * stats.YellowZoneThreshold * RecommendedSpeedMargin;
            float angularSpeed = Mathf.Max(0f, Mathf.Min(stats.TurnSpeed, stats.DriftAlignmentSpeed)) * Mathf.Deg2Rad;

            for (int index = 0; index < limits.Length; index++)
            {
                float curvature = GetCurvature(curve.Points, index);
                limits[index] = curvature > MinimumCurvature ? Mathf.Min(turnSpeed, angularSpeed / curvature) : straightSpeed;
            }

            // Coasting follows dv/ds = -drag. Back-propagation reserves enough distance
            // to reach the next limit under the same drag model used by Movement.

            for (int index = limits.Length - 2; index >= 0; index--)
            {
                float distance = Vector3.Distance(curve.Points[index], curve.Points[index + 1]);
                limits[index] = Mathf.Min(limits[index], limits[index + 1] + stats.CoastingDrag * distance);
            }

            Limits = Array.AsReadOnly(limits);
        }

        public RacerConfiguration Stats { get; }
        public IReadOnlyList<float> Limits { get; }

        private static float GetCurvature(IReadOnlyList<Vector3> points, int index)
        {
            if (index == 0 || index == points.Count - 1)
                return 0f;

            Vector3 incoming = points[index] - points[index - 1];
            Vector3 outgoing = points[index + 1] - points[index];
            incoming.y = 0f;
            outgoing.y = 0f;
            float distance = (incoming.magnitude + outgoing.magnitude) * 0.5f;
            return distance > 0f ? Vector3.Angle(incoming, outgoing) * Mathf.Deg2Rad / distance : 0f;
        }
    }
}
