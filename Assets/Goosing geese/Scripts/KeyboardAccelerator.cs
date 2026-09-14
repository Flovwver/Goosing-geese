using UnityEngine;

public class KeyboardAccelerator : MonoBehaviour, IInputSource
{
    public bool IsAccelerating { get; private set; }

    private void Update()
    {
        bool spaceKeyIsHeld = Input.GetKey(KeyCode.Space);
        bool leftMouseButtonIsHeld = Input.GetMouseButton(0);

        IsAccelerating = spaceKeyIsHeld || leftMouseButtonIsHeld;
    }
}
