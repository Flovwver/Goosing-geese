using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class LevelSnapshotTests
{
    public static string Main()
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var level = UnityEngine.Object.FindFirstObjectByType<Level>();
        var racer = (Racer)typeof(Level).GetField("_racer", Flags).GetValue(level);
        var navigator = racer.GetComponent<RoadNavigator>();
        var roadField = typeof(Level).GetField("_settings", Flags);
        var statsField = typeof(Racer).GetField("_stats", Flags);
        var navigationField = typeof(RoadNavigator).GetField("_settings", Flags);
        var roadSource = (RoadSettings)roadField.GetValue(level);
        var statsSource = (RacerStats)statsField.GetValue(racer);
        var navigationSource = (NavigationSettings)navigationField.GetValue(navigator);
        var road = UnityEngine.Object.Instantiate(roadSource);
        var stats = UnityEngine.Object.Instantiate(statsSource);
        var navigation = UnityEngine.Object.Instantiate(navigationSource);
        var definition = level.CurrentDefinition;
        level.TryGetBuiltPath(out var path, out _);
        var points = path.Curve.Points.ToArray();
        int seed = definition.Seed;

        try
        {
            roadField.SetValue(level, road);
            statsField.SetValue(racer, stats);
            navigationField.SetValue(navigator, navigation);
            road.RoadPieceCount += 5;
            road.CurveSamplesPerSegment += 4;
            stats.MaxSpeed *= 0.5f;
            stats.TimeToOverheat += 3f;
            navigation.LookAheadDistance += 20f;
            navigation.ArrivalDistance += 2f;
            level.RestartRace();
            Require(ReferenceEquals(level.CurrentDefinition, definition), "Repeat must keep the captured definition.");
            Require(ReferenceEquals(racer.Configuration, definition.Racer.Stats), "Movement must use the captured racer configuration.");
            Require(ReferenceEquals(navigator.Configuration, definition.Racer.Navigation), "Navigation must use the captured configuration.");
            Require(racer.Configuration.MaxSpeed != stats.MaxSpeed && navigator.Configuration.LookAheadDistance != navigation.LookAheadDistance, "Source edits must not mutate the captured conditions.");
            racer.ConfigureStats(stats);
            Require(level.TryBuildRoad(out var error), "Creating a new configuration failed: " + error);
            Require(level.CurrentDefinition.Settings.RoadPieceCount == UnityEngine.Object.FindFirstObjectByType<RaceController>().CurrentStage.PieceCount && racer.Configuration.MaxSpeed == stats.MaxSpeed && navigator.Configuration.LookAheadDistance == navigation.LookAheadDistance, "New levels must capture current source settings.");
            Require(level.TryRestoreLevel(definition, out error), "Restoring a definition failed: " + error);
            level.TryGetBuiltPath(out var restored, out _);
            Require(definition.Seed == seed && restored.Curve.Points.SequenceEqual(points), "Restoration must reproduce the original geometry without random generation.");
            Require(ReferenceEquals(racer.Configuration, definition.Racer.Stats) && ReferenceEquals(navigator.Configuration, definition.Racer.Navigation), "Restoration must reproduce captured racer and navigation conditions.");
            Require(Vector3.Distance(racer.transform.position, definition.Racer.Position) < 0.001f && Quaternion.Angle(racer.transform.rotation, definition.Racer.Rotation) < 0.001f, "Restoration must use the captured racer start pose.");

            foreach (Type type in new[] { typeof(LevelDefinition), typeof(RoadConfiguration), typeof(RacerDefinition), typeof(RacerConfiguration), typeof(NavigationConfiguration) })
                Require(type.IsSealed && type.GetProperties().All(p => !p.CanWrite), type.Name + " must not expose mutable snapshot properties.");

            return "PASS: source settings edits cannot alter repeats; new levels capture edits; restoration reproduces seed, geometry, racer/nav settings and start pose; snapshots expose no mutable properties.";
        }
        finally
        {
            roadField.SetValue(level, roadSource);
            statsField.SetValue(racer, statsSource);
            navigationField.SetValue(navigator, navigationSource);
            UnityEngine.Object.Destroy(road);
            UnityEngine.Object.Destroy(stats);
            UnityEngine.Object.Destroy(navigation);
            Require(level.TryRestoreLevel(definition, out var restoreError), restoreError);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new Exception(message);
    }
}
