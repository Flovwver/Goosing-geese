using System;
using GoosingGeese.Profile;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    public sealed class SettingsView
    {
        private readonly PlayerProgress _progress;
        private readonly Localizer _text;
        private readonly ProfileCommands _commands;
        private readonly GameUiFactory _ui;

        public SettingsView(PlayerProgress progress, GameUiFactory ui, ProfileCommands commands)
        {
            _progress = progress;
            _text = new Localizer(progress);
            _commands = commands;
            _ui = ui;
        }

        public void Save()
        {
            _commands.Save();
        }

        public void Render(Transform parent, Action reopen, Action quests, Action scenery, Action back)
        {
            AddNameField(parent);
            AddVolume(parent, _text.Get("music"), _progress.MusicVolume, _commands.SetMusicVolume);
            AddVolume(parent, _text.Get("effects"), _progress.EffectsVolume, _commands.SetEffectsVolume);
            _ui.CreateButton(parent, _text.Get("language.switch"), () =>
            {
                _commands.ToggleLanguage();
                reopen();
            });
            _ui.CreateButton(parent, _text.Get("quests"), quests);
            _ui.CreateButton(parent, _text.Get("scenery"), scenery);
            _ui.CreateButton(parent, _text.Get("back"), () =>
            {
                _commands.Save();
                back();
            });
        }

        private void AddNameField(Transform parent)
        {
            var rect = _ui.CreateRect("Player name", parent, Vector2.zero, Vector2.one);
            rect.gameObject.AddComponent<Image>().color = _ui.Theme.Button;
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 55f;
            var input = rect.gameObject.AddComponent<InputField>();
            input.textComponent = _ui.CreateLabel(rect, "", 24);
            input.characterLimit = 20;
            input.text = _progress.PlayerName;
            input.onEndEdit.AddListener(_commands.SetName);
        }

        private void AddVolume(Transform parent, string title, float value, UnityAction<float> apply)
        {
            _ui.CreateLabel(parent, title, 24);
            var rect = _ui.CreateRect(title, parent, Vector2.zero, Vector2.one);
            rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 45f;
            rect.gameObject.AddComponent<Image>().color = _ui.Theme.Button;
            var slider = rect.gameObject.AddComponent<Slider>();
            var fill = _ui.CreateRect("Fill", rect, Vector2.zero, Vector2.one);
            fill.gameObject.AddComponent<Image>().color = new Color(0.4f, 0.9f, 0.65f);
            slider.fillRect = fill;
            slider.value = value;
            slider.onValueChanged.AddListener(apply);
        }
    }
}
