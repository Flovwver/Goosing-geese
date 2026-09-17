using UnityEngine;

public class Heat : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float _redZoneThreshold = 0.8f; 

    private float _redZoneTimer;

    public bool IsOverheated { get; private set; }

    public float NormalizedSpeed { get; private set; } = 0f;

    public void Tick(float currentSpeed, RacerStats stats)
    {
        NormalizedSpeed = currentSpeed / stats.MaxSpeed;

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
