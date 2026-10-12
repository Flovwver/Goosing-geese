using System.Collections.Generic;
using UnityEngine;

public class RoadLayoutGenerator
{
    private readonly int _maxGenerationAttempts;

    private readonly RoadPieceType[] _availablePieceTypes;

    public RoadLayoutGenerator(RoadPieceType[] availablePieceTypes, int maxGenerationAttempts)
    {
        _availablePieceTypes = availablePieceTypes;
        _maxGenerationAttempts = Mathf.Max(1, maxGenerationAttempts);
    }

    public RoadPieceType[] Generate(int pieceCount)
    {
        for (int attempt = 0; attempt < _maxGenerationAttempts; attempt++)
        {
            if (TryGenerate(pieceCount, out RoadPieceType[] layout))
                return layout;
        }

        Debug.LogWarning($"Не удалось сгенерировать дорогу из {pieceCount} кусков без пересечений.");
        return new RoadPieceType[0];
    }

    private bool TryGenerate(int pieceCount, out RoadPieceType[] layout)
    {
        layout = new RoadPieceType[pieceCount];

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

        return true;
    }

    private List<RoadPieceType> GetShuffledTypes()
    {
        var shuffledTypes = new List<RoadPieceType>(_availablePieceTypes);

        for (int index = shuffledTypes.Count - 1; index > 0; index--)
        {
            int randomIndex = Random.Range(0, index + 1);
            (shuffledTypes[index], shuffledTypes[randomIndex]) = (shuffledTypes[randomIndex], shuffledTypes[index]);
        }

        return shuffledTypes;
    }

    private Vector2Int RotateLeft(Vector2Int direction) => new (-direction.y, direction.x);

    private Vector2Int RotateRight(Vector2Int direction) => new (direction.y, -direction.x);
}
