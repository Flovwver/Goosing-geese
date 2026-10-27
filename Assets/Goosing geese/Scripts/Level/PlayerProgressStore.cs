using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerProgressStore
{
    private const string SaveKey = "GoosingGeese.Profile.v1";

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
        QuestTracker.RefreshDay(progress, DateTime.Today);
        return progress;
    }

    public static void Save(PlayerProgress progress)
    {
        if (progress == null)
            throw new ArgumentNullException(nameof(progress));

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(progress));
        PlayerPrefs.Save();
    }
}
