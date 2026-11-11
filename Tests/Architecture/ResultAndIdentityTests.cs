using System;
using System.Linq;
using GoosingGeese.Levels;
using GoosingGeese.Presentation;
using GoosingGeese.Profile;
using GoosingGeese.Racing;
using UnityEngine;
using UnityEngine.UI;

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
            VerifyCurrentPlayerRow(race.Progress);
            return "PASS: reentrant result commits once; leaderboard identity survives renaming; an out-of-top-five player has a separate row.";
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

    private static void VerifyCurrentPlayerRow(PlayerProgress progress)
    {
        var entries = Enumerable.Range(1,
            5).Select(rank => new LeaderboardEntry { PlayerId = "leader-"
            + rank, Name = "Leader "
            + rank, Rank = rank, Score = 1000 - rank }).ToList();
        entries.Add(new LeaderboardEntry { PlayerId = progress.PlayerId, Name = "Current player", Rank = 20, Score = 100 });
        var parent = new GameObject("Leaderboard regression", typeof(RectTransform));

        try
        {
            var view = new LeaderboardView(progress, new GameUiFactory(GameUiTheme.LoadFont(), null));
            view.Render(parent.transform, entries, () =>
            {
            }, true);
            Text playerRow = parent.GetComponentsInChildren<Text>().Single(label => label.text == "20. Current player — 100 •");
            Require(playerRow.resizeTextForBestFit
                && playerRow.GetComponent<LayoutElement>().preferredHeight >= playerRow.fontSize,
                "The current player must have independent space outside the five top rows.");
        }
        finally
        {
            UnityEngine.Object.Destroy(parent);
        }
    }
}
