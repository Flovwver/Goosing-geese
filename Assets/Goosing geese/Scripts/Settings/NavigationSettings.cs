using UnityEngine;

namespace GoosingGeese.Settings
{
    [CreateAssetMenu(fileName = "NavigationSettings", menuName = "Settings/NavigationSettings")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "NavigationSettings")]
    public class NavigationSettings : ScriptableObject
    {
        [Min(0f)]
        public float ArrivalDistance = 0.5f;
        [Min(0f)]
        public float LookAheadDistance = 5f;
        [Min(0f)]
        public float WaypointPassTolerance = 0.25f;
        [Min(0f)]
        public float DebugPointRadius = 0.5f;
        [Min(0f)]
        public float FinishHalfWidth = 12f;
    }
}
