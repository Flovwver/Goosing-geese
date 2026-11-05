using System.Collections.Generic;
using GoosingGeese.Roads;
using UnityEngine;

namespace GoosingGeese.Settings
{
    [CreateAssetMenu(fileName = "RoadSettings", menuName = "Settings/RoadSettings")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "RoadSettings")]
    public class RoadSettings : ScriptableObject
    {
        [Min(1)]
        public int RoadPieceCount = 20;
        [Min(1)]
        public int MaxGenerationAttempts = 100;
        [Min(1)]
        public int FinishRunoutPieces = 3;
        [Header("Path")]
        [Min(1)]
        public int CurveSamplesPerSegment = 24;
        public bool ShowPathLine = true;
        [Min(0f)]
        public float PathLineWidth = 0.25f;
        public float PathLineHeight = 2.1f;
        public bool DrawPathGizmos = true;
        [Min(0f)]
        public float WaypointGizmoRadius = 0.3f;
        [Header("Generation")]
        [SerializeField]
        private RoadPieceType[] _availablePieceTypes =
        {
            RoadPieceType.Straight,
            RoadPieceType.LeftTurn,
            RoadPieceType.RightTurn
        };
        [SerializeField]
        private RoadPiecePrefabEntry[] _piecePrefabs;

        public RoadPieceType[] AvailablePieceTypes => _availablePieceTypes;
        public IReadOnlyList<RoadPiecePrefabEntry> PiecePrefabs => _piecePrefabs;
    }
}
