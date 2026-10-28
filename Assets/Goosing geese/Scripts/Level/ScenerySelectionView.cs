using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class ScenerySelectionView
{
    private readonly PlayerProgress _progress;
    private readonly ProfileCommands _commands;
    private readonly GameUiFactory _ui;

    public ScenerySelectionView(PlayerProgress progress, GameUiFactory ui)
    {
        _progress = progress;
        _commands = new ProfileCommands(progress);
        _ui = ui;
    }

    public void Render(Transform parent, Action back, Action rebuild)
    {
        _ui.CreateButton(parent, Localize("Сменить окружение", "Change scenery"), () =>
        {
            _commands.NextScenery(3);
            rebuild();
        });
        _ui.CreateButton(parent, Localize("Назад", "Back"), back);
    }

    private string Localize(string russian, string english)
    {
        return _progress.English ? english : russian;
    }

}
