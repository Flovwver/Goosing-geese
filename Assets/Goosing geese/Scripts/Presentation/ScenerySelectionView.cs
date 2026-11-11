using System;
using GoosingGeese.Profile;
using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
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

        public void Render(Transform parent, Action back, Action refresh)
        {
            _ui.CreateLabel(parent, GetThemeName(), 28);
            _ui.CreateButton(parent, _text.Choose("Сменить окружение", "Change scenery"), () =>
            {
                _commands.NextScenery(3);
                refresh();
            });
            _ui.CreateLabel(parent, _text.Choose("Соперник сменится на новой трассе", "The rival changes on a new track"), 20);
            _ui.CreateButton(parent, _text.Choose("Назад", "Back"), back);
        }

        private string GetThemeName()
        {
            switch (_progress.SelectedSceneryIndex)
            {
                case 1:
                    return _text.Choose("Горы", "Mountains");
                case 2:
                    return _text.Choose("Город", "City");
                default:
                    return _text.Choose("Сельская местность", "Countryside");
            }
        }
    }
}
