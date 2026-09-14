using UnityEngine;

public class InputSwitch : MonoBehaviour
{
    [SerializeField] private IInputSource _touchSource;   
    [SerializeField] private IInputSource _keyboardSource; 
    [SerializeField] private Racer _racer;


    private void Update()
    {
        bool touchIsAccelerating = _touchSource != null && _touchSource.IsAccelerating;
        bool keyboardIsAccelerating = _keyboardSource != null && _keyboardSource.IsAccelerating;
        bool isAccelerating = touchIsAccelerating || keyboardIsAccelerating;

        if (isAccelerating == false)
        {
            _racer.Cooldown();
        }
        else
        {
            _racer.Accelerate();
        }
    }
}
