using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadBuilder
{
    private readonly Dictionary<RoadPieceType, RoadPiece> _piecePrefabsByType;

    private GameObject _root;

    public RoadBuilder(Dictionary<RoadPieceType, RoadPiece> piecePrefabsByType)
    {
        _piecePrefabsByType = new Dictionary<RoadPieceType, RoadPiece>(piecePrefabsByType);
    }

    public RoadPath Build(IReadOnlyList<RoadPieceType> layout, Vector3 startPosition, Quaternion startRotation)
    {
        Validate(layout);
        var root = new GameObject("Road");
        root.SetActive(false);
        var pieces = new List<RoadPiece>();

        try
        {
            Vector3 position = startPosition;
            Quaternion rotation = startRotation;

            for (int pieceIndex = 0; pieceIndex < layout.Count; pieceIndex++)
            {
                RoadPiece piece = UnityEngine.Object.Instantiate(_piecePrefabsByType[layout[pieceIndex]], root.transform);
                AlignPieceToPoint(piece, position, rotation);
                pieces.Add(piece);
                position = piece.ExitPoint.position;
                rotation = piece.ExitPoint.rotation;
            }

            var path = new RoadPath(pieces);
            Clear();
            _root = root;
            return path;
        }
        catch
        {
            DestroyRoot(root);
            throw;
        }
    }

    public void Activate()
    {
        _root.SetActive(true);
    }

    public void Clear()
    {
        if (_root == null)
            return;

        DestroyRoot(_root);
        _root = null;
    }

    private static void DestroyRoot(GameObject root)
    {
        root.SetActive(false);

        if (Application.isPlaying)
            UnityEngine.Object.Destroy(root);
        else
            UnityEngine.Object.DestroyImmediate(root);
    }

    private void AlignPieceToPoint(RoadPiece piece, Vector3 position, Quaternion rotation)
    {
        float rotationDelta = rotation.eulerAngles.y - piece.EntryPoint.eulerAngles.y;
        piece.transform.RotateAround(piece.EntryPoint.position, Vector3.up, rotationDelta);
        Vector3 entryToPieceOffset = piece.transform.position - piece.EntryPoint.position;
        piece.transform.position = position + entryToPieceOffset;
    }

    private void Validate(IReadOnlyList<RoadPieceType> layout)
    {
        if (layout == null || layout.Count == 0)
            throw new ArgumentException("The road layout must contain at least one piece.");

        foreach (RoadPieceType type in layout)
        {
            if (!_piecePrefabsByType.TryGetValue(type, out RoadPiece prefab) || prefab == null)
                throw new ArgumentException($"A road prefab is missing for {type}.");

            if (prefab.Type != type || prefab.EntryPoint == null || prefab.ExitPoint == null)
                throw new ArgumentException($"Road prefab {prefab.name} has invalid type or connection points.");
        }
    }
}
