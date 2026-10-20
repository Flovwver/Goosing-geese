using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    private const float MinimumSmoothTime = 0.01f;
    [SerializeField] private Transform _target;
    [SerializeField] private CameraSettings _settings;
    [SerializeField, Min(0f)] private float _menuFocusHeight = 3f;

    private Vector3 _velocity;
    private Racer _racer;
    private bool _isFollowing = true;
    private RaceController _race;
    private OpponentSpawner _opponents;
    private Quaternion _followingRotation;

    private void Awake()
    {
        _racer = _target.GetComponent<Racer>();
        _race = FindFirstObjectByType<RaceController>();
        _opponents = FindFirstObjectByType<OpponentSpawner>();
        _followingRotation = transform.rotation;
    }

    private void OnEnable()
    {
        if (_racer == null)
            return;

        _racer.Finished += OnRaceFinished;
        _racer.Exploded += OnRaceFinished;
        _racer.RaceRestarted += OnRaceRestarted;
        _isFollowing = _racer.IsFinished == false;
    }

    private void LateUpdate()
    {
        if (_isFollowing == false)
            return;

        Vector3 focus = _target.position;
        bool waiting = _race != null && _race.State == RaceState.Waiting;

        if (waiting && _opponents != null && _opponents.Opponent != null)
            focus = (focus + _opponents.Opponent.transform.position) * 0.5f;

        Vector3 desiredPosition = focus + _settings.Offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _settings.SmoothTime);
        Quaternion rotation = waiting ? Quaternion.LookRotation(focus + Vector3.up * _menuFocusHeight - transform.position) : _followingRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime / Mathf.Max(MinimumSmoothTime, _settings.SmoothTime));
    }

    private void OnDisable()
    {
        if (_racer == null)
            return;

        _racer.Finished -= OnRaceFinished;
        _racer.Exploded -= OnRaceFinished;
        _racer.RaceRestarted -= OnRaceRestarted;
    }

    private void OnRaceFinished()
    {
        _isFollowing = false;
        _velocity = Vector3.zero;
    }

    private void OnRaceRestarted()
    {
        _isFollowing = true;
        _velocity = Vector3.zero;
        transform.position = _target.position + _settings.Offset;
    }
}
