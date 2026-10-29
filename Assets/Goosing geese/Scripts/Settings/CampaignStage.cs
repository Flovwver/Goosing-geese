using System;
using GoosingGeese.Roads;

namespace GoosingGeese.Settings
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "CampaignStage")]
    public class CampaignStage
    {
        public int Seed;
        public int PieceCount;
        public LevelDifficulty Difficulty;
        public int Reward;
        public int FirstClearBonus;
        public float OpponentMaxSpeed;
        public float OpponentAcceleration;
        public float CruiseSpeed;
        public float CornerSpeed;
        public RoadPieceType[] Layout;

        public string GetDifficultyName(bool english)
        {
            switch (Difficulty)
            {
                case LevelDifficulty.Easy:
                    return english ? "Easy" : "Лёгкий";
                case LevelDifficulty.Medium:
                    return english ? "Medium" : "Средний";
                default:
                    return english ? "Hard" : "Сложный";
            }
        }
    }
}
