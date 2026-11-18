using System;
using UnityEngine;

namespace GoosingGeese.Garage
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "RacerUpgradeProgress")]
    public class RacerUpgradeProgress
    {
        [SerializeField] private string ModelId;
        [SerializeField] private int Acceleration;
        [SerializeField] private int TopSpeed;
        [SerializeField] private int Cooling;
        [SerializeField] private int Handling;
        [SerializeField] private int ThermalResistance;

        public RacerUpgradeProgress(string modelId)
        {
            ModelId = modelId;
        }

        public string Id => ModelId;

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
                case RacerUpgradeKind.ThermalResistance:
                    return Mathf.Max(0, ThermalResistance);
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
                case RacerUpgradeKind.ThermalResistance:
                    ThermalResistance = level;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}
