using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class QuestsView
{
    private readonly PlayerProgress _progress;
    private readonly GameUiFactory _ui;

    public QuestsView(PlayerProgress progress, GameUiFactory ui)
    {
        _progress = progress;
        _ui = ui;
    }

    public void Render(Transform parent, Action back, Action refresh)
    {
        string[] names = { L("Сегодня: 3 гонки", "Today: 3 races"), L("Сегодня: 1 победа", "Today: 1 win"), L("Всего: 10 побед", "Total: 10 wins") };

        for (int i = 0; i < names.Length; i++)
        {
            int quest = i;
            string title = names[i] + "  " + QuestTracker.GetProgress(_progress, i, DateTime.Today) + "/" + QuestTracker.GetTarget(i);
            var button = _ui.CreateButton(parent, title, () =>
            {
                if (QuestTracker.TryClaim(_progress, quest, DateTime.Today))
                {
                    PlayerProgressStore.Save(_progress);
                    LocalLeaderboard.Record(_progress.PlayerName, _progress.Points);
                }

                refresh();
            });
            button.interactable = !QuestTracker.IsClaimed(_progress, i, DateTime.Today) && QuestTracker.GetProgress(_progress, i, DateTime.Today) >= QuestTracker.GetTarget(i);
        }

        _ui.CreateLabel(parent, L("Нажми выполненное задание, чтобы получить очки", "Tap a completed quest to claim points"), 22);
        _ui.CreateButton(parent, L("Назад", "Back"), back);
    }

    private string L(string russian, string english)
    {
        return _progress.English ? english : russian;
    }

}
