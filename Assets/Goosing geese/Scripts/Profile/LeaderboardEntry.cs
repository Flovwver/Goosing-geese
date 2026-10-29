using System;

namespace GoosingGeese.Profile
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "LeaderboardEntry")]
    public class LeaderboardEntry
    {
        public string PlayerId;
        public string Name;
        public int Score;
    }
}
