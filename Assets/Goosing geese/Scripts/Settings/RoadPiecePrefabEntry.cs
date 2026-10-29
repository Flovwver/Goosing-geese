using System;
using GoosingGeese.Roads;
using UnityEngine;

namespace GoosingGeese.Settings
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "RoadPiecePrefabEntry")]
    public class RoadPiecePrefabEntry
    {
        [SerializeField]
        private RoadPieceType _type;
        [SerializeField]
        private RoadPiece _prefab;

        public RoadPieceType Type => _type;
        public RoadPiece Prefab => _prefab;
    }
}
