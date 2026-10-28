using UnityEngine;

public sealed class GameSession
{
    private readonly Level _level;
    private readonly CampaignSettings _campaign;
    private readonly RaceController _race;

    public GameSession(Level level, CampaignSettings campaign, RaceController race)
    {
        _level = level;
        _campaign = campaign;
        _race = race;
        Progress = PlayerProgressStore.Load();
        Progress.Level = Mathf.Clamp(Progress.Level, 1, Progress.GetUnlockedLevel(int.MaxValue));
    }

    public PlayerProgress Progress { get; }
    public CampaignStage CurrentStage => _campaign.GetStage(Progress.Level);
    public bool HasNextLevel => _level.Number < int.MaxValue;
    public bool IsRacing => _race.isActiveAndEnabled && _race.State == RaceState.Racing;
    public bool IsWaiting => _race.State == RaceState.Waiting;

    public void PrepareAttempt()
    {
        _race.Prepare();
    }

    public int RecordResult(bool won)
    {
        CampaignStage stage = _campaign.GetStage(_level.Number);
        int reward = Progress.RecordRace(won, _level.Number, stage.Reward, stage.FirstClearBonus);
        PlayerProgressStore.Save(Progress);
        LocalLeaderboard.Record(Progress.PlayerName, Progress.Points);
        return reward;
    }

    public void Retry()
    {
        _level.RestartRace();
    }

    public void NextLevel()
    {
        if (_race.State != RaceState.Result || _race.PlayerWon == false || HasNextLevel == false)
            return;

        TrySelectLevel(_level.Number + 1);
    }

    public void ReturnToMenu()
    {
        Retry();
    }

    public bool SelectLevel(int number)
    {
        return _race.State == RaceState.Waiting && TrySelectLevel(number);
    }

    private bool TrySelectLevel(int number)
    {
        int previous = Progress.Level;

        if (Progress.TrySelectLevel(number, int.MaxValue) == false)
            return false;

        if (_level.TryBuildRoad(out string error))
        {
            PlayerProgressStore.Save(Progress);
            return true;
        }

        Progress.Level = previous;
        PlayerProgressStore.Save(Progress);
        Debug.LogError("Level selection failed: " + error, _level);
        return false;
    }
}

