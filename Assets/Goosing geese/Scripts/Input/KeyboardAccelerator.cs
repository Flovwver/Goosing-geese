using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardAccelerator : MonoBehaviour
{
    [SerializeField] private InputActionReference _accelerateAction;
    [SerializeField] private InputSwitch _inputSwitch;

    private void OnEnable()
    {
        _accelerateAction.action.performed += HandleAccelerationPerformed;
        _accelerateAction.action.canceled += HandleAccelerationCanceled;
        _accelerateAction.action.Enable();
    }

    private void OnDisable()
    {
        _accelerateAction.action.performed -= HandleAccelerationPerformed;
        _accelerateAction.action.canceled -= HandleAccelerationCanceled;
        _accelerateAction.action.Disable();
    }

    private void HandleAccelerationPerformed(InputAction.CallbackContext context)
    {
        _inputSwitch.SetKeyboardPressed(true);
    }

    private void HandleAccelerationCanceled(InputAction.CallbackContext context)
    {
        _inputSwitch.SetKeyboardPressed(false);
    }
}