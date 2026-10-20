using System;
using UnityEngine;

[Serializable]
public class RacerUpgradeProgress
{
    public string ModelId;
    public int Acceleration;
    public int TopSpeed;
    public int Cooling;
    public int Handling;

    public int GetLevel(RacerUpgradeKind kind)
    {
        switch (kind)
        {
            case RacerUpgradeKind.Acceleration:
                return Mathf.Max(0, Acceleration);
            case RacerUpgradeKind.TopSpeed:
                return Mathf.Max(0, TopSpeed);
            case RacerUpgradeKind.Cooling:
                return Mathf.Max(0, Cooling);
            case RacerUpgradeKind.Handling:
                return Mathf.Max(0, Handling);
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    public void SetLevel(RacerUpgradeKind kind, int level)
    {
        level = Mathf.Max(0, level);

        switch (kind)
        {
            case RacerUpgradeKind.Acceleration:
                Acceleration = level;
                break;
            case RacerUpgradeKind.TopSpeed:
                TopSpeed = level;
                break;
            case RacerUpgradeKind.Cooling:
                Cooling = level;
                break;
            case RacerUpgradeKind.Handling:
                Handling = level;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }
}
