using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class RaceController : MonoBehaviour
{
    [SerializeField] private Level _level;
    [SerializeField] private Racer _player;
    [SerializeField] private InputSwitch _input;
    [SerializeField] private CampaignSettings _campaign;

    private RaceState _stateBeforePause;
    private float _resumeTimeScale = 1f;
    private bool _ownsPause;

    public event Action<RaceState> StateChanged;
    public event Action<bool> RaceEnded;
    public event Action Prepared;
    public event Action Disabled;

    public RaceState State { get; private set; } = RaceState.Waiting;
    public bool PlayerWon { get; private set; }
    public RaceOutcome Outcome { get; private set; } = RaceOutcome.None;
    public PlayerProgress Progress { get; private set; }
    public int LastReward { get; private set; }
    public CampaignSettings Campaign => _campaign;
    public CampaignStage CurrentStage => _campaign.GetStage(Progress.Level);
    public bool HasNextLevel => _level.Number < int.MaxValue;

    private void Awake()
    {
        Progress = PlayerProgress.Load();
        Progress.Level = Mathf.Clamp(Progress.Level, 1, Progress.GetUnlockedLevel(int.MaxValue));

        if (_level == null)
            _level = GetComponent<Level>();

        if (_player == null)
            _player = FindFirstObjectByType<Racer>();

        if (_input == null)
            _input = FindFirstObjectByType<InputSwitch>();
    }

    private void OnEnable()
    {
        _player.Finished += OnPlayerFinished;
        _player.Exploded += OnPlayerExploded;
        _player.PathCompleted += OnPathCompleted;
        _input.Cleared += _player.ReleaseThrottle;

        if (State == RaceState.Paused)
            SuspendTime();

        _input.SetInputEnabled(State == RaceState.Waiting || State == RaceState.Racing);
    }

    private void Update()
    {
        if (State == RaceState.Waiting && _input.IsThrottlePressed && _player.CanRace)
            StartRace();

        if (State == RaceState.Racing)
            _player.Tick(_input.IsThrottlePressed, Time.deltaTime);
        else if (State == RaceState.Result && PlayerWon)
            _player.Tick(false, Time.deltaTime);
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
        _player.Finished -= OnPlayerFinished;
        _player.Exploded -= OnPlayerExploded;
        _player.PathCompleted -= OnPathCompleted;
        _player.StopMovement();
        _input.SetInputEnabled(false);
        _input.Cleared -= _player.ReleaseThrottle;
        RestoreTimeScale();
        Disabled?.Invoke();
    }

    public void StartRace()
    {
        if (isActiveAndEnabled == false || State != RaceState.Waiting || _player.CanRace == false)
            return;

        SetState(RaceState.Racing);
    }

    public void Pause()
    {
        if (isActiveAndEnabled == false || State != RaceState.Racing)
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
        _input.SetInputEnabled(true);
        SetState(_stateBeforePause);
    }

    public void Prepare()
    {
        RestoreTimeScale();
        PlayerWon = false;
        Outcome = RaceOutcome.None;
        LastReward = 0;
        _input.SetInputEnabled(isActiveAndEnabled);
        SetState(RaceState.Waiting);
        Prepared?.Invoke();
    }

    public void Retry()
    {
        _level.RestartRace();
    }

    public void NextLevel()
    {
        if (State != RaceState.Result || PlayerWon == false || HasNextLevel == false)
            return;

        TrySelectLevel(_level.Number + 1);
    }

    public void ReturnToMenu()
    {
        Retry();
    }

    public bool SelectLevel(int number)
    {
        return State == RaceState.Waiting && TrySelectLevel(number);
    }

    public void EndRace(bool playerWon, RaceOutcome outcome = RaceOutcome.OpponentFinished)
    {
        if (isActiveAndEnabled == false || State != RaceState.Racing)
            return;

        PlayerWon = playerWon;
        Outcome = playerWon ? RaceOutcome.FinishedFirst : outcome;
        CampaignStage stage = _campaign.GetStage(_level.Number);
        LastReward = Progress.RecordRace(playerWon, _level.Number, stage.Reward, stage.FirstClearBonus);
        _input.SetInputEnabled(false);
        SetState(RaceState.Result);

        if (playerWon == false)
            _player.StopMovement();

        RaceEnded?.Invoke(playerWon);
    }

    private bool TrySelectLevel(int number)
    {
        int previous = Progress.Level;

        if (Progress.TrySelectLevel(number, int.MaxValue) == false)
            return false;

        if (_level.TryBuildRoad(out string error))
            return true;

        Progress.Level = previous;
        Progress.Save();
        Debug.LogError("Level selection failed: " + error, this);
        return false;
    }

    private void SetState(RaceState state)
    {
        if (State == state)
            return;

        State = state;
        StateChanged?.Invoke(state);
    }

    private void OnPlayerFinished()
    {
        EndRace(true);
    }

    private void OnPlayerExploded()
    {
        EndRace(false, RaceOutcome.Overheated);
    }

    private void OnPathCompleted()
    {
        if (State == RaceState.Racing)
            EndRace(false, RaceOutcome.RouteEnded);
    }

    private void RestoreTimeScale()
    {
        if (_ownsPause == false)
            return;

        Time.timeScale = _resumeTimeScale;
        _ownsPause = false;
    }

    private void SuspendTime()
    {
        if (_ownsPause)
            return;

        _resumeTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        _ownsPause = true;
    }
}
