public static class GripModel
{
    public static float CalculateSlipFactor(float currentSpeed, RacerConfiguration stats)
    {
        return currentSpeed > 0f && currentSpeed >= stats.MaxSpeed * stats.YellowZoneThreshold ? 1f : 0f;
    }
}
