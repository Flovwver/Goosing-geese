using GoosingGeese.Presentation;
using GoosingGeese.Profile;
using GoosingGeese.Racing;
using GoosingGeese.Settings;
using UnityEngine;

namespace GoosingGeese.Levels
{
    public sealed class GameSession : ILevelSession
    {
        private readonly PlayerProgressStore _store;
        private readonly LocalLeaderboard _leaderboard;
        private readonly Level _level;
        private readonly CampaignSettings _campaign;
        private readonly RaceController _race;

        public GameSession(Level level, CampaignSettings campaign, RaceController race, PlayerProgressStore store, LocalLeaderboard leaderboard)
        {
            _store = store;
            _leaderboard = leaderboard;
            _level = level;
            _campaign = campaign;
            _race = race;
            Progress = _store.Load(Resources.Load<RacerAppearanceCatalog>("RacerAppearanceCatalog"));
            Commands = new ProfileCommands(Progress, _store, _leaderboard);
            Text = new Localizer(Progress);
        }

        public Localizer Text { get; }
        public PlayerProgress Progress { get; }
        public ProfileCommands Commands { get; }
        public CampaignStage CurrentStage => _campaign.GetStage(Progress.Level);
        public int SelectedLevelNumber => Progress.Level;
        public int LevelNumber => _level.Number;
        public bool HasNextLevel => _level.Number < int.MaxValue;
        public bool IsRacing => _race.isActiveAndEnabled && _race.State == RaceState.Racing;
        public bool IsWaiting => _race.State == RaceState.Waiting;

        public System.Collections.Generic.List<LeaderboardEntry> ReadLeaderboard()
        {
            return _leaderboard.Read();
        }

        public void ValidatePreparation()
        {
            _race.ValidatePreparation();
        }

        public void PrepareAttempt()
        {
            _race.Prepare();
        }

        public int RecordResult(bool won)
        {
            CampaignStage stage = _campaign.GetStage(_level.Number);
            int reward = Progress.RecordRace(won, _level.Number, stage.Reward, stage.FirstClearBonus);

            try
            {
                _store.Save(Progress);
                _leaderboard.Record(Progress.PlayerName, Progress.Points, Progress.PlayerId);
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception, _race);
            }

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
                _store.Save(Progress);
                return true;
            }

            Progress.TrySelectLevel(previous, int.MaxValue);
            _store.Save(Progress);
            Debug.LogError("Level selection failed: " + error, _level);
            return false;
        }
    }
}
