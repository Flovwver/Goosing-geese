using System;
using GoosingGeese.Racing;
using GoosingGeese.Settings;
using UnityEngine;

namespace GoosingGeese.Garage
{
    [DefaultExecutionOrder(-50)]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "RacerGarage")]
    public class RacerGarage : MonoBehaviour
    {
        [SerializeField]
        private RaceController _race;
        [SerializeField]
        private RacerAppearanceCatalog _appearances;
        [SerializeField]
        private RacerUpgradeRules _rules;
        private int _previewIndex = -1;

        public event Action SelectionChanged;
        public event Action PreviewChanged;
        public event Action UpgradesChanged;

        public int SelectedIndex => Mathf.Clamp(_race.Progress.SelectedAppearanceIndex, 0, _appearances.Count - 1);
        public int PreviewIndex => _previewIndex >= 0 ? _previewIndex : SelectedIndex;
        public int AppearanceCount => _appearances.Count;
        public int Balance => _race.Progress.Balance;
        public int TotalPoints => _race.Progress.Points;
        public int HighestCompletedLevel => _race.Progress.HighestCompletedLevel;
        public int MaximumUpgradeLevel => _rules.MaximumLevel;
        public RacerAppearanceDefinition SelectedEntry => _appearances.GetEntry(SelectedIndex);
        public RacerAppearanceDefinition PreviewEntry => _appearances.GetEntry(PreviewIndex);
        public RacerStats BaseStats => SelectedEntry.Stats;

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

            if (_appearances.GetEntry(index).ModelId == SelectedEntry.ModelId)
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
            return index >= 0 && index < AppearanceCount && _race.Progress.OwnsModel(_appearances.GetEntry(index).ModelId);
        }

        public bool Select(int index)
        {
            if (_race.State != RaceState.Waiting || IsOwned(index) == false)
                return false;

            _race.Progress.SelectAppearance(index);
            _previewIndex = -1;
            _race.Session.Commands.Save();
            SelectionChanged?.Invoke();
            PreviewChanged?.Invoke();
            return true;
        }

        public bool CanPurchase()
        {
            return _race.State == RaceState.Waiting
                && IsOwned(PreviewIndex) == false
                && HighestCompletedLevel >= PreviewEntry.UnlockLevel
                && Balance >= PreviewEntry.PurchasePrice;
        }

        public bool TryPurchase()
        {
            if (CanPurchase() == false)
                return false;

            if (_race.Progress.TrySpend(PreviewEntry.PurchasePrice) == false)
                return false;

            GrantOwnership(PreviewEntry.ModelId);
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
            return _rules.GetValue(PreviewEntry, kind, GetUpgradeLevel(kind) + additionalLevels, GetUpgradeLevel(RacerUpgradeKind.ThermalResistance));
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
            if ((int)kind < 0 || (int)kind > (int)RacerUpgradeKind.ThermalResistance || _race.State != RaceState.Waiting || IsOwned(PreviewIndex) == false)
                return false;

            return GetUpgradeLevel(kind) < MaximumUpgradeLevel
                && HighestCompletedLevel >= GetUpgradeRequiredStage(kind)
                && Balance >= GetUpgradePrice(kind)
                && GetUpgradeValue(kind,
                1) > GetUpgradeValue(kind);
        }

        public bool TryUpgrade(RacerUpgradeKind kind)
        {
            if (CanUpgrade(kind) == false)
                return false;

            int price = GetUpgradePrice(kind);

            if (_race.Progress.TryBuyUpgrade(PreviewEntry.ModelId, kind, GetUpgradeLevel(kind) + 1, price) == false)
                return false;

            _race.Session.Commands.Save();
            UpgradesChanged?.Invoke();
            return true;
        }

        public void ApplyStats(RacerStats target)
        {
            _rules.Apply(SelectedEntry, FindUpgrades(SelectedEntry), target);
        }

        private void GrantOwnership(string id)
        {
            _race.Progress.GrantModel(id);
        }

        private RacerUpgradeProgress FindUpgrades(RacerAppearanceDefinition model)
        {
            return _race.Progress.GetUpgrades(model.ModelId);
        }
    }
}
