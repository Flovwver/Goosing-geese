using System;
using GoosingGeese.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    public sealed class QuestsView
    {
        private readonly QuestDefinitions _definitions = new QuestDefinitions();
        private readonly PlayerProgress _progress;
        private readonly Localizer _text;
        private readonly ProfileCommands _commands;
        private readonly GameUiFactory _ui;

        public QuestsView(PlayerProgress progress, GameUiFactory ui, ProfileCommands commands)
        {
            _progress = progress;
            _text = new Localizer(progress);
            _commands = commands;
            _ui = ui;
        }

        public void Render(Transform parent, Action back, Action refresh)
        {
            var definitions = _definitions.Entries;

            for (int i = 0; i < definitions.Count; i++)
            {
                int quest = (int)definitions[i].Id;
                string title = _text.Choose(definitions[i].RussianTitle,
                    definitions[i].EnglishTitle)
                    + "  "
                    + _progress.Quests.GetProgress(i,
                    DateTime.Today)
                    + "/"
                    + definitions[i].Target;
                var button = _ui.CreateButton(parent, title, () =>
                {
                    _commands.ClaimQuest(quest, DateTime.Today);
                    refresh();
                });
                button.interactable = _progress.Quests.IsClaimed(i,
                    DateTime.Today) == false
                    && _progress.Quests.GetProgress(i,
                    DateTime.Today) >= definitions[i].Target;
            }

            _ui.CreateLabel(parent, _text.Choose("Нажми выполненное задание, чтобы получить очки", "Tap a completed quest to claim points"), 22);
            _ui.CreateButton(parent, _text.Choose("Назад", "Back"), back);
        }
    }
}
