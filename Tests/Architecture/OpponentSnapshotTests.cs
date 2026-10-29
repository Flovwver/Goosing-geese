using System;
using System.Reflection;
using GoosingGeese.Levels;
using GoosingGeese.Racing;
using GoosingGeese.Roads;
using UnityEngine;

public static class OpponentSnapshotTests
{
    public static string Main()
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var level = UnityEngine.Object.FindFirstObjectByType<Level>();
        var spawner = UnityEngine.Object.FindFirstObjectByType<OpponentSpawner>();
        var definition = level.CurrentDefinition;
        var captured = definition.Opponent;
        Require(captured != null, "Normal levels must capture their opponent.");
        var multiplier = typeof(OpponentSpawner).GetField("_turnRateMultiplier", Flags);
        float original = (float)multiplier.GetValue(spawner);

        try
        {
            multiplier.SetValue(spawner, original + 10f);
            level.RestartRace();
            Require(spawner.Opponent.Configuration == captured.Racer.Stats, "Retry must use captured opponent stats.");
            var restored = JsonUtility.FromJson<OpponentSnapshot>(JsonUtility.ToJson(new OpponentSnapshot(captured))).Restore();
            Require(restored.Racer.Stats.TurnSpeed == captured.Racer.Stats.TurnSpeed
                && restored.Racer.Position == captured.Racer.Position
                && restored.BrakingPreviewDistance == captured.BrakingPreviewDistance,
                "Reload snapshot must restore physics, pose and behaviour.");
            spawner.enabled = false;
            level.CreateLevel(unchecked(definition.Seed + 1));
            spawner.enabled = true;
            var driver = spawner.Opponent.GetComponent<OpponentDriver>();
            level.TryGetBuiltPath(out RoadPath path, out _);
            Require(typeof(OpponentDriver).GetField("_path", Flags).GetValue(driver) == path, "Re-enabled spawner must synchronize the current path.");
            spawner.Opponent.Tick(true, 0.1f);
            driver.enabled = false;
            Require((bool)typeof(Mover).GetField("_isAccelerate",
                Flags).GetValue(spawner.Opponent.GetComponent<Mover>()) == false,
                "Disabling the driver must clear movement commands.");
            driver.enabled = true;
            return "PASS: captured opponent retry; reload round trip; missed path recovery; driver disable.";
        }
        finally
        {
            multiplier.SetValue(spawner, original);
            spawner.enabled = true;
            level.TryRestoreLevel(definition, out _);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }
}
