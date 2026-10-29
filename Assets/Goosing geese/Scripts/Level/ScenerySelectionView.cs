using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class ScenerySelectionView
{
    private readonly PlayerProgress _progress;
    private readonly Localizer _text;
    private readonly ProfileCommands _commands;
    private readonly GameUiFactory _ui;

    public ScenerySelectionView(PlayerProgress progress, GameUiFactory ui, ProfileCommands commands)
    {
        _progress = progress;
        _text = new Localizer(progress);
        _commands = commands;
        _ui = ui;
    }

    public void Render(Transform parent, Action back, Action rebuild)
    {
        _ui.CreateButton(parent, _text.Choose("Сменить окружение", "Change scenery"), () =>
        {
            _commands.NextScenery(3);
            rebuild();
        });
        _ui.CreateButton(parent, _text.Choose("Назад", "Back"), back);
    }

}
