using System;
using UnityEngine;

public class OverheatTracker : MonoBehaviour
{
    private readonly EventPublisher _events = new EventPublisher();

    private float _redZoneTimer;

    public event Action<float> ProgressChanged;
    public event Action Exploded;

    public float Progress { get; private set; }
    public bool IsExploded { get; private set; }

    public void ResetOverheat()
    {
        _redZoneTimer = 0f;
        IsExploded = false;
        Progress = 0f;
        _events.Publish(ProgressChanged, Progress, this);
    }

    public void Tick(float currentSpeed, RacerConfiguration stats, float deltaTime)
    {
        if (IsExploded)
            return;

        bool speedIsInRedZone = currentSpeed > stats.MaxSpeed * stats.RedZoneThreshold;

        if (stats.TimeToOverheat <= 0f)
        {
            _redZoneTimer = 0f;
            UpdateProgress(speedIsInRedZone ? 1f : 0f);
            return;
        }

        float step = Mathf.Max(0f, deltaTime);
        float timerDelta = speedIsInRedZone ? step : -step * stats.CoolingRate;

        _redZoneTimer = Mathf.Clamp(_redZoneTimer + timerDelta, 0f, stats.TimeToOverheat);

        UpdateProgress(_redZoneTimer / stats.TimeToOverheat);
    }

    private void UpdateProgress(float newProgress)
    {
        if (Mathf.Approximately(newProgress, Progress))
            return;

        Progress = newProgress;
        _events.Publish(ProgressChanged, Progress, this);

        if (Progress >= 1f)
        {
            IsExploded = true;
            _events.Publish(Exploded, this);
        }
    }
}
