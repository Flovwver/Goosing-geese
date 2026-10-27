using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class RaceFlowTests
{
    public static string Main()
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var level = UnityEngine.Object.FindFirstObjectByType<Level>();
        var race = UnityEngine.Object.FindFirstObjectByType<RaceController>();
        var racer = (Racer)typeof(RaceController).GetField("_player", Flags).GetValue(race);
        var input = UnityEngine.Object.FindFirstObjectByType<InputSwitch>();
        var finish = UnityEngine.Object.FindFirstObjectByType<FinishLine>();
        var camera = UnityEngine.Object.FindFirstObjectByType<CameraFollower>();
        Action repeat = race.Retry;
        Action regenerate = level.BuildRoad;
        Action pause = () => { if (race.State == RaceState.Paused) race.Resume(); else race.Pause(); };
        var update = typeof(RaceController).GetMethod("Update", Flags);
        var trigger = typeof(FinishLine).GetMethod("OnTriggerEnter", Flags);
        var child = new GameObject("Finish regression collider");
        child.SetActive(false);
        child.transform.SetParent(racer.transform, false);
        var collider = child.AddComponent<BoxCollider>();
        int results = 0;
        Action<bool> completed = _ => results++;
        race.RaceEnded += completed;

        try
        {
            for (int cycle = 0; cycle < 10; cycle++)
            {
                repeat();
                Require(race.State == RaceState.Waiting && race.Outcome == RaceOutcome.None, "Repeat must prepare the race.");
                trigger.Invoke(finish, new object[] { collider });
                Require(!racer.IsFinished, "Finish must be ignored before the start.");
                var definition = level.CurrentDefinition;
                var profile = level.SpeedProfile;
                level.TryGetBuiltPath(out var path, out _);
                typeof(InputSwitch).GetField("_hasFocus", Flags).SetValue(input, true);
                typeof(InputSwitch).GetField("_isPaused", Flags).SetValue(input, false);
                input.SetPedalPressed(true);
                update.Invoke(race, null);
                update.Invoke(race, null);
                Require(race.State == RaceState.Racing, "Throttle must start the race.");
                pause();
                Require(race.State == RaceState.Paused && Time.timeScale == 0f, "Pause button must pause.");
                pause();
                Require(race.State == RaceState.Racing && Time.timeScale > 0f, "Pause button must resume.");

                if (cycle % 2 == 0)
                {
                    trigger.Invoke(finish, new object[] { collider });
                    trigger.Invoke(finish, new object[] { collider });
                }
                else
                {
                    racer.GetComponent<Rigidbody>().linearVelocity = Vector3.forward * racer.Configuration.MaxSpeed;
                    racer.Tick(false, racer.Configuration.TimeToOverheat + 0.1f);
                    trigger.Invoke(finish, new object[] { collider });
                }

                RaceOutcome expected = cycle % 2 == 0 ? RaceOutcome.FinishedFirst : RaceOutcome.Overheated;
                Require(race.State == RaceState.Result && race.Outcome == expected && results == cycle + 1, "Each attempt must produce one immutable result.");
                Require(!(bool)typeof(CameraFollower).GetField("_isFollowing", Flags).GetValue(camera), "Result must stop the camera.");
                input.SetPedalPressed(true);
                Require(!input.IsThrottlePressed, "Result must block throttle.");

                foreach (Vector3 point in path.Curve.Points)
                    racer.GetComponent<RoadNavigator>().UpdateTarget(point);

                Require(results == cycle + 1 && race.Outcome == expected, "Runout must not replace or duplicate results.");
                repeat();
                level.TryGetBuiltPath(out var repeated, out _);
                Require(ReferenceEquals(path, repeated) && ReferenceEquals(definition, level.CurrentDefinition) && ReferenceEquals(profile, level.SpeedProfile), "Repeat must retain level geometry and speed guidance.");
                Require((bool)typeof(CameraFollower).GetField("_isFollowing", Flags).GetValue(camera), "Repeat must restore the camera.");
                regenerate();
                Require(!ReferenceEquals(definition, level.CurrentDefinition) && race.State == RaceState.Waiting, "Regenerate button must create a new prepared level.");
            }

            var types = new[] { RoadPieceType.Straight, RoadPieceType.LeftTurn, RoadPieceType.RightTurn };

            for (int seed = 0; seed < 100; seed++)
            {
                var first = new RoadLayoutGenerator(types, 100, seed).Generate(20, 3);
                var second = new RoadLayoutGenerator(types, 100, seed).Generate(20, 3);
                Require(first.Length == 23 && first.SequenceEqual(second), "Saved seeds must reproduce complete layouts.");
            }

            return "PASS: ten attempts including five finishes/five overheats; child collider detection; immutable result; pause, input and camera recovery; repeat/regeneration; 100 reproducible seeds.";
        }
        finally
        {
            race.RaceEnded -= completed;
            UnityEngine.Object.Destroy(child);
            level.RestartRace();
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new Exception(message);
    }
}
