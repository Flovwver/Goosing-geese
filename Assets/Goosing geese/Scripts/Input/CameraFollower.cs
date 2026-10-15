using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private CameraSettings _settings;
    [SerializeField] private RaceController _race;

    private Vector3 _velocity;
    private bool _isFollow = true;

    private void OnEnable()
    {
        _race.Completed += OnRaceCompleted;
        _race.Prepared += OnRacePrepared;
        _isFollow = _race.Result == RaceResult.None;
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
        _race.Completed -= OnRaceCompleted;
        _race.Prepared -= OnRacePrepared;
    }

    private void OnRaceCompleted(RaceResult result)
    {
        _isFollow = false;
        _velocity = Vector3.zero;
    }

    private void OnRacePrepared()
    {
        _isFollow = true;
        _velocity = Vector3.zero;
        transform.position = _target.position + _settings.Offset;
    }
}
