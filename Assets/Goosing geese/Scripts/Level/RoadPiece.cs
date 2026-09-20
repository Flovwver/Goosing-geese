using System.Collections.Generic;
using UnityEngine;

public class RoadPiece : MonoBehaviour
{
    [SerializeField] private RoadPieceType _type;
    [SerializeField] private Transform _entryPoint;
    [SerializeField] private Transform _exitPoint;
    [SerializeField] private Transform[] _waypoints;

    public RoadPieceType Type => _type;
    public Transform EntryPoint => _entryPoint;
    public Transform ExitPoint => _exitPoint;
    public IReadOnlyList<Transform> Waypoints => _waypoints;
}
