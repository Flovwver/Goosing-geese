using System;
using System.Reflection;
using UnityEngine;

public static class PreparationBoundaryTests
{
    public static string Main()
    {
        var level = UnityEngine.Object.FindFirstObjectByType<Level>();
        var definition = level.CurrentDefinition;
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var finishField = typeof(Level).GetField("_finishLine", Flags);
        object finish = finishField.GetValue(level);
        bool notified = false;
        int expectedErrors = 0;
        Action<RoadPath, RoadConfiguration> broken = (path, settings) => throw new Exception("Expected audit observer failure");
        Action<RoadPath, RoadConfiguration> observer = (path, settings) => notified = true;
        Application.LogCallback log = (message, stack, type) =>
        {
            if (message.Contains("Expected audit observer failure"))
                expectedErrors++;
        };

        try
        {
            level.TryGetBuiltPath(out RoadPath previous, out _);
            finishField.SetValue(level, null);
            Require(level.TryRestoreLevel(definition, out _) == false, "Missing preparation dependency must reject candidate.");
            level.TryGetBuiltPath(out RoadPath retained, out _);
            Require(ReferenceEquals(previous, retained), "Preparation validation must preserve active road.");
            finishField.SetValue(level, finish);
            level.PathBuilt += broken;
            level.PathBuilt += observer;
            Application.logMessageReceived += log;
            Require(level.TryRestoreLevel(definition, out _), "Observer failure must not turn committed replacement into failure.");
            Require(notified && expectedErrors == 1, "Observer failure must be reported and later observers notified.");
            return "PASS: dependency validation preserves previous road; committed replacement isolates and reports observer errors.";
        }
        finally
        {
            finishField.SetValue(level, finish);
            level.PathBuilt -= broken;
            level.PathBuilt -= observer;
            Application.logMessageReceived -= log;
        }
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }
}
