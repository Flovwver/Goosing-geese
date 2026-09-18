using UnityEngine;

public class RoadPiece : MonoBehaviour
{
    [SerializeField] private RoadPieceType _type;
    [SerializeField] private Transform _entryPoint;
    [SerializeField] private Transform _exitPoint;

    public RoadPieceType Type => _type;
    public Transform EntryPoint => _entryPoint;
    public Transform ExitPoint => _exitPoint;
}
