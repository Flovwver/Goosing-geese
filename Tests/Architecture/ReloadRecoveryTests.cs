using System;
using System.Linq;
using System.Reflection;
using GoosingGeese.Levels;
using GoosingGeese.Racing;
using UnityEditor;
using UnityEngine;

public static class ReloadRecoveryTests
{
    public static string Main()
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var level = UnityEngine.Object.FindFirstObjectByType<Level>();
        var definition = level.CurrentDefinition;

        try
        {
            typeof(Level).GetMethod("OnDisable", Flags).Invoke(level, null);
            typeof(Level).GetField("<CurrentDefinition>k__BackingField", Flags).SetValue(level, null);
            typeof(Level).GetField("_roadPath", Flags).SetValue(level, null);
            typeof(Level).GetField("_roadBuilder", Flags).SetValue(level, null);
            typeof(Level).GetMethod("OnEnable", Flags).Invoke(level, null);
            typeof(Level).GetMethod("Update", Flags).Invoke(level, null);
            Require(level.CurrentDefinition != null, "First frame must recover a captured definition.");
            Require(level.Seed == definition.Seed && level.CurrentDefinition.Layout.SequenceEqual(definition.Layout), "Reload must retain the route.");
            Require(level.CurrentDefinition.Opponent.Racer.Stats.MaxSpeed == definition.Opponent.Racer.Stats.MaxSpeed, "Reload must retain opponent physics.");
            int missing = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.scene.IsValid()).Sum(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount);
            Require(missing == 0, "Scene references must survive namespaces and reload.");
            return "PASS: first-frame reload recovery; player/opponent snapshot; route identity; no missing scene scripts.";
        }
        finally
        {
            level.TryRestoreLevel(definition, out _);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }
}
