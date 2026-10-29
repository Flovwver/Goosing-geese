using System;
using System.Collections.Generic;
using GoosingGeese.Garage;
using GoosingGeese.Levels;

namespace GoosingGeese.Profile
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "PlayerProgressData")]
    public sealed class PlayerProgressData
    {
        public string PlayerName = "Player";
        public int Points;
        public int Wallet;
        public bool WalletInitialized;
        public List<RacerUpgradeProgress> Upgrades = new List<RacerUpgradeProgress>();
        public List<string> OwnedModels = new List<string>
        {
            "goose"
        };
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
        public string PlayerId;
    }
}
