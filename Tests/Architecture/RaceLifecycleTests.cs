using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

public static class RaceLifecycleTests
{
    public static async Task<string> Main()
    {
        var level = UnityEngine.Object.FindFirstObjectByType<Level>();
        var race = UnityEngine.Object.FindFirstObjectByType<RaceController>();
        var racer = (Racer)typeof(RaceController).GetField("_player", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(race);
        var input = UnityEngine.Object.FindFirstObjectByType<InputSwitch>();
        var mover = racer.GetComponent<Mover>();
        var body = racer.GetComponent<Rigidbody>();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var update = typeof(RaceController).GetMethod("Update", flags);
        race.Resume();
        float originalTimeScale = Time.timeScale;

        try
        {
            level.RestartRace();
            AllowForegroundInput(input);
            input.SetPedalPressed(true);
            update.Invoke(race, null);
            Require(race.State == RaceState.Racing, "Pedal must start the race.");
            body.linearVelocity = Vector3.forward * 25f;
            race.enabled = false;
            Require(racer.GetCurrentSpeed() == 0f && !input.IsThrottlePressed, "Disable must stop motion and input.");
            Require(!(bool)typeof(Mover).GetField("_isAccelerate", flags).GetValue(mover), "Disable must clear the stored throttle command.");
            await Task.Delay(150);
            Require(racer.GetCurrentSpeed() < 0.01f, "Physics must not continue acceleration while the controller is disabled.");
            race.enabled = true;
            level.RestartRace();
            AllowForegroundInput(input);
            input.SetPedalPressed(true);
            update.Invoke(race, null);
            racer.FinishRace();
            Require(race.Outcome == RaceOutcome.FinishedFirst, "Finish setup must succeed.");
            race.enabled = false;
            level.RestartRace();
            Require(race.State == RaceState.Waiting && race.Outcome == RaceOutcome.None, "Explicit preparation must work while the controller is disabled.");
            input.SetPedalPressed(true);
            Require(!input.IsThrottlePressed, "A disabled controller must keep input blocked after preparation.");
            race.enabled = true;
            AllowForegroundInput(input);
            input.SetPedalPressed(true);
            update.Invoke(race, null);
            Require(race.State == RaceState.Racing, "A repeated race must start after re-enable.");
            Time.timeScale = 0.6f;
            race.Pause();
            race.enabled = false;
            Require(Mathf.Approximately(Time.timeScale, 0.6f) && racer.GetCurrentSpeed() == 0f, "Disable from pause must release only its own time-scale change and stop movement.");
            race.enabled = true;
            Require(race.State == RaceState.Paused && Time.timeScale == 0f, "Re-enable must preserve the paused state.");
            level.RestartRace();
            Require(race.State == RaceState.Waiting && Mathf.Approximately(Time.timeScale, 0.6f), "Preparation from pause must restore the prior time scale.");
            level.TryGetBuiltPath(out var path, out _);
            var navigator = racer.GetComponent<RoadNavigator>();

            foreach (Vector3 point in path.Curve.Points)
                navigator.UpdateTarget(point);

            Require(navigator.IsPathCompleted, "Navigation completion setup must succeed.");
            var definition = level.CurrentDefinition.Racer;
            racer.Prepare(path, new RacerDefinition(definition.Stats, definition.Navigation, new Vector3(1f, 2f, 3f), Quaternion.Euler(0f, 35f, 0f)));
            Require(!navigator.IsPathCompleted && racer.CanRace && racer.GetCurrentSpeed() == 0f && !racer.IsFinished, "One racer preparation operation must reset navigation, movement and local state.");
            return "PASS: controller disable during acceleration and pause; preparation while disabled; repeat after result; owned pause restoration; complete racer preparation.";
        }
        finally
        {
            race.enabled = true;
            level.RestartRace();
            Time.timeScale = originalTimeScale;
        }
    }

    private static void AllowForegroundInput(InputSwitch input)
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(InputSwitch).GetField("_hasFocus", Flags).SetValue(input, true);
        typeof(InputSwitch).GetField("_isPaused", Flags).SetValue(input, false);
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new Exception(message);
    }
}
