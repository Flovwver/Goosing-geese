using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class QuestsView
{
    private readonly PlayerProgress _progress;
    private readonly ProfileCommands _commands;
    private readonly GameUiFactory _ui;

    public QuestsView(PlayerProgress progress, GameUiFactory ui)
    {
        _progress = progress;
        _commands = new ProfileCommands(progress);
        _ui = ui;
    }

    public void Render(Transform parent, Action back, Action refresh)
    {
        string[] names = { Localize("Сегодня: 3 гонки", "Today: 3 races"), Localize("Сегодня: 1 победа", "Today: 1 win"), Localize("Всего: 10 побед", "Total: 10 wins") };

        for (int i = 0; i < names.Length; i++)
        {
            int quest = i;
            string title = names[i] + "  " + QuestTracker.GetProgress(_progress, i, DateTime.Today) + "/" + QuestTracker.GetTarget(i);
            var button = _ui.CreateButton(parent, title, () =>
            {
                _commands.ClaimQuest(quest, DateTime.Today);
                refresh();
            });
            button.interactable = QuestTracker.IsClaimed(_progress, i, DateTime.Today) == false && QuestTracker.GetProgress(_progress, i, DateTime.Today) >= QuestTracker.GetTarget(i);
        }

        _ui.CreateLabel(parent, Localize("Нажми выполненное задание, чтобы получить очки", "Tap a completed quest to claim points"), 22);
        _ui.CreateButton(parent, Localize("Назад", "Back"), back);
    }

    private string Localize(string russian, string english)
    {
        return _progress.English ? english : russian;
    }

}
