using System;
using UnityEngine;
using UnityEngine.UI;

public class GameScreens : MonoBehaviour
{
    private const int CanvasSortingOrder = 20;
    private static readonly Color PanelColor = new Color(0.05f, 0.09f, 0.12f, 0.96f);
    private static readonly Color ButtonColor = new Color(0.12f, 0.32f, 0.36f);

    [SerializeField] private RaceController _race;
    [SerializeField] private InputSwitch _input;
    [SerializeField] private Racer _player;
    [SerializeField] private OpponentSpawner _opponents;
    [SerializeField] private GarageView _garage;

    private GameObject _pedal;
    private PedalPromptView _pedalPrompt;
    private RectTransform _safeArea;
    private GameObject _panel;
    private Text _status;
    private GameUiFactory _ui;
    private GameObject _canvasRoot;
    private Speedometer _speedometer;
    private GameObject _gear;
    private GameObject _trophy;
    private GameObject _garageButton;

    private void Awake()
    {
        InitializeUi();
    }

    private void OnEnable()
    {
        bool rebuilt = _ui == null;

        if (rebuilt)
            InitializeUi();

        _race.StateChanged += OnStateChanged;
        _garage.VisibilityChanged += OnGarageVisibilityChanged;

        if (rebuilt)
            OnStateChanged(_race.State);
    }

    private void Start()
    {
        OnStateChanged(_race.State);
    }

    private void Update()
    {
        GameUiFactory.UpdateSafeArea(_safeArea);
        bool showNavigation = _panel == null && _garage.IsVisible == false && _race.State != RaceState.Result;
        _pedal.SetActive(_race.State != RaceState.Result && _garage.IsVisible == false);
        _pedalPrompt.SetVisible(showNavigation && _race.State == RaceState.Waiting);
        _gear.SetActive(showNavigation);
        _trophy.SetActive(showNavigation && _race.State == RaceState.Waiting);
        _garageButton.SetActive(showNavigation && _race.State == RaceState.Waiting);
        UpdateInputAvailability();
        _status.gameObject.SetActive(_garage.IsVisible == false);
        _speedometer.gameObject.SetActive(_garage.IsVisible == false);
        string state = _race.State == RaceState.Waiting ? L("Нажми педаль, чтобы начать", "Hold the pedal to start") : _race.State == RaceState.Result ? L("Результат", "Result") : _race.State == RaceState.Paused ? L("Пауза", "Paused") : L("Гонка", "Race");
        string lead = "";

        if (_race.State == RaceState.Racing && _opponents.Opponent != null)
        {
            float playerDistance = _player.GetComponent<RoadNavigator>().DistanceAlongPath;
            float opponentDistance = _opponents.Opponent.GetComponent<RoadNavigator>().DistanceAlongPath;
            lead = playerDistance >= opponentDistance ? L("Ты впереди", "You are ahead") : L("Соперник впереди", "Rival is ahead");
        }

        _status.text = _race.State == RaceState.Waiting
            ? L("УРОВЕНЬ ", "LEVEL ") + _race.GetComponent<Level>().Number
            : state + "\n" + lead;
    }

    private void OnDisable()
    {
        _race.StateChanged -= OnStateChanged;
        _garage.VisibilityChanged -= OnGarageVisibilityChanged;

        if (_pedalPrompt != null)
            _pedalPrompt.SetVisible(false);
    }

    private string L(string russian, string english)
    {
        return _race.Progress.English ? english : russian;
    }

    private void InitializeUi()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        _ui = new GameUiFactory(font, GetComponent<GameAudio>().PlayClick);

        if (_canvasRoot != null)
        {
            _speedometer.transform.SetParent(transform, false);
            _pedal.transform.SetParent(transform, false);
            _canvasRoot.SetActive(false);
            Destroy(_canvasRoot);
            _panel = null;
        }

