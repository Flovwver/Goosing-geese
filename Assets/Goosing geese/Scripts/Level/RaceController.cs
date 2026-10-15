using System;
using UnityEngine;

public class RaceController : MonoBehaviour
{
    [SerializeField] private Racer _racer;
    [SerializeField] private InputSwitch _input;

    private RaceState _stateBeforePause;
    private float _resumeTimeScale = 1f;

    public event Action<RaceState> StateChanged;

    public RaceState State { get; private set; } = RaceState.Ready;
    public RaceResult Result { get; private set; }

    private void OnEnable()
    {
        _racer.RaceRestarted += OnRaceRestarted;
        _input.SetInputEnabled(State == RaceState.Ready || State == RaceState.Race);
    }

    private void Update()
    {
        if (State == RaceState.Ready && _input.IsAccelerate && _racer.CanRace)
            SetState(RaceState.Race);

        if (State == RaceState.Race)
            _racer.Tick(_input.IsAccelerate);
        else if (State == RaceState.Result && Result == RaceResult.Finished)
            _racer.Tick(false);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus == false)
            Pause();
    }

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
            Pause();
    }

    private void OnDisable()
    {
        _racer.RaceRestarted -= OnRaceRestarted;
        Resume();
        _input.ResetInput();
    }

    public void Complete(RaceResult result)
    {
        if (State != RaceState.Race || result == RaceResult.None)
            return;

        Result = result;
        _input.SetInputEnabled(false);
        SetState(RaceState.Result);
    }

    public void Pause()
    {
        if (State == RaceState.Paused)
            return;

        _stateBeforePause = State;
        _resumeTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _input.SetInputEnabled(false);
        SetState(RaceState.Paused);
    }

    public void Resume()
    {
        if (State != RaceState.Paused)
            return;

        RestoreTimeScale();
        _input.SetInputEnabled(_stateBeforePause == RaceState.Ready || _stateBeforePause == RaceState.Race);
        SetState(_stateBeforePause);
    }

    private void OnRaceRestarted()
    {
        RestoreTimeScale();
        Result = RaceResult.None;
        _input.SetInputEnabled(true);
        SetState(RaceState.Ready);
    }

    private void RestoreTimeScale()
    {
        if (State == RaceState.Paused)
            Time.timeScale = _resumeTimeScale;
    }

    private void SetState(RaceState state)
    {
        if (State == state)
            return;

        State = state;
        StateChanged?.Invoke(State);
    }
}
