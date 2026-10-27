using UnityEngine;

public class RoadSafety : MonoBehaviour
{
    [SerializeField] private Level _level;
    [SerializeField] private Racer _player;
    [SerializeField] private RaceController _race;
    [SerializeField, Min(0f)] private float _roadHalfWidth = 12f;
    [SerializeField, Min(0f)] private float _offRoadGraceTime = 0.75f;

    private RoadNavigator _navigator;
    private bool _hasPath;
    private float _offRoadTimer;

    private void Awake()
    {
        _navigator = _player.GetComponent<RoadNavigator>();
    }

    private void OnEnable()
    {
        _hasPath = false;
        _level.PathBuilt += OnPathBuilt;
        _player.RaceRestarted += OnRestarted;

        if (_level.TryGetBuiltPath(out RoadPath path, out RoadConfiguration settings))
            OnPathBuilt(path, settings);
    }

    private void Update()
    {
        if (_race.State != RaceState.Racing || _hasPath == false)
            return;

        Vector3 displacement = _player.transform.position - _navigator.ClosestPosition;
        displacement.y = 0f;
        _offRoadTimer = displacement.sqrMagnitude > _roadHalfWidth * _roadHalfWidth ? _offRoadTimer + Time.deltaTime : 0f;

        if (_offRoadTimer >= _offRoadGraceTime)
            _race.EndRace(false, RaceOutcome.OffRoad);
    }

    private void OnDisable()
    {
        _level.PathBuilt -= OnPathBuilt;
        _player.RaceRestarted -= OnRestarted;
        _hasPath = false;
    }

    private void OnPathBuilt(RoadPath path, RoadConfiguration settings)
    {
        _hasPath = path != null && path.Curve != null;
        _offRoadTimer = 0f;
    }

    private void OnRestarted()
    {
        _offRoadTimer = 0f;
    }
}
