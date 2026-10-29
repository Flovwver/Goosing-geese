using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public static class PresentationContractsTests
{
    public static string Main()
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var race = UnityEngine.Object.FindFirstObjectByType<RaceController>();
        var screens = race.GetComponent<GameScreens>();
        var world = race.GetComponent<WorldPresentation>();
        var level = race.GetComponent<Level>();
        var definition = level.CurrentDefinition;
        string profile = race.Progress.ToJson();

        try
        {
            var label = (Text)typeof(GameScreens).GetField("_garageLabel", Flags).GetValue(screens);
            string before = label.text;
            race.Progress.ToggleLanguage();
            Require(label.text != before, "Language change must refresh existing labels through model notification.");
            race.Progress.ToggleLanguage();
            Require(label.text == before, "Language toggle must be reversible.");
            world.enabled = false;
            level.CreateLevel(unchecked(level.Seed + 1));
            world.enabled = true;
            object builder = typeof(WorldPresentation).GetField("_builder", Flags).GetValue(world);
            var root = (GameObject)typeof(SceneryBuilder).GetField("_root", Flags).GetValue(builder);
            Require(root != null && root.activeSelf, "Re-enabled world must rebuild the current level.");
            Require(typeof(WorldPresentation).GetField("_path", Flags).GetValue(world) != null, "World must synchronize its current path.");
            return "PASS: immediate localization refresh; reversible language; late world path recovery.";
        }
        finally
        {
            world.enabled = true;
            race.Progress.RestoreJson(profile, Resources.Load<RacerAppearanceCatalog>("RacerAppearanceCatalog"));
            level.TryRestoreLevel(definition, out _);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }
}
