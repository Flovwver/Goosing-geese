using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class RaceController : MonoBehaviour
{
    private readonly EventPublisher _events = new EventPublisher();

    [SerializeField] private Level _level;
    [SerializeField] private Racer _player;
    [SerializeField] private InputSwitch _input;
    [SerializeField] private CampaignSettings _campaign;

    private OpponentSpawner _opponents;
    private RaceState _stateBeforePause;
    private float _resumeTimeScale = 1f;
    private bool _ownsPause;
    private bool _inputBlocked;

    public event Action<RaceState> StateChanged;
    public event Action<bool> RaceEnded;
    public event Action Prepared;
    public event Action Disabled;

    public RaceState State { get; private set; } = RaceState.Waiting;
    public bool PlayerWon { get; private set; }
    public RaceOutcome Outcome { get; private set; } = RaceOutcome.None;
    public GameSession Session { get; private set; }
    public PlayerProgress Progress => Session.Progress;
    public int LastReward { get; private set; }
    public CampaignSettings Campaign => _campaign;
    public CampaignStage CurrentStage => Session.CurrentStage;
    public bool HasNextLevel => Session.HasNextLevel;

    private void Awake()
    {

        if (_level == null)
            _level = GetComponent<Level>();

        ValidatePreparation();

        Session = new GameSession(_level, _campaign, this, new PlayerProgressStore(), new LocalLeaderboard());
        _level.SetSession(Session);
    }

    private void OnEnable()
    {
        _opponents = GetComponent<OpponentSpawner>();

        if (Session == null)
        {
            Session = new GameSession(_level, _campaign, this, new PlayerProgressStore(), new LocalLeaderboard());
            _level.SetSession(Session);
        }

        _player.Finished += OnPlayerFinished;
        _player.Exploded += OnPlayerExploded;
        _player.PathCompleted += OnPathCompleted;
        _input.Cleared += _player.ReleaseThrottle;

        if (State == RaceState.Paused)
            SuspendTime();

        _input.ResetInput();
        UpdateInputAvailability();
    }

    private void Update()
    {
        if (State == RaceState.Waiting && _input.IsThrottlePressed && _player.CanRace)
            StartRace();

        if (State == RaceState.Racing)
            _player.Tick(_input.IsThrottlePressed, Time.deltaTime);
        else if (State == RaceState.Result && PlayerWon)
            _player.Tick(false, Time.deltaTime);

        if (State == RaceState.Racing && _opponents != null)
            _opponents.Tick(Time.deltaTime);
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
        _events.Publish(Disabled, this);
    }

    public void ValidatePreparation()
    {
        if (_player == null || _input == null || _campaign == null || _level == null)
            throw new InvalidOperationException("Assign the race player, input, campaign and level in the Inspector.");
    }

    public void SetInputBlocked(bool blocked)
    {
        _inputBlocked = blocked;
        UpdateInputAvailability();
    }

    public void StartRace()
    {
        if (isActiveAndEnabled == false || State != RaceState.Waiting || _inputBlocked || _player.CanRace == false)
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
        SetState(_stateBeforePause);
    }

    public void Prepare()
    {
        RestoreTimeScale();
        PlayerWon = false;
        Outcome = RaceOutcome.None;
        LastReward = 0;
        _input.ResetInput();
        SetState(RaceState.Waiting);
        _events.Publish(Prepared, this);
    }

    public void Retry()
    {
        Session.Retry();
    }

    public void NextLevel()
    {
        Session.NextLevel();
    }

    public void ReturnToMenu()
    {
        Session.ReturnToMenu();
    }

    public bool SelectLevel(int number)
    {
        return Session.SelectLevel(number);
    }

    public void EndRace(bool playerWon, RaceOutcome outcome = RaceOutcome.OpponentFinished)
    {
        if (isActiveAndEnabled == false || State != RaceState.Racing)
            return;

        PlayerWon = playerWon;
        Outcome = playerWon ? RaceOutcome.FinishedFirst : outcome;
        LastReward = Session.RecordResult(playerWon);
        _input.SetInputEnabled(false);
        SetState(RaceState.Result);

        if (playerWon == false)
            _player.StopMovement();

        _events.Publish(RaceEnded, playerWon, this);
    }

    

    private void SetState(RaceState state)
    {
        if (State == state)
        {
            UpdateInputAvailability();
            return;
        }

        State = state;
        UpdateInputAvailability();
        _events.Publish(StateChanged, state, this);
    }

    private void UpdateInputAvailability()
    {
        _input.SetInputEnabled(isActiveAndEnabled && _inputBlocked == false
            && (State == RaceState.Waiting || State == RaceState.Racing));
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
