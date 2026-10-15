using System.Collections.Generic;
using UnityEngine;

public class RoadLayoutGenerator
{
    private readonly int _maxGenerationAttempts;
    private readonly System.Random _random;
    private readonly RoadPieceType[] _availablePieceTypes;

    public RoadLayoutGenerator(RoadPieceType[] availablePieceTypes, int maxGenerationAttempts, int seed)
    {
        _availablePieceTypes = (RoadPieceType[])availablePieceTypes.Clone();
        _maxGenerationAttempts = Mathf.Max(1, maxGenerationAttempts);
        _random = new System.Random(seed);
    }

    public RoadPieceType[] Generate(int pieceCount, int finishRunoutPieces = 0)
    {
        if (pieceCount <= 0)
            return new RoadPieceType[0];

        finishRunoutPieces = Mathf.Max(0, finishRunoutPieces);

        for (int attempt = 0; attempt < _maxGenerationAttempts; attempt++)
        {
            if (TryGenerate(pieceCount, finishRunoutPieces, out RoadPieceType[] layout))
                return layout;
        }

        Debug.LogWarning($"Не удалось сгенерировать дорогу из {pieceCount} кусков без пересечений.");
        return new RoadPieceType[0];
    }

    private bool TryGenerate(int pieceCount, int finishRunoutPieces, out RoadPieceType[] layout)
    {
        layout = new RoadPieceType[pieceCount + finishRunoutPieces];

        var occupiedCells = new HashSet<Vector2Int> { Vector2Int.zero };
        Vector2Int position = Vector2Int.zero;
        Vector2Int direction = Vector2Int.up;

        for (int pieceIndex = 0; pieceIndex < pieceCount; pieceIndex++)
        {
            bool piecePlaced = false;

            foreach (RoadPieceType candidateType in GetShuffledTypes())
            {
                Vector2Int targetCell = position + direction;

                if (occupiedCells.Contains(targetCell))
                    continue;

                occupiedCells.Add(targetCell);
                layout[pieceIndex] = candidateType;
                position = targetCell;

                if (candidateType == RoadPieceType.LeftTurn)
                    direction = RotateLeft(direction);
                else if (candidateType == RoadPieceType.RightTurn)
                    direction = RotateRight(direction);

                piecePlaced = true;
                break;
            }

            if (piecePlaced == false)
                return false;
        }

        for (int pieceIndex = pieceCount; pieceIndex < layout.Length; pieceIndex++)
        {
            position += direction;

            if (occupiedCells.Add(position) == false)
                return false;

            layout[pieceIndex] = RoadPieceType.Straight;
        }

        return true;
    }

    private List<RoadPieceType> GetShuffledTypes()
    {
        var shuffledTypes = new List<RoadPieceType>(_availablePieceTypes);

        for (int index = shuffledTypes.Count - 1; index > 0; index--)
        {
            int randomIndex = _random.Next(index + 1);
            (shuffledTypes[index], shuffledTypes[randomIndex]) = (shuffledTypes[randomIndex], shuffledTypes[index]);
        }

        return shuffledTypes;
    }

    private Vector2Int RotateLeft(Vector2Int direction) => new (-direction.y, direction.x);

    private Vector2Int RotateRight(Vector2Int direction) => new (direction.y, -direction.x);
}
