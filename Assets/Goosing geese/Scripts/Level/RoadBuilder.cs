using System.Collections.Generic;
using UnityEngine;

public class RoadBuilder
{
    private readonly Dictionary<RoadPieceType, RoadPiece> _piecePrefabsByType;
    private readonly List<RoadPiece> _placedPieces;

    public RoadBuilder(Dictionary<RoadPieceType, RoadPiece> piecePrefabsByType, List<RoadPiece> placedPieces)
    {
        _piecePrefabsByType = piecePrefabsByType;
        _placedPieces = placedPieces;
    }

    public RoadPath Build(RoadPieceType[] layout, Transform startPoint)
    {
        Clear();

        Transform previousExitPoint = startPoint;

        for (int pieceIndex = 0; pieceIndex < layout.Length; pieceIndex++)
        {
            RoadPiece piecePrefab = _piecePrefabsByType[layout[pieceIndex]];
            RoadPiece placedPiece = Object.Instantiate(piecePrefab);

            AlignPieceToPoint(placedPiece, previousExitPoint);

            _placedPieces.Add(placedPiece);
            previousExitPoint = placedPiece.ExitPoint;
        }

        return new RoadPath(_placedPieces);
    }

    public void Clear()
    {
        for (int pieceIndex = 0; pieceIndex < _placedPieces.Count; pieceIndex++)
        {
            _placedPieces[pieceIndex].gameObject.SetActive(false);
            Object.Destroy(_placedPieces[pieceIndex].gameObject);
        }

        _placedPieces.Clear();
    }

    private void AlignPieceToPoint(RoadPiece piece, Transform targetPoint)
    {
        float rotationDelta = targetPoint.eulerAngles.y - piece.EntryPoint.eulerAngles.y;
        piece.transform.RotateAround(piece.EntryPoint.position, Vector3.up, rotationDelta);

        Vector3 entryToPieceOffset = piece.transform.position - piece.EntryPoint.position;
        piece.transform.position = targetPoint.position + entryToPieceOffset;
    }
}
