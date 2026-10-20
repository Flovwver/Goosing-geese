using System;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class RacerGarage : MonoBehaviour
{
    [SerializeField] private RaceController _race;
    [SerializeField] private RacerAppearanceCatalog _appearances;
    [SerializeField] private RacerUpgradeRules _rules;

    public event Action SelectionChanged;
    public event Action UpgradesChanged;

    public int SelectedIndex => Mathf.Clamp(_race.Progress.Appearance, 0, _appearances.Count - 1);
    public int ModelCount => _appearances.Count;
    public int AvailablePoints => _race.Progress.Wallet;
    public int TotalPoints => _race.Progress.Points;
    public int MaximumUpgradeLevel => _rules.MaximumLevel;
    public RacerAppearanceCatalog.Entry SelectedEntry => _appearances.GetEntry(SelectedIndex);
    public RacerStats BaseStats => SelectedEntry.Stats;

    public bool Select(int index)
    {
        if (_race.State != RaceState.Waiting || index < 0 || index >= ModelCount)
            return false;

        if (SelectedIndex == index)
            return true;

        _race.Progress.Appearance = index;
        _race.Progress.Save();
        SelectionChanged?.Invoke();
        return true;
    }

    public int GetUpgradeLevel(RacerUpgradeKind kind)
    {
        RacerUpgradeProgress progress = FindUpgrades();
        return Mathf.Clamp(progress != null ? progress.GetLevel(kind) : 0, 0, MaximumUpgradeLevel);
    }

    public int GetUpgradePrice(RacerUpgradeKind kind)
    {
        return _rules.GetPrice(GetUpgradeLevel(kind));
    }

    public float GetUpgradeValue(RacerUpgradeKind kind, int additionalLevels = 0)
    {
        return _rules.GetValue(BaseStats, kind, GetUpgradeLevel(kind) + additionalLevels);
    }

    public bool CanUpgrade(RacerUpgradeKind kind)
    {
        if ((int)kind < 0 || (int)kind > (int)RacerUpgradeKind.Handling || _race.State != RaceState.Waiting)
            return false;

        return GetUpgradeLevel(kind) < MaximumUpgradeLevel
            && AvailablePoints >= GetUpgradePrice(kind)
            && GetUpgradeValue(kind, 1) > GetUpgradeValue(kind);
    }

    public bool TryUpgrade(RacerUpgradeKind kind)
    {
        if (CanUpgrade(kind) == false)
            return false;

        int price = GetUpgradePrice(kind);
        RacerUpgradeProgress progress = FindUpgrades();

        if (progress == null)
        {
            progress = new RacerUpgradeProgress { ModelId = SelectedEntry.UpgradeId };
            _race.Progress.Upgrades.Add(progress);
        }

        progress.SetLevel(kind, GetUpgradeLevel(kind) + 1);
        _race.Progress.Wallet -= price;
        _race.Progress.Save();
        UpgradesChanged?.Invoke();
        return true;
    }

    public void ApplyStats(RacerStats target)
    {
        _rules.Apply(BaseStats, FindUpgrades(), target);
    }

    private RacerUpgradeProgress FindUpgrades()
    {
        string modelId = SelectedEntry.UpgradeId;
        return _race.Progress.Upgrades.Find(progress => progress != null && progress.ModelId == modelId);
    }
}
