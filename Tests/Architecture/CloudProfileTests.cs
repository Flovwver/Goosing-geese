using System;
using System.Collections.Generic;
using GoosingGeese.Infrastructure;
using GoosingGeese.Profile;
using UnityEngine;

public static class CloudProfileTests
{
    public static string Main()
    {
        var platform = new FakeCloud("test-" + Guid.NewGuid().ToString("N"));
        string key = "GoosingGeese.Profile.v1." + platform.ProfileScope;
        var store = new PlayerProgressStore(platform);

        try
        {
            platform.Snapshot = JsonUtility.ToJson(new PlayerProgressData { PlayerId = "cloud-player", PlayerName = "Cloud", Points = 42, SavedAtUnixMs = 100 });
            PlayerProgress progress = store.Load();
            Require(progress.PlayerName == "Cloud" && progress.Points == 42 && PlayerPrefs.HasKey(key), "Restore must cache the newer cloud snapshot locally.");
            progress.SetName("Local edit");
            store.Save(progress);
            string saved = PlayerPrefs.GetString(key);
            Require(platform.LastWritten == saved, "Local and cloud saves must use the same snapshot.");
            Require(store.Load().PlayerName == "Local edit", "Older cloud data must not erase a newer local save.");
            store.Save(progress);
            Require(platform.LastWritten == saved, "Saving unchanged state must not advance the save timestamp.");
            platform.Snapshot = "{\"Level\":1,\"SavedAtUnixMs\":9999999999999}";
            Require(store.Load().PlayerName == "Local edit", "Invalid cloud profiles must not replace local data.");
            return "PASS: cloud restore, account cache, conflict handling and local-first persistence.";
        }
        finally
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }

    private sealed class FakeCloud : IGamePlatform
    {
        public FakeCloud(string scope)
        {
            ProfileScope = scope;
        }

        public string Language => null;
        public string ProfileScope { get; }
        public string Snapshot { get; set; }
        public string LastWritten { get; private set; }

        public string ReadCloudProfile()
        {
            return Snapshot;
        }

        public void ReportGameplay(bool isReady, bool isActive)
        {
        }

        public void RequestLeaderboard(string localPlayerId, Action<List<LeaderboardEntry>> onLoaded)
        {
        }

        public void SetLanguage(string language)
        {
        }

        public void SubmitScore(int score)
        {
        }

        public void WriteCloudProfile(string json)
        {
            LastWritten = json;
        }
    }
}
