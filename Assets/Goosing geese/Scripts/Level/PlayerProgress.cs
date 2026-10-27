using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProgress
{
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

    public int GetUnlockedLevel(int stageCount)
    {
        return (int)Math.Max(1L, Math.Min((long)HighestCompletedLevel + 1L, Math.Max(1, stageCount)));
    }

    public bool TrySelectLevel(int number, int stageCount)
    {
        if (number < 1 || number > GetUnlockedLevel(stageCount))
            return false;

        Level = number;
        return true;
    }

    public int RecordRace(bool won, int number, int reward, int firstClearBonus)
    {
        if (number < 1 || (long)number > (long)HighestCompletedLevel + 1L)
            throw new ArgumentOutOfRangeException(nameof(number));

        QuestTracker.RecordRace(this, won, DateTime.Today);

        if (won == false)
            return 0;

        bool firstClear = number > HighestCompletedLevel;
        int earned = Mathf.Max(0, reward) + (firstClear ? Mathf.Max(0, firstClearBonus) : 0);
        Points += earned;
        Wallet += earned;
        HighestCompletedLevel = Mathf.Max(HighestCompletedLevel, number);
        return earned;
    }
}
