using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoadSettings", menuName = "Settings/RoadSettings")]
public class RoadSettings : ScriptableObject
{
    [Min(1)] public int RoadPieceCount = 20;
    [Min(1)] public int MaxGenerationAttempts = 100;
    [Min(1)] public int FinishRunoutPieces = 3;
    [Header("Path")]
    [Min(1)] public int CurveSamplesPerSegment = 24;
    public bool ShowPathLine = true;
    public Color PathLineColor = Color.cyan;
    [Min(0f)] public float PathLineWidth = 0.25f;
    public float PathLineHeight = 2.1f;
    public bool DrawPathGizmos = true;
    [Min(0f)] public float WaypointGizmoRadius = 0.3f;

    [Header("Speed hints")]
    public bool ShowSpeedHints = true;
    [Tooltip("Turns and their approach: keep speed in the green speedometer zone, below the yellow threshold.")]
    public Color SlowPathColor = Color.green;
    [Tooltip("Straights: use the yellow speedometer zone; the red overheat zone is never recommended.")]
    public Color FastPathColor = Color.yellow;
    [Min(0f)] public float TurnApproachDistance = 50f;

    [Header("Generation")]
    [SerializeField] private RoadPieceType[] _availablePieceTypes =
    {
        RoadPieceType.Straight,
        RoadPieceType.LeftTurn,
        RoadPieceType.RightTurn
    };

    [SerializeField] private PiecePrefabEntry[] _piecePrefabs;

    public RoadPieceType[] AvailablePieceTypes => _availablePieceTypes;
    public IReadOnlyList<PiecePrefabEntry> PiecePrefabs => _piecePrefabs;

    [Serializable]
    public class PiecePrefabEntry
    {
        [SerializeField] private RoadPieceType _type;
        [SerializeField] private RoadPiece _prefab;

        public RoadPieceType Type => _type;
        public RoadPiece Prefab => _prefab;
    }
}
