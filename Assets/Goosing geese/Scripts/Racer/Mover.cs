using UnityEngine;

public class Mover : MonoBehaviour
{
    public void Move(float currentSpeed)
    {
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }
}
