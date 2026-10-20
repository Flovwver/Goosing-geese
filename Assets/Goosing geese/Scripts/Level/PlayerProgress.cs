using System;
using UnityEngine;

[Serializable]
public class PlayerProgress
{
    private const string SaveKey = "GoosingGeese.Profile.v1";
    private const int WinReward = 100;
    private const int DailyRaceTarget = 3;
    private const int DailyWinTarget = 1;
    private const int GlobalWinTarget = 10;
    private const int DailyRaceReward = 50;
    private const int DailyWinReward = 75;
    private const int GlobalWinReward = 300;

    public string PlayerName = "Player";
    public int Points;
    public int Level = 1;
    public int Wins;
    public int Races;
    public string DailyDate;
    public int DailyRaces;
    public int DailyWins;
    public bool DailyRacesClaimed;
    public bool DailyWinsClaimed;
    public bool GlobalWinsClaimed;
    public float MusicVolume = 0.3f;
    public float EffectsVolume = 0.7f;
    public bool English;
    public int Appearance;
    public int Environment;

    public static PlayerProgress Load()
    {
        PlayerProgress progress = null;

        try
        {
            progress = JsonUtility.FromJson<PlayerProgress>(PlayerPrefs.GetString(SaveKey, ""));
        }
        catch (ArgumentException)
        {
            progress = null;
        }

        if (progress == null)
            progress = new PlayerProgress();

        progress.Points = Mathf.Max(0, progress.Points);
        progress.Level = Mathf.Max(1, progress.Level);
        progress.MusicVolume = Mathf.Clamp01(progress.MusicVolume);
        progress.EffectsVolume = Mathf.Clamp01(progress.EffectsVolume);
        progress.RefreshDay();
        return progress;
    }

    public int RecordRace(bool won)
    {
        RefreshDay();
        Races++;
        DailyRaces++;

        if (won == false)
        {
            Save();
            return 0;
        }

        Wins++;
        DailyWins++;
        Points += WinReward;
        Level++;
        Save();
        return WinReward;
    }

    public int GetQuestProgress(int quest)
    {
        RefreshDay();
        return quest == 0 ? DailyRaces : quest == 1 ? DailyWins : Wins;
    }

    public int GetQuestTarget(int quest)
    {
        return quest == 0 ? DailyRaceTarget : quest == 1 ? DailyWinTarget : GlobalWinTarget;
    }

    public bool IsQuestClaimed(int quest)
    {
        return quest == 0 ? DailyRacesClaimed : quest == 1 ? DailyWinsClaimed : GlobalWinsClaimed;
    }

    public bool ClaimQuest(int quest)
    {
        if (quest < 0 || quest > 2 || IsQuestClaimed(quest) || GetQuestProgress(quest) < GetQuestTarget(quest))
            return false;

        int reward = quest == 0 ? DailyRaceReward : quest == 1 ? DailyWinReward : GlobalWinReward;
        Points += reward;

        if (quest == 0)
            DailyRacesClaimed = true;
        else if (quest == 1)
            DailyWinsClaimed = true;
        else
            GlobalWinsClaimed = true;

        Save();
        return true;
    }

    public void Save()
    {
        LocalLeaderboard.Record(PlayerName, Points);
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(this));
        PlayerPrefs.Save();
    }

    private void RefreshDay()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");

        if (DailyDate == today)
            return;

        DailyDate = today;
        DailyRaces = 0;
        DailyWins = 0;
        DailyRacesClaimed = false;
        DailyWinsClaimed = false;
    }
}
