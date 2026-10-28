using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public static class AuditLifecycleTests
{
    public static string Main()
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var race = UnityEngine.Object.FindFirstObjectByType<RaceController>();
        var level = UnityEngine.Object.FindFirstObjectByType<Level>();
        var racer = (Racer)typeof(RaceController).GetField("_player", Flags).GetValue(race);
        var safety = UnityEngine.Object.FindFirstObjectByType<RoadSafety>();
        var scale = UnityEngine.Object.FindFirstObjectByType<SpeedometerScaleView>(FindObjectsInactive.Include);
        var grace = typeof(RoadSafety).GetField("_offRoadGraceTime", Flags);
        var labels = typeof(SpeedometerScaleView).GetField("_labels", Flags);
        float originalGrace = (float)grace.GetValue(safety);
        object originalLabels = labels.GetValue(scale);

        try
        {
            level.RestartRace();
            race.StartRace();
            racer.Tick(true, 0.1f);
            racer.enabled = false;
            racer.Tick(true, 0.1f);
            Require(racer.GetCurrentSpeed() == 0f, "Disabled racer must stop.");
            Require((bool)typeof(Mover).GetField("_isAccelerate", Flags).GetValue(racer.GetComponent<Mover>()) == false, "Disabled racer must reject ticks.");
            racer.enabled = true;
            racer.GetComponent<RoadNavigator>().UpdateTarget(racer.transform.position);
            grace.SetValue(safety, 0f);
            typeof(RoadSafety).GetMethod("Update", Flags).Invoke(safety, null);
            Require(race.State == RaceState.Racing, "Zero grace must not lose while on the road.");
            labels.SetValue(scale, new Text[1]);
            scale.SetValues(100f, 0.4f, 0.8f);
            labels.SetValue(scale, new Text[0]);
            scale.SetValues(101f, 0.4f, 0.8f);
            labels.SetValue(scale, null);
            scale.SetValues(102f, 0.4f, 0.8f);
            return "PASS: disabled racer rejects commands; zero grace on road; missing and short scale labels.";
        }
        finally
        {
            racer.enabled = true;
            grace.SetValue(safety, originalGrace);
            labels.SetValue(scale, originalLabels);
            level.RestartRace();
        }
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }
}
