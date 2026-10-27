using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class ScenerySelectionView
{
    private readonly PlayerProgress _progress;
    private readonly GameUiFactory _ui;

    public ScenerySelectionView(PlayerProgress progress, GameUiFactory ui)
    {
        _progress = progress;
        _ui = ui;
    }

    public void Render(Transform parent, Action back, Action rebuild)
    {
        _ui.CreateButton(parent, L("Сменить окружение", "Change scenery"), () =>
        {
            _progress.Environment = (_progress.Environment + 1) % 3;
            PlayerProgressStore.Save(_progress);
            rebuild();
        });
        _ui.CreateButton(parent, L("Назад", "Back"), back);
    }

    private string L(string russian, string english)
    {
        return _progress.English ? english : russian;
    }

}
