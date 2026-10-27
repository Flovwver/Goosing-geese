using UnityEngine;

public static class SpeedZones
{
    public static readonly Color Green = Color.green;
    public static readonly Color Yellow = Color.yellow;
    public static readonly Color Red = Color.red;

    public static Color GetColor(float speed, RacerConfiguration stats)
    {
        if (speed <= 0f || speed < stats.MaxSpeed * stats.YellowZoneThreshold)
            return Green;

        if (speed < stats.MaxSpeed * stats.RedZoneThreshold)
            return Yellow;

        return Red;
    }
}
