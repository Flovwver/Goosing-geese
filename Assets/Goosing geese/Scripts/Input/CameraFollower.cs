using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private CameraSettings _settings;

    private Vector3 _velocity;
    private Racer _racer;
    private bool _isFollow = true;

    private void Awake()
    {
        _racer = _target.GetComponent<Racer>();
    }

    private void OnEnable()
    {
        if (_racer == null)
            return;

        _racer.Finished += OnRaceFinished;
        _racer.RaceRestarted += OnRaceRestarted;
        _isFollow = _racer.IsFinished == false;
    }

    private void LateUpdate()
    {
        if (_isFollow == false)
            return;

        Vector3 desiredPosition = _target.position + _settings.Offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _settings.SmoothTime);
    }

    private void OnDisable()
    {
        if (_racer == null)
            return;

        _racer.Finished -= OnRaceFinished;
        _racer.RaceRestarted -= OnRaceRestarted;
    }

    private void OnRaceFinished()
    {
        _isFollow = false;
        _velocity = Vector3.zero;
    }

    private void OnRaceRestarted()
    {
        _isFollow = true;
        _velocity = Vector3.zero;
        transform.position = _target.position + _settings.Offset;
    }
}
