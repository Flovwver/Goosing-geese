using System;
using GoosingGeese.Levels;
using GoosingGeese.Profile;
using UnityEngine;

public static class ProfileArchitectureTests
{
    public static string Main()
    {
        const string ProfileKey = "GoosingGeese.Profile.v1";
        const string LeaderboardKey = "GoosingGeese.Leaderboard.v1";
        bool hadProfile = PlayerPrefs.HasKey(ProfileKey);
        bool hadLeaderboard = PlayerPrefs.HasKey(LeaderboardKey);
        string profileJson = PlayerPrefs.GetString(ProfileKey);
        string leaderboardJson = PlayerPrefs.GetString(LeaderboardKey);

        try
        {
            var yesterday = new DateTime(2026, 10, 26);
            var today = yesterday.AddDays(1);
            var progress = new PlayerProgress();
            progress.Quests.RecordRace(true, yesterday);
            Require(progress.Quests.TryClaim(1, yesterday), "Completed daily win must pay.");
            Require((progress.Quests.TryClaim(1, yesterday) == false), "Daily win must pay once.");
            string beforeRead = progress.ToJson();
            Require(progress.Quests.GetProgress(1, today) == 0, "Yesterday's wins must not count today.");
            Require((progress.Quests.IsClaimed(1, today) == false), "Yesterday's claim must not block today.");
            Require(progress.ToJson() == beforeRead, "Reading quests must not mutate the profile.");
            Require((progress.Quests.TryClaim(1, today) == false), "A new day must not pay for yesterday's win.");
            progress.Quests.RecordRace(true, today);
            Require(progress.Quests.TryClaim(1, today), "Today's completed quest must pay after rollover.");
            Require(progress.Points == 150 && progress.Balance == 150, "Exactly two daily rewards expected.");
            Require((progress.Quests.TryClaim(-1, today) == false) && (progress.Quests.TryClaim(3, today) == false), "Invalid quest ids must not pay.");
            new PlayerProgressStore().Save(progress);
            Require(PlayerPrefs.GetString(LeaderboardKey) == leaderboardJson, "Saving settings/profile must not write the leaderboard.");
            PlayerProgress loaded = new PlayerProgressStore().Load();
            Require(loaded.Points == progress.Points && loaded.Balance == progress.Balance, "Profile round trip must retain rewards.");
            PlayerPrefs.SetString(ProfileKey, "{\"Points\":100,\"Level\":4}");
            loaded = new PlayerProgressStore().Load();
            Require(loaded.Balance == 100 && loaded.HighestCompletedLevel == 3, "Legacy wallet/campaign migration must survive extraction.");
            PlayerPrefs.SetString(ProfileKey, "corrupt");
            loaded = new PlayerProgressStore().Load();
            Require(loaded.Level == 1 && loaded.Points == 0, "Corrupt saves must use a fresh profile.");
            return "PASS: read-only quest queries, day rollover, one reward, invalid ids, profile round trip, legacy migration, corrupt save, leaderboard isolation.";
        }
        finally
        {
            Restore(ProfileKey, hadProfile, profileJson);
            Restore(LeaderboardKey, hadLeaderboard, leaderboardJson);
            PlayerPrefs.Save();
        }
    }

    private static void Restore(string key, bool existed, string value)
    {
        if (existed)
            PlayerPrefs.SetString(key, value);
        else
            PlayerPrefs.DeleteKey(key);
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }
}
