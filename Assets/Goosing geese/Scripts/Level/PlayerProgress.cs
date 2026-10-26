using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProgress
{
    private const string SaveKey = "GoosingGeese.Profile.v1";
    private const int DailyRaceTarget = 3;
    private const int DailyWinTarget = 1;
    private const int GlobalWinTarget = 10;
    private const int DailyRaceReward = 50;
    private const int DailyWinReward = 75;
    private const int GlobalWinReward = 300;

    public string PlayerName = "Player";
    public int Points;
    public int Wallet;
    public bool WalletInitialized;
    public List<RacerUpgradeProgress> Upgrades = new List<RacerUpgradeProgress>();
    public List<string> OwnedModels = new List<string> { "goose" };
    public bool OwnershipInitialized;
    public int Level = 1;
    public int HighestCompletedLevel;
    public bool CampaignInitialized;
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

        if (progress.WalletInitialized == false)
        {
            progress.Wallet = progress.Points;
            progress.WalletInitialized = true;
        }

        progress.Wallet = Mathf.Clamp(progress.Wallet, 0, progress.Points);

        if (progress.Upgrades == null)
            progress.Upgrades = new List<RacerUpgradeProgress>();

        progress.Level = Mathf.Max(1, progress.Level);

        if (progress.CampaignInitialized == false)
        {
            progress.HighestCompletedLevel = progress.Level - 1;
            progress.CampaignInitialized = true;
        }

        progress.HighestCompletedLevel = Mathf.Max(0, progress.HighestCompletedLevel);
        progress.MusicVolume = Mathf.Clamp01(progress.MusicVolume);
        progress.EffectsVolume = Mathf.Clamp01(progress.EffectsVolume);
        progress.RefreshDay();
        return progress;
    }

    public int GetUnlockedLevel(int stageCount)
    {
        return (int)Math.Max(1L, Math.Min((long)HighestCompletedLevel + 1L, Math.Max(1, stageCount)));
    }

    public bool TrySelectLevel(int number, int stageCount)
    {
        if (number < 1 || number > GetUnlockedLevel(stageCount))
            return false;

        Level = number;
        Save();
        return true;
    }

    public int RecordRace(bool won, int number, int reward, int firstClearBonus)
    {
        if (number < 1 || (long)number > (long)HighestCompletedLevel + 1L)
            throw new ArgumentOutOfRangeException(nameof(number));

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
        bool firstClear = number > HighestCompletedLevel;
        int earned = Mathf.Max(0, reward) + (firstClear ? Mathf.Max(0, firstClearBonus) : 0);
        Points += earned;
        Wallet += earned;
        HighestCompletedLevel = Mathf.Max(HighestCompletedLevel, number);
        Save();
        return earned;
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
        Wallet += reward;

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
