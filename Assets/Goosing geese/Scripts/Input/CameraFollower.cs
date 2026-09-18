using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 12f, -8f);
    [SerializeField] private float _smoothTime = 0.15f;

    private Vector3 _velocity;

    private void LateUpdate()
    {
        Vector3 desiredPosition = _target.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _smoothTime);
    }
}
