using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoadSettings", menuName = "Settings/RoadSettings")]
public class RoadSettings : ScriptableObject
{
    [SerializeField] private RoadPieceType[] _availablePieceTypes =
    {
        RoadPieceType.Straight,
        RoadPieceType.LeftTurn,
        RoadPieceType.RightTurn
    };

    public RoadPieceType[] AvailablePieceTypes => _availablePieceTypes;

    [SerializeField] private PiecePrefabEntry[] _piecePrefabs;

    public IReadOnlyList<PiecePrefabEntry> PiecePrefabs => _piecePrefabs;
    [Min(1)] public int RoadPieceCount = 20;
    [Min(1)] public int MaxGenerationAttempts = 100;
    public bool DrawPathGizmos = true;
    [Min(0f)] public float WaypointGizmoRadius = 0.3f;

    [Serializable]
    public class PiecePrefabEntry
    {
        [SerializeField] private RoadPieceType _type;
        [SerializeField] private RoadPiece _prefab;

        public RoadPieceType Type => _type;
        public RoadPiece Prefab => _prefab;
    }
}
