using System;
using UnityEngine;

public static class GripAndOverheatTests
{
    public static string Main()
    {
        var source = ScriptableObject.CreateInstance<RacerStats>();
        source.MaxSpeed = 100f;
        source.YellowZoneThreshold = 0.5f;
        source.RedZoneThreshold = 0.75f;
        source.TimeToOverheat = 2f;
        var stats = new RacerConfiguration(source);
        var grip = new GripModel();
        var root = new GameObject("Overheat regression test");
        var heat = root.AddComponent<OverheatTracker>();

        try
        {
            Require(grip.CalculateSlipFactor(0f, stats) == 0f && grip.CalculateSlipFactor(49f, stats) == 0f, "Grip must hold below the yellow threshold.");
            Require(grip.CalculateSlipFactor(50f, stats) == 1f && grip.CalculateSlipFactor(100f, stats) == 1f, "Slip must begin at the configured yellow threshold.");
            int explosions = 0;
            heat.Exploded += () => explosions++;
            heat.Tick(100f, stats, 0.5f);
            Require(Mathf.Approximately(heat.Progress, 0.25f), "Heat must integrate the explicit simulation step.");
            heat.Tick(0f, stats, 0.25f);
            Require(Mathf.Approximately(heat.Progress, 0.125f), "Cooling must use the explicit simulation step.");
            heat.Tick(100f, stats, 0f);
            Require(Mathf.Approximately(heat.Progress, 0.125f), "A paused step must not change heat.");
            heat.Tick(100f, stats, 2f);
            heat.Tick(100f, stats, 2f);
            Require(heat.IsExploded && explosions == 1, "Overheat must explode exactly once.");
            heat.ResetOverheat();
            Require(!heat.IsExploded && heat.Progress == 0f && grip.CalculateSlipFactor(100f, stats) == 1f, "Resetting heat must not change the independent grip rule.");
            return "PASS: grip thresholds are independent; heat/cooling/pause use explicit time; explosion is raised once and reset correctly.";
        }
        finally
        {
            UnityEngine.Object.Destroy(root);
            UnityEngine.Object.Destroy(source);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new Exception(message);
    }
}
