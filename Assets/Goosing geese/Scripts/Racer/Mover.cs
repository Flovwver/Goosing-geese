using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Mover : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void MoveTowards(Vector3 targetPosition, float allowedSpeed)
    {
        FaceTowards(targetPosition);
        PushForward(allowedSpeed);
    }

    public void Stop()
    {
        if (_rigidbody == null)
            return;

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        Stop();
        _rigidbody.position = position;
        _rigidbody.rotation = rotation;
        transform.SetPositionAndRotation(position, rotation);
    }

    private void FaceTowards(Vector3 targetPosition)
    {
        Vector3 offsetToTarget = targetPosition - transform.position;
        offsetToTarget.y = 0f;

        if (offsetToTarget.sqrMagnitude <= Mathf.Epsilon)
            return;

        transform.rotation = Quaternion.LookRotation(offsetToTarget.normalized, Vector3.up);
    }

    private void PushForward(float allowedSpeed)
    {
        if (_rigidbody == null || allowedSpeed <= 0f)
            return;

        _rigidbody.AddForce(transform.forward * allowedSpeed, ForceMode.Acceleration);
    }
}
