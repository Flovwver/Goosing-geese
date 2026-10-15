using System;
using System.Linq;
using System.Reflection;
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
        var approach = MakeCurve(new[] { new Vector3(0, 0, 0), new Vector3(0, 0, 30), new Vector3(0, 0, 60), new Vector3(0, 0, 90), new Vector3(20, 0, 110), new Vector3(50, 0, 110) });

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
                Require(weakBrake.Limits[i] <= weakBrake.Limits[i + 1] + weakBrake.Stats.CoastingDrag * distance + 0.001f, "Braking envelope must obey the movement drag model.");
                Require(SpeedZones.GetColor(weakBrake.Limits[i], weakBrake.Stats) != SpeedZones.Red, "Guidance must not recommend the overheat zone.");
            }

            VerifyView();
            return "PASS: curvature, steering limits and coasting drag affect recommendations; braking envelope is consistent; no red recommendations; view preserves all points and shares speedometer colors.";
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
        if (!condition)
            throw new Exception(message);
    }

    private static void VerifyView()
    {
        var level = UnityEngine.Object.FindFirstObjectByType<Level>();
        var view = UnityEngine.Object.FindFirstObjectByType<RoadPathView>();
        var speedometer = UnityEngine.Object.FindFirstObjectByType<Speedometer>();
        level.TryGetBuiltPath(out var path, out var settings);
        var parent = view.GetComponent<LineRenderer>();
        var lines = view.GetComponentsInChildren<LineRenderer>().Where(l => l != parent).OrderBy(l => int.Parse(l.name.Substring("Speed hint ".Length))).ToArray();
        Require(lines.Length > 0 && !parent.enabled, "Hint lines must replace the plain line.");
        int point = 0;

        foreach (LineRenderer line in lines)
        {
            Require(line.startColor == SpeedZones.Green || line.startColor == SpeedZones.Yellow, "Hint colors must use the shared palette.");

            for (int index = 0; index < line.positionCount; index++)
            {
                Require(Vector3.Distance(line.GetPosition(index), path.Curve.Points[point] + Vector3.up * settings.PathLineHeight) < 0.001f, "Hint rendering must preserve curve samples and shared endpoints.");

                if (index < line.positionCount - 1)
                    point++;
            }
        }

        Require(point == path.Curve.Points.Count - 1, "Hints must cover the whole route without gaps.");
        var ids = lines.Select(l => l.GetInstanceID()).ToArray();
        level.RestartRace();
        Require(ids.SequenceEqual(view.GetComponentsInChildren<LineRenderer>().Where(l => l != parent).OrderBy(l => int.Parse(l.name.Substring("Speed hint ".Length))).Select(l => l.GetInstanceID())), "Repeat must retain rendered hints.");
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(Speedometer).GetMethod("Update", Flags).Invoke(speedometer, null);
        var green = (UnityEngine.UI.Slider)typeof(Speedometer).GetField("_greenZoneSlider", Flags).GetValue(speedometer);
        var yellow = (UnityEngine.UI.Slider)typeof(Speedometer).GetField("_yellowZoneSlider", Flags).GetValue(speedometer);
        Require(green.fillRect.GetComponent<UnityEngine.UI.Image>().color == SpeedZones.Green && yellow.fillRect.GetComponent<UnityEngine.UI.Image>().color == SpeedZones.Yellow, "Speedometer and hints must use the same colors.");
        var definition = level.CurrentDefinition;
        var field = typeof(Level).GetField("_settings", Flags);
        var original = (RoadSettings)field.GetValue(level);
        var source = UnityEngine.Object.Instantiate(original);

        try
        {
            field.SetValue(level, source);
            source.ShowSpeedHints = false;
            Require(level.TryBuildRoad(out var error), error);
            Require(parent.enabled && view.GetComponentsInChildren<LineRenderer>().Length == 1, "Disabling hints must restore the plain line.");
            source.ShowPathLine = false;
            Require(level.TryBuildRoad(out error), error);
            Require(!parent.enabled && view.GetComponentsInChildren<LineRenderer>().Length == 1, "Path visibility must hide all line renderers.");
        }
        finally
        {
            field.SetValue(level, original);
            UnityEngine.Object.Destroy(source);
            Require(level.TryRestoreLevel(definition, out var error), error);
        }
    }
}
