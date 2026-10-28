using UnityEngine;

[RequireComponent(typeof(Racer))]
public class OpponentDriver : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float _cruiseSpeed = 0.72f;
    [SerializeField, Range(0f, 1f)] private float _cornerSpeed = 0.45f;
    [SerializeField, Min(0f)] private float _laneOffset = 4f;
    [SerializeField, Min(0f)] private float _brakingPreviewDistance = 45f;

    private RaceController _race;
    private Racer _racer;
    private RoadNavigator _navigator;
    private RoadPath _path;
    private Vector3 _startPosition;
    private Quaternion _startRotation;

    private void Awake()
    {
        _racer = GetComponent<Racer>();
        _navigator = GetComponent<RoadNavigator>();
        _race = FindFirstObjectByType<RaceController>();
        _startPosition = transform.position;
        _startRotation = transform.rotation;
        _navigator.SetLaneOffset(_laneOffset);
    }

    private void Update()
    {
        if (_race.isActiveAndEnabled == false || _race.State != RaceState.Racing || _racer.IsFinished || _path == null)
            return;

        int segment = Mathf.Min(_navigator.CurrentSegment, _path.Pieces.Count - 1);
        bool corner = _path.Pieces[segment].Type != RoadPieceType.Straight;

        if (segment + 1 < _path.Pieces.Count && _path.Curve.GetSegmentEndDistance(segment) - _navigator.DistanceAlongPath < _brakingPreviewDistance)
            corner |= _path.Pieces[segment + 1].Type != RoadPieceType.Straight;

        float targetSpeed = corner ? _cornerSpeed : _cruiseSpeed;

        if (_racer.NormalizedSpeed < targetSpeed)
            _racer.Accelerate();
        else
            _racer.Cooldown();
    }

    private void OnDisable()
    {
        _racer.StopMovement();
    }

    public void Configure(float cruiseSpeed, float cornerSpeed)
    {
        _cruiseSpeed = Mathf.Clamp01(cruiseSpeed);
        _cornerSpeed = Mathf.Clamp01(cornerSpeed);
    }

    public void SetPath(RoadPath path)
    {
        _path = path;
        ResetOpponent();
    }

    public void ResetOpponent()
    {
        if (_path == null)
            return;

        RacerDefinition definition = _racer.CaptureDefinition(_startPosition + _startRotation * Vector3.right * _laneOffset, _startRotation);
        _racer.Prepare(_path, definition);
    }
}
