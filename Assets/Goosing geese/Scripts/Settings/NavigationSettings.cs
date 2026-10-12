using UnityEngine;

[CreateAssetMenu(fileName = "NavigationSettings", menuName = "Settings/NavigationSettings")]
public class NavigationSettings : ScriptableObject
{
    [Min(0f)] public float ArrivalDistance = 0.5f;
}
