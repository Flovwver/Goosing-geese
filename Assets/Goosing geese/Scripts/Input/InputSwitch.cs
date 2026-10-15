using UnityEngine;

public class InputSwitch : MonoBehaviour
{
    private bool _pedalIsPressed;
    private bool _keyboardIsPressed;
    private bool _hasFocus = true;
    private bool _isPaused;
    private bool _canInput = true;

    public bool IsAccelerate => _pedalIsPressed || _keyboardIsPressed;

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

    private void OnDisable()
    {
        ResetInput();
    }

    public void ResetInput()
    {
        _pedalIsPressed = false;
        _keyboardIsPressed = false;
    }

    public void SetInputEnabled(bool isEnabled)
    {
        _canInput = isEnabled;
        ResetInput();
    }

    public void SetPedalPressed(bool isPressed)
    {
        _pedalIsPressed = isPressed && _canInput && isActiveAndEnabled && _hasFocus && _isPaused == false;
    }

    public void SetKeyboardPressed(bool isPressed)
    {
        _keyboardIsPressed = isPressed && _canInput && isActiveAndEnabled && _hasFocus && _isPaused == false;
    }
}
