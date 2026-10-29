using System;
using System.Linq;
using UnityEngine;

public sealed class PlayerProgress
{
    private PlayerProgressData _data;

    public PlayerProgress() : this(new PlayerProgressData())
    {
    }

    public PlayerProgress(PlayerProgressData data, RacerAppearanceCatalog catalog = null)
    {
        Restore(data, catalog);
    }

    public string PlayerName => _data.PlayerName;
    public int Points => _data.Points;
    public int Wallet => _data.Wallet;
    public int Level => _data.Level;
    public int HighestCompletedLevel => _data.HighestCompletedLevel;
    public int Wins => _data.Wins;
    public int Races => _data.Races;
    public string DailyDate => _data.DailyDate;
    public int DailyRaces => _data.DailyRaces;
    public int DailyWins => _data.DailyWins;
    public bool DailyRacesClaimed => _data.DailyRacesClaimed;
    public bool DailyWinsClaimed => _data.DailyWinsClaimed;
    public bool GlobalWinsClaimed => _data.GlobalWinsClaimed;
    public float MusicVolume => _data.MusicVolume;
    public float EffectsVolume => _data.EffectsVolume;
    public bool English => _data.English;
    public int Appearance => _data.Appearance;
    public int Environment => _data.Environment;
    public string PlayerId => _data.PlayerId;
    public QuestTracker Quests { get; private set; }

    public string ToJson()
    {
        return JsonUtility.ToJson(_data);
    }

    public void RestoreJson(string json, RacerAppearanceCatalog catalog = null)
    {
        Restore(JsonUtility.FromJson<PlayerProgressData>(json), catalog);
    }

    public bool OwnsModel(string modelId)
    {
        return _data.OwnedModels.Contains(modelId);
    }

    public void GrantModel(string modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId) == false && OwnsModel(modelId) == false)
            _data.OwnedModels.Add(modelId);
    }

    public RacerUpgradeProgress GetUpgrades(string modelId)
    {
        RacerUpgradeProgress entry = _data.Upgrades.Find(item => item.ModelId == modelId);
        return entry == null ? null : JsonUtility.FromJson<RacerUpgradeProgress>(JsonUtility.ToJson(entry));
    }

    public bool TrySpend(int amount)
    {
        if (amount < 0 || amount > Wallet)
            return false;

        _data.Wallet -= amount;
        return true;
    }

    public bool TryBuyUpgrade(string modelId, RacerUpgradeKind kind, int nextLevel, int price)
    {
        if (Enum.IsDefined(typeof(RacerUpgradeKind), kind) == false || OwnsModel(modelId) == false || nextLevel < 1 || TrySpend(price) == false)
            return false;

        RacerUpgradeProgress entry = _data.Upgrades.Find(item => item.ModelId == modelId);

        if (entry == null)
        {
            entry = new RacerUpgradeProgress { ModelId = modelId };
            _data.Upgrades.Add(entry);
        }

        entry.SetLevel(kind, nextLevel);
        return true;
    }

    public void SelectAppearance(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        _data.Appearance = index;
    }

    public void SetName(string name)
    {
        _data.PlayerName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
    }

    public void SetMusicVolume(float value)
    {
        _data.MusicVolume = float.IsNaN(value) ? 0f : Mathf.Clamp01(value);
    }

    public void SetEffectsVolume(float value)
    {
        _data.EffectsVolume = float.IsNaN(value) ? 0f : Mathf.Clamp01(value);
    }

    public void ToggleLanguage()
    {
        _data.English = English == false;
    }

    public void NextScenery(int count)
    {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count));

        _data.Environment = (int)(((long)Mathf.Max(0, Environment) + 1) % count);
    }

    public void AddReward(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        _data.Points = (int)Math.Min(int.MaxValue, (long)Points + amount);
        _data.Wallet = (int)Math.Min(Points, (long)Wallet + amount);
    }

    public int GetUnlockedLevel(int stageCount)
    {
        return (int)Math.Max(1L, Math.Min((long)HighestCompletedLevel + 1L, Math.Max(1, stageCount)));
    }

    public bool TrySelectLevel(int number, int stageCount)
    {
        if (number < 1 || number > GetUnlockedLevel(stageCount))
            return false;

        _data.Level = number;
        return true;
    }

    public int RecordRace(bool won, int number, int reward, int firstClearBonus)
    {
        if (number < 1 || (long)number > (long)HighestCompletedLevel + 1L)
            throw new ArgumentOutOfRangeException(nameof(number));

        Quests.RecordRace(won, DateTime.Today);

        if (won == false)
            return 0;

        bool firstClear = number > HighestCompletedLevel;
        int earned = (int)Math.Min(int.MaxValue, (long)Mathf.Max(0, reward) + (firstClear ? Mathf.Max(0, firstClearBonus) : 0));
        AddReward(earned);
        _data.HighestCompletedLevel = Mathf.Max(HighestCompletedLevel, number);
        return earned;
    }

    private void Restore(PlayerProgressData data, RacerAppearanceCatalog catalog)
    {
        _data = data == null ? new PlayerProgressData() : JsonUtility.FromJson<PlayerProgressData>(JsonUtility.ToJson(data));
        _data.Points = Mathf.Max(0, _data.Points);
        _data.Wallet = Mathf.Clamp(_data.WalletInitialized ? _data.Wallet : _data.Points, 0, _data.Points);
        _data.WalletInitialized = true;
        _data.Level = Mathf.Max(1, _data.Level);
        _data.HighestCompletedLevel = Mathf.Max(0, _data.CampaignInitialized ? _data.HighestCompletedLevel : _data.Level - 1);
        _data.CampaignInitialized = true;
        _data.Level = Mathf.Min(_data.Level, GetUnlockedLevel(int.MaxValue));
        _data.PlayerId = string.IsNullOrWhiteSpace(_data.PlayerId) ? Guid.NewGuid().ToString("N") : _data.PlayerId;
        SetName(_data.PlayerName);
        SetMusicVolume(_data.MusicVolume);
        SetEffectsVolume(_data.EffectsVolume);
        _data.Appearance = catalog != null ? Mathf.Clamp(_data.Appearance, 0, Mathf.Max(0, catalog.Count - 1)) : Mathf.Max(0, _data.Appearance);
        _data.Environment = Mathf.Max(0, _data.Environment);
        _data.Upgrades = (_data.Upgrades ?? new System.Collections.Generic.List<RacerUpgradeProgress>()).Where(item => item != null && string.IsNullOrWhiteSpace(item.ModelId) == false).GroupBy(item => item.ModelId).Select(group => group.First()).ToList();
        _data.OwnedModels = (_data.OwnedModels ?? new System.Collections.Generic.List<string>()).Where(id => string.IsNullOrWhiteSpace(id) == false).Distinct().ToList();
        GrantModel("goose");

        if (_data.OwnershipInitialized == false)
        {
            if (catalog != null && catalog.Count > 0)
                GrantModel(catalog.GetEntry(_data.Appearance).UpgradeId);

            foreach (RacerUpgradeProgress upgrade in _data.Upgrades)
                GrantModel(upgrade.ModelId);

            _data.OwnershipInitialized = catalog != null;
        }

        Quests = new QuestTracker(_data, AddReward);
        Quests.RefreshDay(DateTime.Today);
    }
}
