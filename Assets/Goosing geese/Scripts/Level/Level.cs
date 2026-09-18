using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private Transform _roadStartPoint;
    [SerializeField] private RoadPiece _straightPiecePrefab;
    [SerializeField] private RoadPiece _leftTurnPiecePrefab;
    [SerializeField] private RoadPiece _rightTurnPiecePrefab;
    [SerializeField] private int _roadPieceCount = 20;

    private RoadLayoutGenerator _roadLayoutGenerator;
    private RoadBuilder _roadBuilder;

    private void Awake()
    {
        _roadLayoutGenerator = new RoadLayoutGenerator(new[] { RoadPieceType.Straight, RoadPieceType.LeftTurn, RoadPieceType.RightTurn });
        _roadBuilder = new RoadBuilder(BuildPiecePrefabsByType());
    }

    private void Start()
    {
        BuildRoad();
    }

    [ContextMenu("Build road")]
    public void BuildRoad()
    {
        RoadPieceType[] roadLayout = _roadLayoutGenerator.Generate(_roadPieceCount);
        _roadBuilder.Build(roadLayout, _roadStartPoint);
    }

    private Dictionary<RoadPieceType, RoadPiece> BuildPiecePrefabsByType()
    {
        return new Dictionary<RoadPieceType, RoadPiece>
        {
            { RoadPieceType.Straight, _straightPiecePrefab },
            { RoadPieceType.LeftTurn, _leftTurnPiecePrefab },
            { RoadPieceType.RightTurn, _rightTurnPiecePrefab }
        };
    }
}