        BuildCanvas();
    }

    private void BuildCanvas()
    {
        var root = GameUiFactory.CreateCanvas("Game screens", transform, CanvasSortingOrder);
        _canvasRoot = root;
        _safeArea = GameUiFactory.CreateRect("Safe area", root.transform, Vector2.zero, Vector2.one);
        _status = _ui.CreateLabel(_safeArea, "", 23);
        GameUiFactory.Place(_status.rectTransform, new Vector2(0.22f, 0.81f), new Vector2(0.9f, 0.88f));
        _gear = MakeIconButton("Settings", MenuSymbol.Gear, OpenSettings, new Vector2(0.025f, 0.89f), new Vector2(0.17f, 0.975f), Color.white);
        _trophy = MakeIconButton("Leaderboard", MenuSymbol.Trophy, ShowLeaderboard, new Vector2(0.81f, 0.045f), new Vector2(0.97f, 0.145f), new Color(1f, 0.77f, 0.28f));
        _garageButton = MakeIconButton("Garage", MenuSymbol.Garage, OpenGarage, new Vector2(0.025f, 0.045f), new Vector2(0.185f, 0.145f), new Color(0.7f, 0.86f, 1f));
        Text garageLabel = _ui.CreateLabel(_garageButton.transform, L("ГАРАЖ", "GARAGE"), 18);
        GameUiFactory.Place(garageLabel.rectTransform, new Vector2(0f, -0.25f), new Vector2(1f, 0.02f));
        ConfigureRaceHud();
    }

    private GameObject MakeIconButton(string name, MenuSymbol symbol, Action action, Vector2 min, Vector2 max, Color tint)
    {
        Button button = _ui.CreateButton(_safeArea, name, action);
        Text label = button.GetComponentInChildren<Text>();
        label.gameObject.SetActive(false);
        Destroy(label.gameObject);
        button.GetComponent<Image>().color = new Color(0.04f, 0.09f, 0.11f, 0.82f);
        GameUiFactory.Place((RectTransform)button.transform, min, max);
        var iconRect = GameUiFactory.CreateRect("Symbol", button.transform, Vector2.zero, Vector2.one);
        var icon = iconRect.gameObject.AddComponent<MenuSymbolGraphic>();
        icon.color = tint;
        icon.SetSymbol(symbol);
        return button.gameObject;
    }

    private void ConfigureRaceHud()
    {
        if (_speedometer == null)
            _speedometer = FindFirstObjectByType<Speedometer>(FindObjectsInactive.Include);

        RectTransform bar = _speedometer.GetComponent<RectTransform>();
        bar.SetParent(_safeArea, false);
        bar.localScale = Vector3.one;
        GameUiFactory.Place(bar, new Vector2(0.22f, 0.914f), new Vector2(0.91f, 0.954f));
        PedalButton pedal = _pedal != null ? _pedal.GetComponent<PedalButton>() : FindFirstObjectByType<PedalButton>(FindObjectsInactive.Include);
        _pedal = pedal.gameObject;
        _pedalPrompt = pedal.GetComponent<PedalPromptView>();
        RectTransform pedalRect = pedal.GetComponent<RectTransform>();
        pedalRect.SetParent(_safeArea, false);
        pedalRect.localScale = Vector3.one;
        GameUiFactory.Place(pedalRect, new Vector2(0.36f, 0.055f), new Vector2(0.64f, 0.215f));
    }

    private void ShowPanel(string title, string page)
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
            Destroy(_panel);
        }

        _garage.Hide();
        var rect = GameUiFactory.CreateRect("Panel " + page, _safeArea, new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.86f));
        _panel = rect.gameObject;

        _panel.AddComponent<Image>().color = PanelColor;
        var layout = _panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 12f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        _ui.CreateLabel(rect, title, 32);
        UpdateInputAvailability();
    }

    private void ClosePanel()
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
            Destroy(_panel);
            _panel = null;
        }

        UpdateInputAvailability();
    }

    private void OnStateChanged(RaceState state)
    {
        if (state == RaceState.Waiting)
            ShowMenu();
        else if (state == RaceState.Paused)
            ShowPause();
        else
            ClosePanel();
    }

    private void UpdateInputAvailability()
    {
        bool acceptsInput = _race.State == RaceState.Waiting || _race.State == RaceState.Racing;
        _input.enabled = acceptsInput && _panel == null && _garage.IsVisible == false;
    }

    private void OnGarageVisibilityChanged(bool visible)
    {
        if (visible)
            ClosePanel();

        UpdateInputAvailability();
    }

    private void OpenGarage()
    {
        ClosePanel();
        _garage.Show();
    }

    private void ShowMenu()
    {
        ClosePanel();
    }

    private void ShowPause()
    {
        ShowPanel(L("Пауза", "Paused"), "pause");
        _ui.CreateButton(_panel.transform, L("Продолжить", "Resume"), _race.Resume);
        _ui.CreateButton(_panel.transform, L("Настройки", "Settings"), OpenSettings);
        _ui.CreateButton(_panel.transform, L("В главное меню", "Main menu"), _race.ReturnToMenu);
    }

    private void OpenSettings()
    {
        if (_race.State == RaceState.Racing)
            _race.Pause();

        if (_race.State == RaceState.Result)
            return;

        ShowPanel(L("Настройки", "Settings"), "settings");
        AddNameField();
        AddVolume(L("Музыка", "Music"), _race.Progress.MusicVolume, value => _race.Progress.MusicVolume = value);
        AddVolume(L("Эффекты", "Effects"), _race.Progress.EffectsVolume, value => _race.Progress.EffectsVolume = value);
        _ui.CreateButton(_panel.transform, _race.Progress.English ? "Русский" : "English", () =>
        {
            _race.Progress.English = !_race.Progress.English;
            _race.Progress.Save();
            OpenSettings();
        });
        _ui.CreateButton(_panel.transform, L("Задания", "Quests"), ShowQuests);
        _ui.CreateButton(_panel.transform, L("Окружение", "Scenery"), ShowScenery);
        _ui.CreateButton(_panel.transform, L("Назад", "Back"), Back);
    }

    private void AddNameField()
    {
        var rect = GameUiFactory.CreateRect("Player name", _panel.transform, Vector2.zero, Vector2.one);
        rect.gameObject.AddComponent<Image>().color = ButtonColor;
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 55f;
        var input = rect.gameObject.AddComponent<InputField>();
        input.textComponent = _ui.CreateLabel(rect, "", 24);
        input.characterLimit = 20;
        input.text = _race.Progress.PlayerName;
        input.onEndEdit.AddListener(value =>
        {
            _race.Progress.PlayerName = string.IsNullOrWhiteSpace(value) ? "Player" : value.Trim();
            _race.Progress.Save();
        });
    }

    private void AddVolume(string title, float value, Action<float> apply)
    {
        _ui.CreateLabel(_panel.transform, title, 24);
        var rect = GameUiFactory.CreateRect(title, _panel.transform, Vector2.zero, Vector2.one);
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 45f;
        rect.gameObject.AddComponent<Image>().color = ButtonColor;
        var slider = rect.gameObject.AddComponent<Slider>();
        var fill = GameUiFactory.CreateRect("Fill", rect, Vector2.zero, Vector2.one);
        fill.gameObject.AddComponent<Image>().color = new Color(0.4f, 0.9f, 0.65f);
        slider.fillRect = fill;
        slider.value = value;
        slider.onValueChanged.AddListener(v =>
        {
            apply(v);
            _race.Progress.Save();
        });
    }

    private void ShowLeaderboard()
    {
        ShowPanel(L("Локальный рекорд", "Local leaderboard"), "leaderboard");
        LocalLeaderboard.Record(_race.Progress.PlayerName, _race.Progress.Points);
        var entries = LocalLeaderboard.Read();
        string rows = "";

        for (int i = 0; i < entries.Count; i++)
        {
            bool currentPlayer = entries[i].Name == _race.Progress.PlayerName;

            if (i < 5 || currentPlayer)
                rows += LocalLeaderboard.GetRank(entries, i) + ". " + entries[i].Name + " — " + entries[i].Score + (currentPlayer ? " •" : "") + "\n";
        }

        if (entries.Find(entry => entry.Name == _race.Progress.PlayerName) == null)
            rows += (entries.Count + 1) + ". " + _race.Progress.PlayerName + " — " + _race.Progress.Points + " •";

        _ui.CreateLabel(_panel.transform, rows, 24).GetComponent<LayoutElement>().preferredHeight = 170f;
        _ui.CreateLabel(_panel.transform, L("Рекорд этого устройства. Онлайн-рейтинг пока не подключён.", "Device record. Online rankings are not connected."), 23);
        _ui.CreateButton(_panel.transform, L("Назад", "Back"), Back);
    }

    private void ShowQuests()
    {
        ShowPanel(L("Задания", "Quests"), "quests");
        string[] names = { L("Сегодня: 3 гонки", "Today: 3 races"), L("Сегодня: 1 победа", "Today: 1 win"), L("Всего: 10 побед", "Total: 10 wins") };

        for (int i = 0; i < names.Length; i++)
        {
            int quest = i;
            string title = names[i] + "  " + _race.Progress.GetQuestProgress(i) + "/" + _race.Progress.GetQuestTarget(i);
            var button = _ui.CreateButton(_panel.transform, title, () =>
            {
                _race.Progress.ClaimQuest(quest);
                ShowQuests();
            });
            button.interactable = !_race.Progress.IsQuestClaimed(i) && _race.Progress.GetQuestProgress(i) >= _race.Progress.GetQuestTarget(i);
        }

        _ui.CreateLabel(_panel.transform, L("Нажми выполненное задание, чтобы получить очки", "Tap a completed quest to claim points"), 22);
        _ui.CreateButton(_panel.transform, L("Назад", "Back"), Back);
    }

    private void ShowScenery()
    {
        ShowPanel(L("Окружение", "Scenery"), "appearance");
        _ui.CreateButton(_panel.transform, L("Сменить окружение", "Change scenery"), () =>
        {
            _race.Progress.Environment = (_race.Progress.Environment + 1) % 3;
            _race.Progress.Save();
            GetComponent<WorldPresentation>().Rebuild();
        });
        _ui.CreateButton(_panel.transform, L("Назад", "Back"), Back);
    }

    private void Back()
    {
        if (_race.State == RaceState.Paused)
            ShowPause();
        else
            ShowMenu();
    }
}
