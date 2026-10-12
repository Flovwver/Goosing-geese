using System;
using UnityEngine;

public class OverheatTracker : MonoBehaviour
{
    [SerializeField] private OverheatSettings _settings;

    private float _redZoneTimer;

    public float Progress { get; private set; }
    public bool IsExploded { get; private set; }

    public event Action<float> ProgressChanged;
    public event Action Exploded;

    public void ResetOverheat()
    {
        _redZoneTimer = 0f;
        IsExploded = false;
        Progress = 0f;
        ProgressChanged?.Invoke(Progress);
    }

    public void Tick(float currentSpeed, RacerStats stats)
    {
        if (IsExploded)
            return;

        bool speedIsInRedZone = currentSpeed > stats.MaxSpeed * _settings.RedZoneThreshold;
        float timerDelta = speedIsInRedZone ? Time.deltaTime : -Time.deltaTime;

        _redZoneTimer = Mathf.Clamp(_redZoneTimer + timerDelta, 0f, _settings.TimeToOverheat);

        UpdateProgress(_redZoneTimer / _settings.TimeToOverheat);
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
