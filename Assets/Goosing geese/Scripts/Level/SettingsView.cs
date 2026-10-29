using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class SettingsView
{
    private readonly PlayerProgress _progress;
    private readonly ProfileCommands _commands;
    private readonly GameUiFactory _ui;

    public SettingsView(PlayerProgress progress, GameUiFactory ui, ProfileCommands commands)
    {
        _progress = progress;
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
        AddVolume(parent, Localize("Музыка", "Music"), _progress.MusicVolume, _commands.SetMusicVolume);
        AddVolume(parent, Localize("Эффекты", "Effects"), _progress.EffectsVolume, _commands.SetEffectsVolume);
        _ui.CreateButton(parent, _progress.English ? "Русский" : "English", () =>
        {
            _commands.ToggleLanguage();
            reopen();
        });
        _ui.CreateButton(parent, Localize("Задания", "Quests"), quests);
        _ui.CreateButton(parent, Localize("Окружение", "Scenery"), scenery);
        _ui.CreateButton(parent, Localize("Назад", "Back"), () =>
        {
            _commands.Save();
            back();
        });
    }

    private string Localize(string russian, string english)
    {
        return _progress.English ? english : russian;
    }

    private void AddNameField(Transform parent)
    {
        var rect = GameUiFactory.CreateRect("Player name", parent, Vector2.zero, Vector2.one);
        rect.gameObject.AddComponent<Image>().color = GameUiTheme.Button;
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 55f;
        var input = rect.gameObject.AddComponent<InputField>();
        input.textComponent = _ui.CreateLabel(rect, "", 24);
        input.characterLimit = 20;
        input.text = _progress.PlayerName;
        input.onEndEdit.AddListener(value =>
        {
            _commands.SetName(value);
        });
    }

    private void AddVolume(Transform parent, string title, float value, Action<float> apply)
    {
        _ui.CreateLabel(parent, title, 24);
        var rect = GameUiFactory.CreateRect(title, parent, Vector2.zero, Vector2.one);
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 45f;
        rect.gameObject.AddComponent<Image>().color = GameUiTheme.Button;
        var slider = rect.gameObject.AddComponent<Slider>();
        var fill = GameUiFactory.CreateRect("Fill", rect, Vector2.zero, Vector2.one);
        fill.gameObject.AddComponent<Image>().color = new Color(0.4f, 0.9f, 0.65f);
        slider.fillRect = fill;
        slider.value = value;
        slider.onValueChanged.AddListener(value => apply(value));
    }
}
