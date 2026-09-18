using UnityEngine;

public class InputSwitch : MonoBehaviour
{
    [SerializeField] private Racer _racer;

    private bool _pedalIsPressed;
    private bool _keyboardIsPressed;

    public void SetPedalPressed(bool isPressed)
    {
        _pedalIsPressed = isPressed;
    }

    public void SetKeyboardPressed(bool isPressed)
    {
        _keyboardIsPressed = isPressed;
    }

    private void Update()
    {
        bool isAccelerating = _pedalIsPressed || _keyboardIsPressed;

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