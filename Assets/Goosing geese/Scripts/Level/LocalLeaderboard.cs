using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class LocalLeaderboard
{
    private const string SaveKey = "GoosingGeese.Leaderboard.v1";
    private const int MaximumEntries = 100;

    public List<LeaderboardEntry> Read()
    {
        LeaderboardData table = null;

        try
        {
            table = JsonUtility.FromJson<LeaderboardData>(PlayerPrefs.GetString(SaveKey, ""));
        }
        catch (ArgumentException)
        {
            table = null;
        }

        var entries = table != null && table.Entries != null ? table.Entries : new List<LeaderboardEntry>();
        entries.RemoveAll(entry => entry == null || string.IsNullOrWhiteSpace(entry.Name));
        entries.Sort((first, second) => second.Score.CompareTo(first.Score));
        return entries;
    }

    public void Record(string name, int score, string playerId = null)
    {
        name = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
        var entries = Read();
        LeaderboardEntry entry = entries.Find(item => string.IsNullOrEmpty(playerId) ? item.Name == name : item.PlayerId == playerId || (string.IsNullOrEmpty(item.PlayerId) && item.Name == name));

        if (entry == null)
            entries.Add(new LeaderboardEntry { PlayerId = playerId, Name = name, Score = Mathf.Max(0, score) });
        else
        {
            entry.PlayerId = playerId;
            entry.Name = name;
            entry.Score = Mathf.Max(entry.Score, score);
        }

        entries.Sort((first, second) => second.Score.CompareTo(first.Score));

        if (entries.Count > MaximumEntries)
            entries.RemoveRange(MaximumEntries, entries.Count - MaximumEntries);

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(new LeaderboardData { Entries = entries }));
        PlayerPrefs.Save();
    }

    public int GetRank(List<LeaderboardEntry> entries, int index)
    {
        int rank = 1;

        for (int i = 0; i < index; i++)
        {
            if (entries[i].Score > entries[index].Score)
                rank++;
        }

        return rank;
    }
}
