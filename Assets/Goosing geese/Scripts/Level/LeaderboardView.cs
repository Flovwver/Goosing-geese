using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class LeaderboardView
{
    private readonly PlayerProgress _progress;
    private readonly GameUiFactory _ui;

    public LeaderboardView(PlayerProgress progress, GameUiFactory ui)
    {
        _progress = progress;
        _ui = ui;
    }

    public void Render(Transform parent, Action back)
    {
        var entries = LocalLeaderboard.Read();
        string rows = "";

        for (int i = 0; i < entries.Count; i++)
        {
            bool currentPlayer = entries[i].Name == _progress.PlayerName;

            if (i < 5 || currentPlayer)
                rows += LocalLeaderboard.GetRank(entries, i) + ". " + entries[i].Name + " — " + entries[i].Score + (currentPlayer ? " •" : "") + "\n";
        }

        if (entries.Find(entry => entry.Name == _progress.PlayerName) == null)
            rows += (entries.Count + 1) + ". " + _progress.PlayerName + " — " + _progress.Points + " •";

        _ui.CreateLabel(parent, rows, 24).GetComponent<LayoutElement>().preferredHeight = 170f;
        _ui.CreateLabel(parent, L("Рекорд этого устройства. Онлайн-рейтинг пока не подключён.", "Device record. Online rankings are not connected."), 23);
        _ui.CreateButton(parent, L("Назад", "Back"), back);
    }

    private string L(string russian, string english)
    {
        return _progress.English ? english : russian;
    }

}
