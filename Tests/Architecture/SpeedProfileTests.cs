using System;
using System.Linq;
using System.Reflection;
using GoosingGeese.EditorTools;
using GoosingGeese.Levels;
using GoosingGeese.Presentation;
using GoosingGeese.Roads;
using GoosingGeese.Settings;
using UnityEngine;

public static class SpeedProfileTests
{
    public static string Main()
    {
        var source = ScriptableObject.CreateInstance<RacerStats>();
        source.MaxSpeed = 100f;
        source.YellowZoneThreshold = 0.5f;
        source.RedZoneThreshold = 0.75f;
        source.CoastingDrag = 0.1f;
        var approach = MakeCurve(new[] { new Vector3(0,
            0,
            0), new Vector3(0,
            0,
            30), new Vector3(0,
            0,
            60), new Vector3(0,
            0,
            90), new Vector3(20,
            0,
            110), new Vector3(50,
            0,
            110) });

        try
        {
            var weakBrake = new RoadSpeedProfile(approach, new RacerConfiguration(source));
            source.CoastingDrag = 2f;
            var strongBrake = new RoadSpeedProfile(approach, new RacerConfiguration(source));
            Require(weakBrake.Limits[0] < strongBrake.Limits[0], "Weaker coasting drag must reserve more braking distance upstream.");
            var gentleCurve = MakeCurve(new[] { Vector3.zero, Vector3.forward * 20f, new Vector3(20f, 0f, 20f) });
            var tightCurve = MakeCurve(new[] { Vector3.zero, Vector3.forward * 2f, new Vector3(2f, 0f, 2f) });
            var stats = new RacerConfiguration(source);
            var gentle = new RoadSpeedProfile(gentleCurve, stats);
            var tight = new RoadSpeedProfile(tightCurve, stats);
            Require(tight.Limits[1] < gentle.Limits[1], "Higher curvature must lower recommended turn speed.");
            source.DriftAlignmentSpeed = 20f;
            var slowerSteering = new RoadSpeedProfile(tightCurve, new RacerConfiguration(source));
            Require(slowerSteering.Limits[1] < tight.Limits[1], "Slower direction alignment must lower the turn recommendation.");

            for (int i = 0; i < weakBrake.Limits.Count - 1; i++)
            {
                float distance = Vector3.Distance(approach.Points[i], approach.Points[i + 1]);
                Require(weakBrake.Limits[i] <= weakBrake.Limits[i + 1]
                    + weakBrake.Stats.CoastingDrag * distance
                    + 0.001f,
                    "Braking envelope must obey the movement drag model.");
                Require(new SpeedZones().GetColor(weakBrake.Limits[i],
                    weakBrake.Stats) != new SpeedZones().Red,
                    "Guidance must not recommend the overheat zone.");
            }

            VerifyView();
            return "PASS: curvature, steering limits and coasting drag affect recommendations; braking envelope is consistent; no red recommendations; road sections cover the route with seamless transitions and visibility control.";
        }
        finally
        {
            UnityEngine.Object.Destroy(source);
        }
    }

    private static RoadCurve MakeCurve(Vector3[] points)
    {
        return new RoadCurve(points, new[] { 0, points.Length - 1 });
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }

    private static void VerifyView()
    {
        var level = UnityEngine.Object.FindFirstObjectByType<Level>();
        var view = UnityEngine.Object.FindFirstObjectByType<RoadPathView>();
        level.TryGetBuiltPath(out var path, out var settings);
        var lines = view.GetComponentsInChildren<LineRenderer>();
        Require(lines.Length == path.Pieces.Count, "Every road piece must have a guidance section.");

        for (int index = 0; index < lines.Length; index++)
        {
            float start = index == 0 ? 0f : path.Curve.GetSegmentEndDistance(index - 1);
            float end = path.Curve.GetSegmentEndDistance(index);
            Require(Vector3.Distance(lines[index].GetPosition(0),
                path.Curve.GetPointAtDistance(start)
                + Vector3.up * settings.PathLineHeight) < 0.001f,
                "Section start mismatch.");
            Require(Vector3.Distance(lines[index].GetPosition(lines[index].positionCount - 1),
                path.Curve.GetPointAtDistance(end)
                + Vector3.up * settings.PathLineHeight) < 0.001f,
                "Section end mismatch.");

            if (index > 0)
                Require(lines[index - 1].colorGradient.Evaluate(1f) == lines[index].colorGradient.Evaluate(0f), "Color transitions must join without a seam.");
        }

        var ids = lines.Select(line => line.GetInstanceID()).ToArray();
        level.RestartRace();
        Require(ids.SequenceEqual(view.GetComponentsInChildren<LineRenderer>().Select(line => line.GetInstanceID())), "Repeat must retain rendered guidance.");
        var definition = level.CurrentDefinition;
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var field = typeof(Level).GetField("_settings", Flags);
        var original = (RoadSettings)field.GetValue(level);
        var source = UnityEngine.Object.Instantiate(original);

        try
        {
            field.SetValue(level, source);
            source.ShowPathLine = false;
            Require(level.TryBuildRoad(out var error), error);
            Require(view.GetComponentsInChildren<LineRenderer>().Length == 0, "Path visibility must hide guidance.");
        }
        finally
        {
            field.SetValue(level, original);
            UnityEngine.Object.Destroy(source);
            Require(level.TryRestoreLevel(definition, out var error), error);
        }
    }
}
