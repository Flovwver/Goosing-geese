using System.Collections.Generic;
using UnityEngine;

public class RoadBuilder
{
    private readonly Dictionary<RoadPieceType, RoadPiece> _piecePrefabsByType;
    private readonly List<RoadPiece> _placedPieces = new List<RoadPiece>();

    public RoadBuilder(Dictionary<RoadPieceType, RoadPiece> piecePrefabsByType)
    {
        _piecePrefabsByType = piecePrefabsByType;
    }

    public RoadPath Build(IReadOnlyList<RoadPieceType> layout, Vector3 startPosition, Quaternion startRotation)
    {
        Clear();

        Vector3 position = startPosition;
        Quaternion rotation = startRotation;

        for (int pieceIndex = 0; pieceIndex < layout.Count; pieceIndex++)
        {
            RoadPiece piecePrefab = _piecePrefabsByType[layout[pieceIndex]];
            RoadPiece placedPiece = Object.Instantiate(piecePrefab);

            AlignPieceToPoint(placedPiece, position, rotation);

            _placedPieces.Add(placedPiece);
            position = placedPiece.ExitPoint.position;
            rotation = placedPiece.ExitPoint.rotation;
        }

        return new RoadPath(_placedPieces);
    }

    public void Clear()
    {
        for (int pieceIndex = 0; pieceIndex < _placedPieces.Count; pieceIndex++)
        {
            if (_placedPieces[pieceIndex] == null)
                continue;

            _placedPieces[pieceIndex].gameObject.SetActive(false);
            Object.Destroy(_placedPieces[pieceIndex].gameObject);
        }

        _placedPieces.Clear();
    }

    private void AlignPieceToPoint(RoadPiece piece, Vector3 position, Quaternion rotation)
    {
        float rotationDelta = rotation.eulerAngles.y - piece.EntryPoint.eulerAngles.y;
        piece.transform.RotateAround(piece.EntryPoint.position, Vector3.up, rotationDelta);

        Vector3 entryToPieceOffset = piece.transform.position - piece.EntryPoint.position;
        piece.transform.position = position + entryToPieceOffset;
    }
}
