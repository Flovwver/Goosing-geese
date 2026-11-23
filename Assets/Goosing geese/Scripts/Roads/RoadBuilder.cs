using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoosingGeese.Roads
{
    public class RoadBuilder
    {
        private readonly Dictionary<RoadPieceType, RoadPiece> _piecePrefabsByType;
        private GameObject _root;

        public RoadBuilder(IReadOnlyDictionary<RoadPieceType, RoadPiece> piecePrefabsByType)
        {
            _piecePrefabsByType = new Dictionary<RoadPieceType, RoadPiece>();

            foreach (KeyValuePair<RoadPieceType, RoadPiece> pair in piecePrefabsByType)
                _piecePrefabsByType.Add(pair.Key, pair.Value);
        }

        public RoadPath Build(IReadOnlyList<RoadPieceType> layout, Vector3 startPosition, Quaternion startRotation)
        {
            Validate(layout);
            var root = new GameObject("Road");
            root.SetActive(false);
            var pieces = new List<RoadPiece>();

            try
            {
                RoadPiece extension = UnityEngine.Object.Instantiate(_piecePrefabsByType[RoadPieceType.Straight], root.transform);
                extension.name = "Road before start";
                AlignPieceToPoint(extension.transform, extension.ExitPoint, startPosition, startRotation);
                Vector3 position = startPosition;
                Quaternion rotation = startRotation;

                for (int pieceIndex = 0; pieceIndex < layout.Count; pieceIndex++)
                {
                    RoadPiece piece = UnityEngine.Object.Instantiate(_piecePrefabsByType[layout[pieceIndex]], root.transform);
                    AlignPieceToPoint(piece.transform, piece.EntryPoint, position, rotation);
                    pieces.Add(piece);
                    position = piece.ExitPoint.position;
                    rotation = piece.ExitPoint.rotation;
                }

                var path = new RoadPath(pieces, extension);
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

        private static void AlignPieceToPoint(Transform piece, Transform connection, Vector3 position, Quaternion rotation)
        {
            float rotationDelta = rotation.eulerAngles.y - connection.eulerAngles.y;
            piece.RotateAround(connection.position, Vector3.up, rotationDelta);
            piece.position += position - connection.position;
        }

        private void Validate(IReadOnlyList<RoadPieceType> layout)
        {
            if (layout == null || layout.Count == 0)
                throw new ArgumentException("The road layout must contain at least one piece.");

            ValidatePrefab(RoadPieceType.Straight);

            foreach (RoadPieceType type in layout)
                ValidatePrefab(type);
        }

        private void ValidatePrefab(RoadPieceType type)
        {
            if (_piecePrefabsByType.TryGetValue(type, out RoadPiece prefab) == false || prefab == null)
                throw new ArgumentException($"A road prefab is missing for {type}.");

            if (prefab.Type != type || prefab.EntryPoint == null || prefab.ExitPoint == null)
                throw new ArgumentException($"Road prefab {prefab.name} has invalid type or connection points.");
        }
    }
}
