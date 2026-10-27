using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RacerUpgradeRules", menuName = "Racer/Upgrade Rules")]
public class RacerUpgradeRules : ScriptableObject
{
    [Header("Prices and limits")]
    [SerializeField, Min(1)] private int _maximumLevel = 5;
    [SerializeField, Min(1)] private int _basePrice = 100;
    [SerializeField, Min(0)] private int _pricePerLevel = 100;
    [SerializeField, Min(0)] private int _firstUpgradeStage = 2;
    [SerializeField, Min(1)] private int _upgradeStageStep = 3;

    [Header("Gain per level")]
    [SerializeField, Min(0f)] private float _accelerationGain = 0.1f;
    [SerializeField, Min(0f)] private float _topSpeedGain = 0.05f;
    [SerializeField, Min(0f)] private float _coolingGain = 0.2f;
    [SerializeField, Range(0f, 1f)] private float _greenZoneGain = 0.07f;
    [SerializeField, Range(0f, 1f)] private float _thermalGain = 0.04f;
    [SerializeField, Range(0f, 1f)] private float _minimumYellowZone;

    public int MaximumLevel => Mathf.Max(1, _maximumLevel);

    public int GetPrice(int currentLevel, float multiplier)
    {
        double price = ((double)Mathf.Max(1, _basePrice)
            + (double)Mathf.Max(0, _pricePerLevel) * Mathf.Max(0, currentLevel)) * Mathf.Max(0.1f, multiplier);
        return (int)Math.Min(int.MaxValue, Math.Round(price, MidpointRounding.AwayFromZero));
    }

    public int GetRequiredStage(int nextLevel)
    {
        return Mathf.Max(0, _firstUpgradeStage) + Mathf.Max(0, nextLevel - 1) * Mathf.Max(1, _upgradeStageStep);
    }

    public float GetValue(RacerAppearanceDefinition model, RacerUpgradeKind kind, int level, int thermalLevel)
    {
        RacerStats source = model.Stats;
        level = Mathf.Clamp(level, 0, MaximumLevel);

        switch (kind)
        {
            case RacerUpgradeKind.Acceleration:
                return source.Acceleration * (1f + level * _accelerationGain);
            case RacerUpgradeKind.TopSpeed:
                return source.MaxSpeed * (1f + level * _topSpeedGain);
            case RacerUpgradeKind.Cooling:
                return source.CoolingRate * (1f + level * _coolingGain);
            case RacerUpgradeKind.ThermalResistance:
                return GetRedStart(model, level);
            case RacerUpgradeKind.Handling:
                float limit = Mathf.Min(model.MaximumGreenZone, GetRedStart(model, thermalLevel) - _minimumYellowZone);
                return Mathf.Max(source.YellowZoneThreshold, Mathf.Min(limit, source.YellowZoneThreshold + level * _greenZoneGain));
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    public void Apply(RacerAppearanceDefinition model, RacerUpgradeProgress progress, RacerStats target)
    {
        int thermal = progress != null ? progress.GetLevel(RacerUpgradeKind.ThermalResistance) : 0;

        foreach (RacerUpgradeKind kind in Enum.GetValues(typeof(RacerUpgradeKind)))
        {
            int level = progress != null ? progress.GetLevel(kind) : 0;
            float value = GetValue(model, kind, level, thermal);

            switch (kind)
            {
                case RacerUpgradeKind.Acceleration:
                    target.Acceleration = value;
                    break;
                case RacerUpgradeKind.TopSpeed:
                    target.MaxSpeed = value;
                    break;
                case RacerUpgradeKind.Cooling:
                    target.CoolingRate = value;
                    break;
                case RacerUpgradeKind.Handling:
                    target.YellowZoneThreshold = value;
                    break;
                case RacerUpgradeKind.ThermalResistance:
                    target.RedZoneThreshold = value;
                    break;
            }
        }
    }

    private float GetRedStart(RacerAppearanceDefinition model, int level)
    {
        float cap = Mathf.Clamp(model.MaximumRedStart, model.Stats.RedZoneThreshold, 1f);
        return Mathf.Min(cap, model.Stats.RedZoneThreshold + Mathf.Clamp(level, 0, MaximumLevel) * _thermalGain);
    }
}
