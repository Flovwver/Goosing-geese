using System.Linq;
using GoosingGeese.Levels;
using GoosingGeese.Racing;
using GoosingGeese.Roads;
using UnityEditor;
using UnityEngine;

namespace GoosingGeese.EditorTools
{
    public sealed class RoadDiagnostics
    {
        [MenuItem("Tools/Goosing Geese/Report advisory speed profile")]
        public static void Report()
        {
            Level level = UnityEngine.Object.FindFirstObjectByType<Level>();

            if (level == null || level.CurrentDefinition == null || level.TryGetBuiltPath(out RoadPath path, out _) == false)
            {
                Debug.Log("Enter Play Mode and create a level before reporting its advisory speed profile.");
                return;
            }

            var profile = new RoadSpeedProfile(path.Curve, level.CurrentDefinition.Racer.Stats);
            Debug.Log($"Advisory speed range: {profile.Limits.Min():0.##}–{profile.Limits.Max():0.##}; {profile.Limits.Count} samples. Road colors remain based on section type.",
                level);
        }
    }
}
