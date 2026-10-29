using System;
using GoosingGeese.Garage;
using GoosingGeese.Profile;
using UnityEngine;

public static class ProfileContractsTests
{
    public static string Main()
    {
        var data = new PlayerProgressData
        {
            Points = int.MaxValue - 1,
            Wallet = 10,
            WalletInitialized = true
        };
        var profile = new PlayerProgress(data);
        var raceProfile = new PlayerProgress();
        int notifications = 0;
        int observedStage = -1;
        int observedRaces = -1;
        int observedBalance = -1;
        raceProfile.Changed += () =>
        {
            notifications++;
            observedStage = raceProfile.HighestCompletedLevel;
            observedRaces = raceProfile.Races;
            observedBalance = raceProfile.Balance;
        };
        raceProfile.RecordRace(true, 1, 100, 20);
        Require(notifications == 1
            && observedStage == 1
            && observedRaces == 1
            && observedBalance == 120,
            "Win observers must receive one notification with the complete result.");
        raceProfile.RecordRace(false, 1, 100, 20);
        Require(notifications == 2 && observedRaces == 2 && observedBalance == 120, "Loss observers must receive updated counters without a reward.");
        data.Points = 0;
        Require(profile.Points == int.MaxValue - 1, "Model must own a copy of save data.");
        profile.AddReward(100);
        Require(profile.Points == int.MaxValue && profile.Balance == 110, "Rewards must saturate without wrapping.");
        Require(profile.TrySpend(-1) == false && profile.TrySpend(111) == false, "Invalid payments must not change balance.");
        Require(profile.TrySpend(10) && profile.Balance == 100, "Payment must be explicit and atomic.");
        string id = profile.PlayerId;
        profile.SetName("Renamed");
        profile.RestoreJson(profile.ToJson());
        Require(profile.PlayerId == id, "Renaming and restoring must preserve identity.");
        profile.GrantModel("car");
        Require(profile.TryBuyUpgrade("car", RacerUpgradeKind.TopSpeed, 1, 25), "Owned upgrade purchase must work.");
        var upgrades = profile.GetUpgrades("car");
        upgrades.TopSpeed = 99;
        Require(profile.GetUpgrades("car").TopSpeed == 1, "Upgrade queries must not expose mutable state.");
        Require(profile.TryBuyUpgrade("car", (RacerUpgradeKind)99, 2, 25) == false && profile.Balance == 75, "Invalid upgrade must not spend currency.");
        return "PASS: DTO isolation; saturating rewards; atomic spending; stable identity; defensive upgrade reads.";
    }

    private static void Require(bool condition, string message)
    {
        if (condition == false)
            throw new Exception(message);
    }
}
