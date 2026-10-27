using System;
using UnityEngine;

[Serializable]
public class RoadPiecePrefabEntry
{
    [SerializeField] private RoadPieceType _type;
    [SerializeField] private RoadPiece _prefab;

    public RoadPieceType Type => _type;
    public RoadPiece Prefab => _prefab;
}
