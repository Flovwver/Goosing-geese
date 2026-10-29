using UnityEngine;
using UnityEngine.InputSystem;

namespace GoosingGeese.Input
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "KeyboardAccelerator")]
    public class KeyboardAccelerator : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference _accelerateAction;
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_inputSwitch")]
        private ThrottleInput _input;

        private void OnEnable()
        {
            if (_input == null || _accelerateAction == null || _accelerateAction.action == null)
                throw new System.InvalidOperationException("Assign the keyboard action and throttle input in the Inspector.");

            _accelerateAction.action.performed += HandleAccelerationPerformed;
            _accelerateAction.action.canceled += HandleAccelerationCanceled;
            _accelerateAction.action.Enable();
        }

        private void OnDisable()
        {
            if (_accelerateAction != null && _accelerateAction.action != null)
            {
                _accelerateAction.action.performed -= HandleAccelerationPerformed;
                _accelerateAction.action.canceled -= HandleAccelerationCanceled;
                _accelerateAction.action.Disable();
            }

            if (_input != null)
                _input.SetKeyboardPressed(false);
        }

        private void HandleAccelerationPerformed(InputAction.CallbackContext context)
        {
            _input.SetKeyboardPressed(true);
        }

        private void HandleAccelerationCanceled(InputAction.CallbackContext context)
        {
            _input.SetKeyboardPressed(false);
        }
    }
}
