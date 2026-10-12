using UnityEngine;

[CreateAssetMenu(fileName = "OverheatSettings", menuName = "Settings/OverheatSettings")]
public class OverheatSettings : ScriptableObject
{
    [Range(0f, 1f)] public float RedZoneThreshold = 0.8f;
    [Min(0.01f)] public float TimeToOverheat = 3f;
}
