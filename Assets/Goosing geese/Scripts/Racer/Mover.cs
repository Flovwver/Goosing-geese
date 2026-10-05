using UnityEngine;

public class Mover : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;

    public void MoveTowards(Vector3 targetPosition, float allowedSpeed, RacerStats stats)
    {
        FaceTowards(targetPosition);
        PushForward(allowedSpeed, stats);
    }

    public void Stop()
    {
        if (_rigidbody == null)
            return;

        _rigidbody.linearVelocity = Vector3.zero;
    }

    private void FaceTowards(Vector3 targetPosition)
    {
        Vector3 offsetToTarget = targetPosition - transform.position;
        offsetToTarget.y = 0f;

        if (offsetToTarget.sqrMagnitude <= Mathf.Epsilon)
            return;

        transform.rotation = Quaternion.LookRotation(offsetToTarget.normalized, Vector3.up);
    }

    private void PushForward(float allowedSpeed, RacerStats stats)
    {
        if (_rigidbody == null || allowedSpeed <= 0f)
            return;

        _rigidbody.AddForce(transform.forward * allowedSpeed, ForceMode.Acceleration);
    }
}