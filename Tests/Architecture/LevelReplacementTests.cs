using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GoosingGeese.Levels;
using GoosingGeese.Roads;
using GoosingGeese.Settings;
using UnityEngine;

public static class LevelReplacementTests
{
    public static async Task<string> Main()
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var level = UnityEngine.Object.FindFirstObjectByType<Level>();
        var field = typeof(Level).GetField("_settings", Flags);
        var source = (RoadSettings)field.GetValue(level);
        var bad = UnityEngine.Object.Instantiate(source);
        var definition = level.CurrentDefinition;
        level.TryGetBuiltPath(out var path, out _);
        var originalPieces = UnityEngine.Object.FindObjectsByType<RoadPiece>(FindObjectsSortMode.None).Select(p => p.GetInstanceID()).OrderBy(x => x).ToArray();
        GameObject brokenPrefab = null;

        try
        {
            field.SetValue(level, bad);
            bad.RoadPieceCount = 0;
            RequireFailure(level, definition, path, originalPieces, "Invalid road length");
            bad.RoadPieceCount = source.RoadPieceCount;
            var entries = source.PiecePrefabs.ToArray();
            var entriesField = typeof(RoadSettings).GetField("_piecePrefabs", Flags);
            entriesField.SetValue(bad, new[] { entries[0], entries[0] });
            RequireFailure(level, definition, path, originalPieces, "Duplicate prefab type");
            entriesField.SetValue(bad, new RoadPiecePrefabEntry[0]);
            RequireFailure(level, definition, path, originalPieces, "Missing prefab");
            RoadPiece straight = source.PiecePrefabs.First(e => e.Type == RoadPieceType.Straight).Prefab;
            brokenPrefab = UnityEngine.Object.Instantiate(straight.gameObject);
            brokenPrefab.SetActive(false);
            brokenPrefab.transform.localScale = Vector3.zero;
            var brokenEntry = new RoadPiecePrefabEntry();
            typeof(RoadPiecePrefabEntry).GetField("_type", Flags).SetValue(brokenEntry, RoadPieceType.Straight);
            typeof(RoadPiecePrefabEntry).GetField("_prefab", Flags).SetValue(brokenEntry, brokenPrefab.GetComponent<RoadPiece>());
            entriesField.SetValue(bad, new[] { brokenEntry });
            typeof(RoadSettings).GetField("_availablePieceTypes", Flags).SetValue(bad, new[] { RoadPieceType.Straight });
            RequireFailure(level, definition, path, originalPieces, "Geometry failure after candidate objects exist");
            await Task.Delay(150);
            Require(UnityEngine.Object.FindObjectsByType<RoadPiece>(FindObjectsSortMode.None).Select(p => p.GetInstanceID()).OrderBy(x => x).SequenceEqual(originalPieces),
                "Failed build must not leave active candidate pieces.");
            field.SetValue(level, source);
            Require(level.TryBuildRoad(out var error), "Valid replacement failed: " + error);
            Require((ReferenceEquals(definition, level.CurrentDefinition) == false), "A successful replacement must commit the candidate definition.");
            Require(level.TryGetBuiltPath(out var next,
                out _)
                && next.Curve.Length > 0
                && next.FinishPoint != null,
                "A successful replacement must have a complete path and finish.");
            return "PASS: invalid length, duplicate/missing prefabs and post-build geometry failure preserve the old level; failed candidate objects are cleaned; valid replacement succeeds.";
        }
        finally
        {
            field.SetValue(level, source);
            UnityEngine.Object.Destroy(bad);

            if (brokenPrefab != null)
                UnityEngine.Object.Destroy(brokenPrefab);
        }
    }

    private static void RequireFailure(Level level, LevelDefinition definition, RoadPath path, int[] pieces, string scenario)
    {
        Require((level.TryBuildRoad(out string error) == false) && (string.IsNullOrWhiteSpace(error) == false), scenario + " must return a diagnostic.");
        Require(ReferenceEquals(definition, level.CurrentDefinition), scenario + " replaced the current definition.");
        Require(level.TryGetBuiltPath(out var current, out _) && ReferenceEquals(path, current), scenario + " replaced the working path.");
        Require(UnityEngine.Object.FindObjectsByType<RoadPiece>(FindObjectsSortMode.None).Select(p => p.GetInstanceID()).OrderBy(x => x).SequenceEqual(pieces),
            scenario
            + " changed active road objects.");
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }
}
