using System.Collections.Generic;
using UnityEngine;

public class RoadLayoutGenerator
{
    private readonly int _maxGenerationAttempts;
    private readonly System.Random _random;

    private readonly RoadPieceType[] _availablePieceTypes;

    public RoadLayoutGenerator(IReadOnlyList<RoadPieceType> availablePieceTypes, int maxGenerationAttempts, int seed)
    {
        _availablePieceTypes = new RoadPieceType[availablePieceTypes?.Count ?? 0];

        for (int index = 0; index < _availablePieceTypes.Length; index++)
            _availablePieceTypes[index] = availablePieceTypes[index];

        _random = new System.Random(seed);
        _maxGenerationAttempts = Mathf.Max(1, maxGenerationAttempts);
    }

    public RoadPieceType[] Generate(int pieceCount, int finishRunoutPieces = 0)
    {
        pieceCount = Mathf.Max(1, pieceCount);

        finishRunoutPieces = Mathf.Max(0, finishRunoutPieces);

        for (int attempt = 0; attempt < _maxGenerationAttempts; attempt++)
        {
            if (TryGenerate(pieceCount, finishRunoutPieces, out RoadPieceType[] layout))
                return layout;
        }

        Debug.LogWarning("Road generation attempts exhausted; using a straight fallback.");
        var fallback = new RoadPieceType[pieceCount + finishRunoutPieces];

        for (int index = 0; index < fallback.Length; index++)
            fallback[index] = RoadPieceType.Straight;

        return fallback;
    }

    private bool TryGenerate(int pieceCount, int finishRunoutPieces, out RoadPieceType[] layout)
    {
        layout = new RoadPieceType[pieceCount + finishRunoutPieces];
        var occupiedCells = new HashSet<Vector2Int> { Vector2Int.zero };
        long remainingChoices = (long)pieceCount * Mathf.Max(1, _availablePieceTypes.Length);
        return TryPlacePiece(0, pieceCount, layout, Vector2Int.zero, Vector2Int.up, occupiedCells, ref remainingChoices);
    }

    private bool TryPlacePiece(int index, int pieceCount, RoadPieceType[] layout, Vector2Int position,
        Vector2Int direction, HashSet<Vector2Int> occupiedCells, ref long remainingChoices)
    {
        if (index == pieceCount)
            return TryAppendRunout(pieceCount, layout, position, direction, occupiedCells);

        Vector2Int targetCell = position + direction;

        if (remainingChoices <= 0 || occupiedCells.Add(targetCell) == false)
            return false;

        foreach (RoadPieceType candidate in GetShuffledTypes())
        {
            if (remainingChoices-- <= 0)
                break;

            Vector2Int nextDirection = direction;

            if (candidate == RoadPieceType.LeftTurn)
                nextDirection = RotateLeft(direction);
            else if (candidate == RoadPieceType.RightTurn)
                nextDirection = RotateRight(direction);

            layout[index] = candidate;

            if (TryPlacePiece(index + 1, pieceCount, layout, targetCell, nextDirection, occupiedCells, ref remainingChoices))
                return true;
        }

        occupiedCells.Remove(targetCell);
        return false;
    }

    private bool TryAppendRunout(int pieceCount, RoadPieceType[] layout, Vector2Int position,
        Vector2Int direction, HashSet<Vector2Int> occupiedCells)
    {
        for (int index = pieceCount; index < layout.Length; index++)
        {
            position += direction;

            if (occupiedCells.Contains(position))
                return false;

            layout[index] = RoadPieceType.Straight;
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
