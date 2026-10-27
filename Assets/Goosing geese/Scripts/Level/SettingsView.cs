using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class SettingsView
{
    private readonly PlayerProgress _progress;
    private readonly GameUiFactory _ui;

    public SettingsView(PlayerProgress progress, GameUiFactory ui)
    {
        _progress = progress;
        _ui = ui;
    }

    public void Render(Transform parent, Action reopen, Action quests, Action scenery, Action back)
    {
        AddNameField(parent);
        AddVolume(parent, L("Музыка", "Music"), _progress.MusicVolume, value => _progress.MusicVolume = value);
        AddVolume(parent, L("Эффекты", "Effects"), _progress.EffectsVolume, value => _progress.EffectsVolume = value);
        _ui.CreateButton(parent, _progress.English ? "Русский" : "English", () =>
        {
            _progress.English = !_progress.English;
            PlayerProgressStore.Save(_progress);
            reopen();
        });
        _ui.CreateButton(parent, L("Задания", "Quests"), quests);
        _ui.CreateButton(parent, L("Окружение", "Scenery"), scenery);
        _ui.CreateButton(parent, L("Назад", "Back"), back);
    }

    private string L(string russian, string english)
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
            _progress.PlayerName = string.IsNullOrWhiteSpace(value) ? "Player" : value.Trim();
            PlayerProgressStore.Save(_progress);
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
        slider.onValueChanged.AddListener(v =>
        {
            apply(v);
            PlayerProgressStore.Save(_progress);
        });
    }
}
