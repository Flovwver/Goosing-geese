using System;
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
            QuestTracker.RecordRace(progress, true, yesterday);
            Require(QuestTracker.TryClaim(progress, 1, yesterday), "Completed daily win must pay.");
            Require(!QuestTracker.TryClaim(progress, 1, yesterday), "Daily win must pay once.");
            string beforeRead = JsonUtility.ToJson(progress);
            Require(QuestTracker.GetProgress(progress, 1, today) == 0, "Yesterday's wins must not count today.");
            Require(!QuestTracker.IsClaimed(progress, 1, today), "Yesterday's claim must not block today.");
            Require(JsonUtility.ToJson(progress) == beforeRead, "Reading quests must not mutate the profile.");
            Require(!QuestTracker.TryClaim(progress, 1, today), "A new day must not pay for yesterday's win.");
            QuestTracker.RecordRace(progress, true, today);
            Require(QuestTracker.TryClaim(progress, 1, today), "Today's completed quest must pay after rollover.");
            Require(progress.Points == 150 && progress.Wallet == 150, "Exactly two daily rewards expected.");
            Require(!QuestTracker.TryClaim(progress, -1, today) && !QuestTracker.TryClaim(progress, 3, today), "Invalid quest ids must not pay.");
            PlayerProgressStore.Save(progress);
            Require(PlayerPrefs.GetString(LeaderboardKey) == leaderboardJson, "Saving settings/profile must not write the leaderboard.");
            PlayerProgress loaded = PlayerProgressStore.Load();
            Require(loaded.Points == progress.Points && loaded.Wallet == progress.Wallet, "Profile round trip must retain rewards.");
            PlayerPrefs.SetString(ProfileKey, "{\"Points\":100,\"Level\":4}");
            loaded = PlayerProgressStore.Load();
            Require(loaded.Wallet == 100 && loaded.HighestCompletedLevel == 3, "Legacy wallet/campaign migration must survive extraction.");
            PlayerPrefs.SetString(ProfileKey, "corrupt");
            loaded = PlayerProgressStore.Load();
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
        if (!condition)
            throw new Exception(message);
    }
}
