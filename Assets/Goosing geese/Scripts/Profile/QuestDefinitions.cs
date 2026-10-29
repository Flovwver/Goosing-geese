using System;
using System.Collections.Generic;

namespace GoosingGeese.Profile
{
    public sealed class QuestDefinitions
    {
        private readonly IReadOnlyList<QuestDefinition> _entries = Array.AsReadOnly(new[] { new QuestDefinition(QuestId.DailyRaces,
            3,
            50,
            "Сегодня: 3 гонки",
            "Today: 3 races"), new QuestDefinition(QuestId.DailyWin,
            1,
            75,
            "Сегодня: 1 победа",
            "Today: 1 win"), new QuestDefinition(QuestId.TotalWins,
            10,
            300,
            "Всего: 10 побед",
            "Total: 10 wins") });

        public IReadOnlyList<QuestDefinition> Entries => _entries;

        public QuestDefinition Get(int quest)
        {
            if (quest < 0 || quest >= _entries.Count)
                throw new ArgumentOutOfRangeException(nameof(quest));

            return _entries[quest];
        }

        public int GetTarget(int quest)
        {
            return Get(quest).Target;
        }
    }
}
