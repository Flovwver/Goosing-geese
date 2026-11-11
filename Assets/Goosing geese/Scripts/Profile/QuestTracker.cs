using System;

namespace GoosingGeese.Profile
{
    public sealed class QuestTracker
    {
        private readonly QuestDefinitions _definitions = new QuestDefinitions();
        private readonly PlayerProgressData _data;
        private readonly Action<int> _addReward;

        internal QuestTracker(PlayerProgressData data, Action<int> addReward)
        {
            _data = data;
            _addReward = addReward;
        }

        public void RefreshDay(DateTime date)
        {
            string today = date.ToString("yyyy-MM-dd");

            if (_data.DailyDate == today)
                return;

            _data.DailyDate = today;
            _data.DailyRaces = 0;
            _data.DailyWins = 0;
            _data.DailyRacesClaimed = false;
            _data.DailyWinsClaimed = false;
            _data.DailyOverheat = false;
            _data.DailyOverheatClaimed = false;
        }

        public void RecordRace(bool won, DateTime date, bool overheated = false)
        {
            RefreshDay(date);
            _data.Races = (int)Math.Min(int.MaxValue, (long)_data.Races + 1);
            _data.DailyRaces = (int)Math.Min(int.MaxValue, (long)_data.DailyRaces + 1);
            _data.DailyOverheat |= overheated;

            if (won)
            {
                _data.Wins = (int)Math.Min(int.MaxValue, (long)_data.Wins + 1);
                _data.DailyWins = (int)Math.Min(int.MaxValue, (long)_data.DailyWins + 1);
            }
        }

        public int GetProgress(int quest, DateTime date)
        {
            Validate(quest);
            bool currentDay = _data.DailyDate == date.ToString("yyyy-MM-dd");

            switch ((QuestId)quest)
            {
                case QuestId.DailyRaces:
                    return currentDay ? _data.DailyRaces : 0;
                case QuestId.DailyWin:
                    return currentDay ? _data.DailyWins : 0;
                case QuestId.DailyOverheat:
                    return currentDay && _data.DailyOverheat ? 1 : 0;
                default:
                    return _data.Wins;
            }
        }

        public int GetTarget(int quest)
        {
            Validate(quest);
            return _definitions.GetTarget(quest);
        }

        public bool IsClaimed(int quest, DateTime date)
        {
            Validate(quest);

            if ((QuestId)quest == QuestId.TotalWins)
                return _data.GlobalWinsClaimed;

            if (_data.DailyDate != date.ToString("yyyy-MM-dd"))
                return false;

            switch ((QuestId)quest)
            {
                case QuestId.DailyRaces:
                    return _data.DailyRacesClaimed;
                case QuestId.DailyWin:
                    return _data.DailyWinsClaimed;
                default:
                    return _data.DailyOverheatClaimed;
            }
        }

        public bool TryClaim(int quest, DateTime date)
        {
            if (quest < 0 || quest >= _definitions.Entries.Count)
                return false;

            RefreshDay(date);

            if (IsClaimed(quest, date) || GetProgress(quest, date) < GetTarget(quest))
                return false;

            int reward = _definitions.Get(quest).Reward;

            switch ((QuestId)quest)
            {
                case QuestId.DailyRaces:
                    _data.DailyRacesClaimed = true;
                    break;
                case QuestId.DailyWin:
                    _data.DailyWinsClaimed = true;
                    break;
                case QuestId.DailyOverheat:
                    _data.DailyOverheatClaimed = true;
                    break;
                default:
                    _data.GlobalWinsClaimed = true;
                    break;
            }

            _addReward(reward);
            return true;
        }

        private void Validate(int quest)
        {
            if (quest < 0 || quest >= _definitions.Entries.Count)
                throw new ArgumentOutOfRangeException(nameof(quest));
        }
    }
}
