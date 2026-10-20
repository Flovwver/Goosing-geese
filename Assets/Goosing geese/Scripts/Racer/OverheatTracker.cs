using System;
using UnityEngine;

public class OverheatTracker : MonoBehaviour
{
    private float _redZoneTimer;

    public event Action<float> ProgressChanged;
    public event Action Exploded;

    public float SlipFactor { get; private set; }
    public float Progress { get; private set; }
    public bool IsExploded { get; private set; }

    public void ResetOverheat()
    {
        SlipFactor = 0f;
        _redZoneTimer = 0f;
        IsExploded = false;
        Progress = 0f;
        ProgressChanged?.Invoke(Progress);
    }

    public void Tick(float currentSpeed, RacerStats stats)
    {
        if (IsExploded)
            return;

        float normalizedSpeed = stats.MaxSpeed > 0f ? currentSpeed / stats.MaxSpeed : 0f;
        SlipFactor = currentSpeed > 0f && normalizedSpeed >= stats.YellowZoneThreshold ? 1f : 0f;

        bool speedIsInRedZone = currentSpeed > stats.MaxSpeed * stats.RedZoneThreshold;

        if (stats.TimeToOverheat <= 0f)
        {
            _redZoneTimer = 0f;
            UpdateProgress(speedIsInRedZone ? 1f : 0f);
            return;
        }

        float timerDelta = speedIsInRedZone ? Time.deltaTime : -Time.deltaTime * Mathf.Max(0f, stats.CoolingRate);

        _redZoneTimer = Mathf.Clamp(_redZoneTimer + timerDelta, 0f, stats.TimeToOverheat);

        UpdateProgress(_redZoneTimer / stats.TimeToOverheat);
    }

    private void UpdateProgress(float newProgress)
    {
        if (Mathf.Approximately(newProgress, Progress))
            return;

        Progress = newProgress;
        ProgressChanged?.Invoke(Progress);

        if (Progress >= 1f)
        {
            IsExploded = true;
            Exploded?.Invoke();
        }
    }
}
