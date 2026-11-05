using System;
using System.Linq;
using GoosingGeese.Levels;
using GoosingGeese.Profile;
using GoosingGeese.Racing;
using UnityEngine;

public static class ResultAndIdentityTests
{
    public static string Main()
    {
        var race = UnityEngine.Object.FindFirstObjectByType<RaceController>();
        var level = race.GetComponent<Level>();
        int callbacks = 0;
        Action reenter = () => race.EndRace(RaceOutcome.FinishedFirst);
        Action<bool> observe = won => callbacks++;

        try
        {
            level.RestartRace();
            race.StartRace();
            race.Progress.Changed += reenter;
            race.RaceEnded += observe;
            race.EndRace(RaceOutcome.FinishedFirst);
            Require(callbacks == 1 && race.State == RaceState.Result, "Result must commit once even when profile observers reenter.");
            var leaderboard = new LocalLeaderboard();
            string id = Guid.NewGuid().ToString("N");
            leaderboard.Record("Before rename", 100, id);
            leaderboard.Record("After rename", 50, id);
            var entries = leaderboard.Read().Where(entry => entry.PlayerId == id).ToArray();
            Require(entries.Length == 1
                && entries[0].Name == "After rename"
                && entries[0].Score == 100,
                "Renaming must retain one personal best by stable id.");
            return "PASS: reentrant result commits once; leaderboard identity survives renaming.";
        }
        finally
        {
            race.Progress.Changed -= reenter;
            race.RaceEnded -= observe;
            level.RestartRace();
        }
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }
}
