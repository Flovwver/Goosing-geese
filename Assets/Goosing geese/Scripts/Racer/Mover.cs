using UnityEngine;

public class Mover : MonoBehaviour
{
    public void MoveTowards(Vector3 targetPosition, float currentSpeed)
    {
        float savedheight = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, 
                                    new Vector3(targetPosition.x, savedheight, targetPosition.z), 
                                    currentSpeed * Time.deltaTime);
        
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }
}
