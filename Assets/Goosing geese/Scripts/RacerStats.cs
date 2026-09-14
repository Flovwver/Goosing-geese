using UnityEngine;

[CreateAssetMenu(fileName = "RacerStats", menuName = "Racer/Stats")]
public class RacerStats : ScriptableObject
{
    public float MaxSpeed = 10f;
    public float Acceleration = 5f;
    public float TimeToOverheat = 3f;
}
