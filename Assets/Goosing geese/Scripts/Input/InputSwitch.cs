using UnityEngine;

public class InputSwitch : MonoBehaviour
{
    [SerializeField] private Racer _racer;

    private bool _pedalIsPressed;
    private bool _keyboardIsPressed;

    private bool _hasFocus = true;
    private bool _isPaused;

    private void OnEnable()
    {
        _racer.RaceRestarted += ResetInput;
    }

    private void OnDisable()
    {
        if (_racer != null)
            _racer.RaceRestarted -= ResetInput;

        ResetInput();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        _hasFocus = hasFocus;

        if (hasFocus == false)
            ResetInput();
    }

    private void OnApplicationPause(bool isPaused)
    {
        _isPaused = isPaused;

        if (isPaused)
            ResetInput();
    }

    public void ResetInput()
    {
        _pedalIsPressed = false;
        _keyboardIsPressed = false;

        if (_racer != null)
            _racer.Cooldown();
    }

    public void SetPedalPressed(bool isPressed)
    {
        _pedalIsPressed = isPressed && isActiveAndEnabled && _hasFocus && _isPaused == false;
    }

    public void SetKeyboardPressed(bool isPressed)
    {
        _keyboardIsPressed = isPressed && isActiveAndEnabled && _hasFocus && _isPaused == false;
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
