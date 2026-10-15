using System;
using UnityEngine;

public class RaceController : MonoBehaviour
{
    [SerializeField] private Racer _racer;
    [SerializeField] private InputSwitch _input;

    private RaceState _stateBeforePause;
    private float _resumeTimeScale = 1f;
    private bool _ownsPause;

    public event Action<RaceState> StateChanged;
    public event Action<RaceResult> Completed;
    public event Action Prepared;

    public RaceState State { get; private set; } = RaceState.Ready;
    public RaceResult Result { get; private set; }

    private void OnEnable()
    {
        _racer.Finished += OnFinished;
        _racer.Exploded += OnExploded;

        if (State == RaceState.Paused)
            SuspendTime();

        _input.SetInputEnabled(State == RaceState.Ready || State == RaceState.Race);
    }

    private void Update()
    {
        if (State == RaceState.Ready && _input.IsAccelerate && _racer.CanRace)
            SetState(RaceState.Race);

        if (State == RaceState.Race)
            _racer.Tick(_input.IsAccelerate, Time.deltaTime);
        else if (State == RaceState.Result && Result == RaceResult.Finished)
            _racer.Tick(false, Time.deltaTime);
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
        _racer.Finished -= OnFinished;
        _racer.Exploded -= OnExploded;
        _racer.StopMovement();
        RestoreTimeScale();
        _input.SetInputEnabled(false);
    }

    public void Pause()
    {
        if (isActiveAndEnabled == false || State == RaceState.Paused)
            return;

        _stateBeforePause = State;
        SuspendTime();
        _input.SetInputEnabled(false);
        SetState(RaceState.Paused);
    }

    public void Resume()
    {
        if (isActiveAndEnabled == false || State != RaceState.Paused)
            return;

        RestoreTimeScale();
        _input.SetInputEnabled(_stateBeforePause == RaceState.Ready || _stateBeforePause == RaceState.Race);
        SetState(_stateBeforePause);
    }

    public void Prepare()
    {
        RestoreTimeScale();
        Result = RaceResult.None;
        _input.SetInputEnabled(isActiveAndEnabled);
        SetState(RaceState.Ready);
        Prepared?.Invoke();
    }

    private void Complete(RaceResult result)
    {
        if (State != RaceState.Race)
            return;

        Result = result;
        _input.SetInputEnabled(false);
        SetState(RaceState.Result);
        Completed?.Invoke(Result);
    }

    private void OnExploded()
    {
        Complete(RaceResult.Overheated);
    }

    private void OnFinished()
    {
        Complete(RaceResult.Finished);
    }

    private void RestoreTimeScale()
    {
        if (_ownsPause == false)
            return;

        Time.timeScale = _resumeTimeScale;
        _ownsPause = false;
    }

    private void SetState(RaceState state)
    {
        if (State == state)
            return;

        State = state;
        StateChanged?.Invoke(State);
    }

    private void SuspendTime()
    {
        _resumeTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _ownsPause = true;
    }
}
