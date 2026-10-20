using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class RaceController : MonoBehaviour
{
    [SerializeField] private Level _level;
    [SerializeField] private Racer _player;
    [SerializeField] private InputSwitch _input;

    public event Action<RaceState> StateChanged;
    public event Action<bool> RaceEnded;

    public RaceState State { get; private set; } = RaceState.Waiting;
    public bool PlayerWon { get; private set; }
    public RaceOutcome Outcome { get; private set; }
    public PlayerProgress Progress { get; private set; }
    public int LastReward { get; private set; }

    private void Awake()
    {
        Progress = PlayerProgress.Load();

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
        _player.RaceRestarted += OnRestarted;
    }

    private void OnDisable()
    {
        _player.Finished -= OnPlayerFinished;
        _player.Exploded -= OnPlayerExploded;
        _player.PathCompleted -= OnPathCompleted;
        _player.RaceRestarted -= OnRestarted;
        Time.timeScale = 1f;
    }

    public void StartRace()
    {
        if (State != RaceState.Waiting)
            return;

        SetState(RaceState.Racing);
    }

    public void Pause()
    {
        if (State != RaceState.Racing)
            return;

        SetState(RaceState.Paused);
        _input.ResetInput();
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (State != RaceState.Paused)
            return;

        _input.ResetInput();
        Time.timeScale = 1f;
        SetState(RaceState.Racing);
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        _level.RestartRace();
    }

    public void NextLevel()
    {
        if (State != RaceState.Result || PlayerWon == false)
            return;

        Time.timeScale = 1f;
        _level.BuildRoad();
        _player.ResetRace();
    }

    public void ReturnToMenu()
    {
        if (State == RaceState.Result && PlayerWon)
            NextLevel();
        else
            Retry();
    }

    public void EndRace(bool playerWon, RaceOutcome outcome = RaceOutcome.OpponentFinished)
    {
        if (State != RaceState.Racing)
            return;

        PlayerWon = playerWon;
        Outcome = playerWon ? RaceOutcome.FinishedFirst : outcome;
        LastReward = Progress.RecordRace(playerWon);
        SetState(RaceState.Result);
        _input.ResetInput();

        if (playerWon == false)
            _player.StopMovement();

        RaceEnded?.Invoke(playerWon);
    }

    private void SetState(RaceState state)
    {
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

    private void OnRestarted()
    {
        PlayerWon = false;
        _input.ResetInput();
        SetState(RaceState.Waiting);
    }
}
