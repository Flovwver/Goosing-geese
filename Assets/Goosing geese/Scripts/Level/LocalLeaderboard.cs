using System;
using System.Collections.Generic;
using UnityEngine;

public static class LocalLeaderboard
{
    private const string SaveKey = "GoosingGeese.Leaderboard.v1";
    private const int MaximumEntries = 100;

    public static List<Entry> Read()
    {
        Table table = null;

        try
        {
            table = JsonUtility.FromJson<Table>(PlayerPrefs.GetString(SaveKey, ""));
        }
        catch (ArgumentException)
        {
            table = null;
        }

        var entries = table != null && table.Entries != null ? table.Entries : new List<Entry>();
        entries.RemoveAll(entry => entry == null || string.IsNullOrWhiteSpace(entry.Name));
        entries.Sort((first, second) => second.Score.CompareTo(first.Score));
        return entries;
    }

    public static void Record(string name, int score)
    {
        name = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
        var entries = Read();
        Entry entry = entries.Find(item => item.Name == name);

        if (entry == null)
            entries.Add(new Entry { Name = name, Score = Mathf.Max(0, score) });
        else
            entry.Score = Mathf.Max(entry.Score, score);

        entries.Sort((first, second) => second.Score.CompareTo(first.Score));

        if (entries.Count > MaximumEntries)
            entries.RemoveRange(MaximumEntries, entries.Count - MaximumEntries);

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(new Table { Entries = entries }));
        PlayerPrefs.Save();
    }

    public static int GetRank(List<Entry> entries, int index)
    {
        int rank = 1;

        for (int i = 0; i < index; i++)
        {
            if (entries[i].Score > entries[index].Score)
                rank++;
        }

        return rank;
    }

    [Serializable]
    public class Entry
    {
        public string Name;
        public int Score;
    }

    [Serializable]
    private class Table
    {
        public List<Entry> Entries;
    }
}
