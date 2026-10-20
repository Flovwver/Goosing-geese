using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RacerUpgradeRules", menuName = "Racer/Upgrade Rules")]
public class RacerUpgradeRules : ScriptableObject
{
    [Header("Prices and limits")]
    [SerializeField, Min(1)] private int _maximumLevel = 5;
    [SerializeField, Min(1)] private int _basePrice = 100;
    [SerializeField, Min(0)] private int _pricePerLevel = 100;

    [Header("Gain per level")]
    [SerializeField, Min(0f)] private float _accelerationGain = 0.1f;
    [SerializeField, Min(0f)] private float _topSpeedGain = 0.05f;
    [SerializeField, Min(0f)] private float _coolingGain = 0.2f;
    [SerializeField, Range(0f, 1f)] private float _greenZoneGain = 0.04f;
    [SerializeField, Range(0f, 1f)] private float _minimumYellowZone = 0.05f;

    public int MaximumLevel => Mathf.Max(1, _maximumLevel);

    public int GetPrice(int currentLevel)
    {
        long price = (long)Mathf.Max(1, _basePrice) + (long)Mathf.Max(0, _pricePerLevel) * Mathf.Max(0, currentLevel);
        return (int)Math.Min(int.MaxValue, price);
    }

    public float GetValue(RacerStats source, RacerUpgradeKind kind, int level)
    {
        level = Mathf.Clamp(level, 0, MaximumLevel);

        switch (kind)
        {
            case RacerUpgradeKind.Acceleration:
                return source.Acceleration * (1f + level * _accelerationGain);
            case RacerUpgradeKind.TopSpeed:
                return source.MaxSpeed * (1f + level * _topSpeedGain);
            case RacerUpgradeKind.Cooling:
                return source.CoolingRate * (1f + level * _coolingGain);
            case RacerUpgradeKind.Handling:
                float limit = Mathf.Max(source.YellowZoneThreshold, source.RedZoneThreshold - _minimumYellowZone);
                return Mathf.Min(limit, source.YellowZoneThreshold + level * _greenZoneGain);
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    public void Apply(RacerStats source, RacerUpgradeProgress progress, RacerStats target)
    {
        int acceleration = progress != null ? progress.GetLevel(RacerUpgradeKind.Acceleration) : 0;
        int topSpeed = progress != null ? progress.GetLevel(RacerUpgradeKind.TopSpeed) : 0;
        int cooling = progress != null ? progress.GetLevel(RacerUpgradeKind.Cooling) : 0;
        int handling = progress != null ? progress.GetLevel(RacerUpgradeKind.Handling) : 0;
        target.Acceleration = GetValue(source, RacerUpgradeKind.Acceleration, acceleration);
        target.MaxSpeed = GetValue(source, RacerUpgradeKind.TopSpeed, topSpeed);
        target.CoolingRate = GetValue(source, RacerUpgradeKind.Cooling, cooling);
        target.YellowZoneThreshold = GetValue(source, RacerUpgradeKind.Handling, handling);
        target.RedZoneThreshold = source.RedZoneThreshold;
    }
}
