using UnityEngine;

public class InputSwitch : MonoBehaviour
{
    [SerializeField] private Racer _racer;

    private bool _pedalIsPressed;
    private bool _keyboardIsPressed;

    private bool _hasFocus = true;
    private bool _isPaused;
    private RaceController _race;

    private void Awake()
    {
        _race = FindFirstObjectByType<RaceController>();
    }

    private void OnEnable()
    {
        _racer.RaceRestarted += ResetInput;
        _racer.Finished += ResetInput;
    }

    private void Update()
    {
        if (_racer.IsFinished)
            return;

        if (_race != null)
        {
            if (_race.State == RaceState.Waiting && (_pedalIsPressed || _keyboardIsPressed))
                _race.StartRace();

            if (_race.State != RaceState.Racing)
                return;
        }

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

    private void OnApplicationFocus(bool hasFocus)
    {
        _hasFocus = hasFocus;

        if (hasFocus == false && _race != null)
            _race.Pause();

        if (hasFocus == false)
            ResetInput();
    }

    private void OnApplicationPause(bool isPaused)
    {
        _isPaused = isPaused;

        if (isPaused && _race != null)
            _race.Pause();

        if (isPaused)
            ResetInput();
    }

    private void OnDisable()
    {
        if (_racer != null)
        {
            _racer.RaceRestarted -= ResetInput;
            _racer.Finished -= ResetInput;
        }

        ResetInput();
    }

    public void ResetInput()
    {
        _pedalIsPressed = false;
        _keyboardIsPressed = false;

        if (_racer != null)
            _racer.ReleaseThrottle();
    }

    public void SetPedalPressed(bool isPressed)
    {
        _pedalIsPressed = isPressed && isActiveAndEnabled && _hasFocus && _isPaused == false && _racer.IsFinished == false;
    }

    public void SetKeyboardPressed(bool isPressed)
    {
        _keyboardIsPressed = isPressed && isActiveAndEnabled && _hasFocus && _isPaused == false && _racer.IsFinished == false;
    }

}
