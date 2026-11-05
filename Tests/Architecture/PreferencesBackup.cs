using System;
using System.IO;
using GoosingGeese.Racing;
using UnityEngine;

public static class PreferencesBackup
{
    public static string Capture()
    {
        string path = BackupPath();

        if (File.Exists(path))
            throw new InvalidOperationException("A preferences backup already exists. Restore it before starting another run: " + path);

        var data = new BackupData();
        var race = UnityEngine.Object.FindFirstObjectByType<RaceController>();
        data.LiveProfile = Application.isPlaying && race?.Progress != null ? race.Progress.ToJson() : null;
        data.HasProfile = PlayerPrefs.HasKey(data.ProfileKey);
        data.HasLeaderboard = PlayerPrefs.HasKey(data.LeaderboardKey);
        data.Profile = PlayerPrefs.GetString(data.ProfileKey);
        data.Leaderboard = PlayerPrefs.GetString(data.LeaderboardKey);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(data));
        return "Preferences backup saved.";
    }

    public static string Restore()
    {
        string path = BackupPath();
        var data = JsonUtility.FromJson<BackupData>(File.ReadAllText(path));
        var race = UnityEngine.Object.FindFirstObjectByType<RaceController>();

        if (Application.isPlaying && race?.Progress != null && data.LiveProfile != null)
            race.Progress.RestoreJson(data.LiveProfile);

        RestoreKey(data.ProfileKey, data.HasProfile, data.Profile);
        RestoreKey(data.LeaderboardKey, data.HasLeaderboard, data.Leaderboard);
        PlayerPrefs.Save();
        File.Delete(path);
        return "Original preferences restored.";
    }

    private static string BackupPath()
    {
        return Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Temp", "ArchitecturePreferences.json");
    }

    private static void RestoreKey(string key, bool existed, string value)
    {
        if (existed)
            PlayerPrefs.SetString(key, value);
        else
            PlayerPrefs.DeleteKey(key);
    }

    [Serializable]
    private sealed class BackupData
    {
        public string ProfileKey = "GoosingGeese.Profile.v1";
        public string LeaderboardKey = "GoosingGeese.Leaderboard.v1";
        public bool HasProfile;
        public bool HasLeaderboard;
        public string Profile;
        public string Leaderboard;
        public string LiveProfile;
    }
}
