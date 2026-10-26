using System;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class RacerGarage : MonoBehaviour
{
    [SerializeField] private RaceController _race;
    [SerializeField] private RacerAppearanceCatalog _appearances;
    [SerializeField] private RacerUpgradeRules _rules;

    private int _previewIndex = -1;

    public event Action SelectionChanged;
    public event Action PreviewChanged;
    public event Action UpgradesChanged;

    public int SelectedIndex => Mathf.Clamp(_race.Progress.Appearance, 0, _appearances.Count - 1);
    public int PreviewIndex => _previewIndex >= 0 ? _previewIndex : SelectedIndex;
    public int AppearanceCount => _appearances.Count;
    public int AvailablePoints => _race.Progress.Wallet;
    public int TotalPoints => _race.Progress.Points;
    public int HighestCompletedLevel => _race.Progress.HighestCompletedLevel;
    public int MaximumUpgradeLevel => _rules.MaximumLevel;
    public RacerAppearanceCatalog.Entry SelectedEntry => _appearances.GetEntry(SelectedIndex);
    public RacerAppearanceCatalog.Entry PreviewEntry => _appearances.GetEntry(PreviewIndex);
    public RacerStats BaseStats => SelectedEntry.Stats;

    private void Awake()
    {
        PlayerProgress progress = _race.Progress;

        if (progress.OwnedModels == null)
            progress.OwnedModels = new System.Collections.Generic.List<string>();

        if (progress.OwnedModels.Contains("goose") == false)
            progress.OwnedModels.Add("goose");

        if (progress.OwnershipInitialized == false)
        {
            GrantOwnership(SelectedEntry.UpgradeId);

            foreach (RacerUpgradeProgress upgrades in progress.Upgrades)
            {
                if (upgrades != null)
                    GrantOwnership(upgrades.ModelId);
            }

            progress.OwnershipInitialized = true;
            progress.Save();
        }
    }

    public int[] GetPreviewColorIndices()
    {
        return _appearances.GetColorIndices(PreviewIndex);
    }

    public Color GetColor(int index)
    {
        return _appearances.GetEntry(index).Tint;
    }

    public void BrowseRelativeModel(int direction)
    {
        if (direction != -1 && direction != 1)
            return;

        int index = _appearances.GetRelativeModelIndex(PreviewIndex, direction);

        if (_appearances.GetEntry(index).UpgradeId == SelectedEntry.UpgradeId)
            index = SelectedIndex;

        Browse(index);
    }

    public bool Browse(int index)
    {
        if (_race.State != RaceState.Waiting || index < 0 || index >= AppearanceCount)
            return false;

        _previewIndex = index;
        PreviewChanged?.Invoke();
        return true;
    }

    public void EndPreview()
    {
        _previewIndex = -1;
        PreviewChanged?.Invoke();
    }

    public bool IsOwned(int index)
    {
        return index >= 0 && index < AppearanceCount
            && _race.Progress.OwnedModels.Contains(_appearances.GetEntry(index).UpgradeId);
    }

    public bool Select(int index)
    {
        if (_race.State != RaceState.Waiting || IsOwned(index) == false)
            return false;

        _race.Progress.Appearance = index;
        _previewIndex = -1;
        _race.Progress.Save();
        SelectionChanged?.Invoke();
        PreviewChanged?.Invoke();
        return true;
    }

    public bool CanPurchase()
    {
        return _race.State == RaceState.Waiting && IsOwned(PreviewIndex) == false
            && HighestCompletedLevel >= PreviewEntry.UnlockLevel
            && AvailablePoints >= PreviewEntry.PurchasePrice;
    }

    public bool TryPurchase()
    {
        if (CanPurchase() == false)
            return false;

        _race.Progress.Wallet -= PreviewEntry.PurchasePrice;
        GrantOwnership(PreviewEntry.UpgradeId);
        return Select(PreviewIndex);
    }

    public int GetUpgradeLevel(RacerUpgradeKind kind)
    {
        RacerUpgradeProgress progress = FindUpgrades(PreviewEntry);
        return Mathf.Clamp(progress != null ? progress.GetLevel(kind) : 0, 0, MaximumUpgradeLevel);
    }

    public int GetUpgradePrice(RacerUpgradeKind kind)
    {
        return _rules.GetPrice(GetUpgradeLevel(kind), PreviewEntry.UpgradePriceMultiplier);
    }

    public int GetUpgradeRequiredStage(RacerUpgradeKind kind)
    {
        return Mathf.Max(PreviewEntry.UnlockLevel, _rules.GetRequiredStage(GetUpgradeLevel(kind) + 1));
    }

    public float GetUpgradeValue(RacerUpgradeKind kind, int additionalLevels = 0)
    {
        return _rules.GetValue(PreviewEntry, kind, GetUpgradeLevel(kind) + additionalLevels,
            GetUpgradeLevel(RacerUpgradeKind.ThermalResistance));
    }

    public bool IsHandlingThermallyLimited()
    {
        int level = GetUpgradeLevel(RacerUpgradeKind.Handling);

        if (level >= MaximumUpgradeLevel)
            return false;

        float unlocked = _rules.GetValue(PreviewEntry, RacerUpgradeKind.Handling, level + 1, MaximumUpgradeLevel);
        return unlocked > GetUpgradeValue(RacerUpgradeKind.Handling, 1);
    }

    public bool CanUpgrade(RacerUpgradeKind kind)
    {
        if ((int)kind < 0 || (int)kind > (int)RacerUpgradeKind.ThermalResistance
            || _race.State != RaceState.Waiting || IsOwned(PreviewIndex) == false)
            return false;

        return GetUpgradeLevel(kind) < MaximumUpgradeLevel
            && HighestCompletedLevel >= GetUpgradeRequiredStage(kind)
            && AvailablePoints >= GetUpgradePrice(kind)
            && GetUpgradeValue(kind, 1) > GetUpgradeValue(kind);
    }

    public bool TryUpgrade(RacerUpgradeKind kind)
    {
        if (CanUpgrade(kind) == false)
            return false;

        int price = GetUpgradePrice(kind);
        RacerUpgradeProgress progress = FindUpgrades(PreviewEntry);

        if (progress == null)
        {
            progress = new RacerUpgradeProgress { ModelId = PreviewEntry.UpgradeId };
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
        _rules.Apply(SelectedEntry, FindUpgrades(SelectedEntry), target);
    }

    private void GrantOwnership(string id)
    {
        if (string.IsNullOrEmpty(id) == false && _race.Progress.OwnedModels.Contains(id) == false)
            _race.Progress.OwnedModels.Add(id);
    }

    private RacerUpgradeProgress FindUpgrades(RacerAppearanceCatalog.Entry model)
    {
        return _race.Progress.Upgrades.Find(progress => progress != null && progress.ModelId == model.UpgradeId);
    }
}
