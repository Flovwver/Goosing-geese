using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class MergeIntegrationTests
{
    public static string Main()
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var race = UnityEngine.Object.FindFirstObjectByType<RaceController>();
        var level = UnityEngine.Object.FindFirstObjectByType<Level>();
        var garage = UnityEngine.Object.FindFirstObjectByType<RacerGarage>();
        var racer = (Racer)typeof(RaceController).GetField("_player", Flags).GetValue(race);
        var appearance = racer.GetComponent<RacerAppearance>();
        var heat = racer.GetComponent<HeatFeedback>();
        var catalog = Resources.Load<RacerAppearanceCatalog>("RacerAppearanceCatalog");
        var root = new GameObject("Merge finish regression");
        int results = 0;
        Action<bool> onResult = won => results++;
        race.RaceEnded += onResult;

        try
        {
            Require(catalog.GetRelativeModelIndex(0, 1) == 3 && catalog.GetRelativeModelIndex(5, 1) == 0, "Model arrows must skip goose colors.");

            foreach (int color in new[] { 0, 1, 2, 0 })
            {
                garage.Browse(color);

                foreach (Animator animator in appearance.GetComponentsInChildren<Animator>())
                    animator.Update(0.1f);

                typeof(HeatFeedback).GetMethod("LateUpdate", Flags).Invoke(heat, null);
                var renderer = appearance.GetComponentsInChildren<MeshRenderer>().First(r => r.name != "Sweat");
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                Require(block.GetColor("_Color") == catalog.GetEntry(color).Tint, "Animation overwrote the chosen color.");
            }

            garage.EndPreview();
            var pieces = new List<RoadPiece>();

            for (int index = 0; index < 3; index++)
            {
                var obj = new GameObject("piece");
                obj.transform.SetParent(root.transform);
                var piece = obj.AddComponent<RoadPiece>();
                var entry = new GameObject("entry");
                entry.transform.SetParent(obj.transform);
                entry.transform.position = Vector3.forward * index * 10f;
                var exit = new GameObject("exit");
                exit.transform.SetParent(obj.transform);
                exit.transform.position = Vector3.forward * (index + 1) * 10f;
                var serialized = new SerializedObject(piece);
                serialized.FindProperty("_entryPoint").objectReferenceValue = entry.transform;
                serialized.FindProperty("_exitPoint").objectReferenceValue = exit.transform;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                pieces.Add(piece);
            }

            var path = new RoadPath(pieces);
            path.SetFinishWaypoint(2);
            path.BuildCurve(2);
            var definition = level.CurrentDefinition.Racer;
            var body = racer.GetComponent<Rigidbody>();

            foreach (float height in new[] { 1f, 100f })
            {
                racer.Prepare(path, definition);
                race.Prepare();
                race.StartRace();
                body.position = new Vector3(0f, height, 19f);
                racer.transform.position = body.position;
                racer.Tick(false, 0f);
                Require(!racer.IsFinished && race.State == RaceState.Racing, "Finish was counted early.");
                body.position = new Vector3(0f, height, 30f);
                racer.transform.position = body.position;
                racer.Tick(false, 0f);
                Require(racer.IsFinished && race.PlayerWon && race.Outcome == RaceOutcome.FinishedFirst, "Crossing finish and runout in one step must win, including in the air.");
            }

            racer.Prepare(path, definition);
            race.Prepare();
            race.StartRace();
            body.position = new Vector3(12.01f, 100f, 26f);
            racer.transform.position = body.position;
            racer.Tick(false, 0f);
            Require(!racer.IsFinished, "Passing outside the finish width must not win.");
            Require(results == 2, "Each completed crossing must report exactly one result.");
            Require((body.constraints & RigidbodyConstraints.FreezePositionY) != 0, "Flat-road movement must constrain vertical launches.");
            return "PASS: grouped colors survive animation; finish/runout crossing at ground and height 100; no off-road finish; exactly one result; vertical lock retained.";
        }
        finally
        {
            race.RaceEnded -= onResult;
            UnityEngine.Object.Destroy(root);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new Exception(message);
    }
}
