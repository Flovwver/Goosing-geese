using System;
using UnityEngine;

namespace GoosingGeese.Profile
{
    public sealed class ProfileCommands
    {
        private readonly PlayerProgress _progress;
        private readonly PlayerProgressStore _store;
        private readonly LocalLeaderboard _leaderboard;

        public ProfileCommands(PlayerProgress progress, PlayerProgressStore store, LocalLeaderboard leaderboard)
        {
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _leaderboard = leaderboard ?? throw new ArgumentNullException(nameof(leaderboard));
        }

        public void SetName(string name)
        {
            _progress.SetName(name);
            Save();
        }

        public void SetMusicVolume(float value)
        {
            _progress.SetMusicVolume(value);
        }

        public void SetEffectsVolume(float value)
        {
            _progress.SetEffectsVolume(value);
        }

        public void ToggleLanguage()
        {
            _progress.ToggleLanguage();
            Save();
        }

        public void NextScenery(int count)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));

            _progress.NextScenery(count);
            Save();
        }

        public bool ClaimQuest(int quest, DateTime date)
        {
            if (_progress.Quests.TryClaim(quest, date) == false)
                return false;

            Save();
            _leaderboard.Record(_progress.PlayerName, _progress.Points, _progress.PlayerId);
            return true;
        }

        public void Save()
        {
            _store.Save(_progress);
        }
    }
}
