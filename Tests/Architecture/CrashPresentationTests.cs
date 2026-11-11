using System;
using System.Reflection;
using GoosingGeese.Presentation;
using GoosingGeese.Racing;
using UnityEngine;

public static class CrashPresentationTests
{
    public static string Main()
    {
        var race = UnityEngine.Object.FindFirstObjectByType<RaceController>();
        var result = UnityEngine.Object.FindFirstObjectByType<RaceResultView>();
        var view = (RacerOverheatView)typeof(RaceResultView).GetField("_overheatView", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(result);
        var animator = view.GetComponent<Animator>();
        var appearance = view.GetComponentInParent<RacerAppearance>();

        try
        {
            race.Retry();
            race.StartRace();
            race.EndRace(RaceOutcome.OffRoad);

            if (race.Outcome != RaceOutcome.OffRoad
                || view.HasExploded == false
                || animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Explosion") == false)
                throw new Exception($"Off-road defeat must play the explosion. Outcome: {race.Outcome}, active: {view.HasExploded}, state hash: {animator.GetCurrentAnimatorStateInfo(0).fullPathHash}.");

            typeof(RacerAppearance).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(appearance, null);

            if (view.GetComponent<MeshRenderer>().enabled)
                throw new Exception("The visible goose must disappear during the explosion on its child animator.");

            race.Retry();

            if (view.HasExploded)
                throw new Exception("Retry must reset the explosion view.");

            return "PASS: off-road defeat plays the explosion and retry resets it.";
        }
        finally
        {
            race.Retry();
        }
    }
}
