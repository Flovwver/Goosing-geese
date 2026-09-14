using UnityEngine;

public class Overheat : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float _redZoneThreshold = 0.8f; 

    private float _redZoneTimer;

    public bool IsOverheated { get; private set; }

    public void Tick(float currentSpeed, RacerStats stats)
    {
        bool speedIsInRedZone = currentSpeed > stats.MaxSpeed * _redZoneThreshold;

        if (speedIsInRedZone == false)
        {
            _redZoneTimer = Mathf.Max(0f, _redZoneTimer - Time.deltaTime);
            IsOverheated = false;
        }
        else
        {
            _redZoneTimer += Time.deltaTime;

            if (_redZoneTimer >= stats.TimeToOverheat)
            {
                IsOverheated = true;
            }
        }
    }
}
