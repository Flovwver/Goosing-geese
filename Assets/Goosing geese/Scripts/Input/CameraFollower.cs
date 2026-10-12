using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private CameraSettings _settings;

    private Vector3 _velocity;

    private void LateUpdate()
    {
        Vector3 desiredPosition = _target.position + _settings.Offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _settings.SmoothTime);
    }
}
