using UnityEngine;

namespace GoosingGeese.Settings
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "Settings/CameraSettings")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "CameraSettings")]
    public class CameraSettings : ScriptableObject
    {
        public Vector3 Offset = new Vector3(0f, 12f, -8f);
        [Min(0f)]
        public float SmoothTime = 0.15f;
    }
}
