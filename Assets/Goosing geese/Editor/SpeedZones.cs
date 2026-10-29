using GoosingGeese.Settings;
using UnityEngine;

namespace GoosingGeese.EditorTools
{
    public sealed class SpeedZones
    {
        public readonly Color Green = Color.green;
        public readonly Color Yellow = Color.yellow;
        public readonly Color Red = Color.red;

        public Color GetColor(float speed, RacerConfiguration stats)
        {
            if (speed <= 0f || speed < stats.MaxSpeed * stats.YellowZoneThreshold)
                return Green;

            if (speed < stats.MaxSpeed * stats.RedZoneThreshold)
                return Yellow;

            return Red;
        }
    }
}
