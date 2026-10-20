using System;
using UnityEngine;
using UnityEngine.UI;

public class GameScreens : MonoBehaviour
{
    private const float ReferenceWidth = 720f;
    private const float ReferenceHeight = 1280f;
    private static readonly Color PanelColor = new Color(0.05f, 0.09f, 0.12f, 0.96f);
    private static readonly Color ButtonColor = new Color(0.12f, 0.32f, 0.36f);

    [SerializeField] private RaceController _race;
    [SerializeField] private InputSwitch _input;
    [SerializeField] private Racer _player;
    [SerializeField] private OpponentSpawner _opponents;

    private RectTransform _safeArea;
    private GameObject _panel;
    private Text _status;
    private Font _font;
    private Text _pauseLabel;

    private void Awake()
    {
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildCanvas();
    }

    private void OnEnable()
    {
        _race.StateChanged += OnStateChanged;
    }

    private void Start()
    {
        OnStateChanged(_race.State);
    }

    private void Update()
    {
        Rect safe = Screen.safeArea;
        _safeArea.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
        _safeArea.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
        _pauseLabel.text = L("Пауза", "Pause");
        string state = _race.State == RaceState.Waiting ? L("Нажми педаль, чтобы начать", "Hold the pedal to start") : _race.State == RaceState.Result ? L("Результат", "Result") : _race.State == RaceState.Paused ? L("Пауза", "Paused") : L("Гонка", "Race");
        string lead = "";

        if (_race.State == RaceState.Racing && _opponents.Opponent != null)
        {
            float playerDistance = _player.GetComponent<RoadNavigator>().DistanceAlongPath;
            float opponentDistance = _opponents.Opponent.GetComponent<RoadNavigator>().DistanceAlongPath;
            lead = playerDistance >= opponentDistance ? L("Ты впереди", "You are ahead") : L("Соперник впереди", "Rival is ahead");
        }

        _status.text = L("УРОВЕНЬ ", "LEVEL ") + _race.GetComponent<Level>().Number + "   •   " + _race.Progress.Points + L(" очков", " points") + "\n" + state + "\n" + lead;
    }

    private void OnDisable()
    {
        _race.StateChanged -= OnStateChanged;
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private string L(string russian, string english)
    {
        return _race.Progress.English ? english : russian;
    }

    private void BuildCanvas()
    {
        var root = new GameObject("Game screens", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.matchWidthOrHeight = 0.5f;
        _safeArea = Rect("Safe area", root.transform, Vector2.zero, Vector2.one);
        _status = Label(_safeArea, "", 26);
        var statusRect = _status.rectTransform;
        statusRect.anchorMin = new Vector2(0.04f, 0.84f);
        statusRect.anchorMax = new Vector2(0.78f, 0.98f);
        var settings = MakeButton(_safeArea, L("Пауза", "Pause"), OpenSettings);
        _pauseLabel = settings.GetComponentInChildren<Text>();
        var buttonRect = (RectTransform)settings.transform;
        buttonRect.anchorMin = new Vector2(0.79f, 0.91f);
        buttonRect.anchorMax = new Vector2(0.97f, 0.98f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;
    }

    private Text Label(Transform parent, string text, int size)
    {
        var rect = Rect("Label", parent, Vector2.zero, Vector2.one);
        var label = rect.gameObject.AddComponent<Text>();
        label.font = _font;
        label.fontSize = size;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleCenter;
        label.text = text;
        label.raycastTarget = false;
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = size * 2.4f;
        return label;
    }

    private Button MakeButton(Transform parent, string text, Action action)
    {
        var rect = Rect(text, parent, Vector2.zero, Vector2.one);
        rect.gameObject.AddComponent<Image>().color = ButtonColor;
        var button = rect.gameObject.AddComponent<Button>();
        button.onClick.AddListener(() =>
        {
            GetComponent<GameAudio>().PlayClick();
            action();
        });
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 68f;
        Label(rect, text, 25);
        return button;
    }

    private void ShowPanel(string title, string page)
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
            Destroy(_panel);
        }

        var rect = Rect("Panel " + page, _safeArea, new Vector2(0.06f, 0.22f), new Vector2(0.94f, 0.82f));
        _panel = rect.gameObject;
        _panel.AddComponent<Image>().color = PanelColor;
        var layout = _panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 12f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        Label(rect, title, 32);
        _input.enabled = page == "menu" || page == "result";
    }

    private void ClosePanel()
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
            Destroy(_panel);
        }

        _input.enabled = true;
    }

    private void OnStateChanged(RaceState state)
    {
        if (state == RaceState.Waiting)
            ShowMenu();
        else if (state == RaceState.Racing)
            ClosePanel();
        else if (state == RaceState.Paused)
            ShowPause();
        else
            ShowResult();
    }

    private void ShowMenu()
    {
        ShowPanel("GOOSING GEESE", "menu");
        Label(_panel.transform, L("Гони. Остывай. Побеждай.", "Race. Cool down. Win."), 24);
        MakeButton(_panel.transform, L("Таблица рекордов", "Leaderboard"), ShowLeaderboard);
        MakeButton(_panel.transform, L("Задания", "Quests"), ShowQuests);
        MakeButton(_panel.transform, L("Внешний вид и окружение", "Appearance and scenery"), ShowAppearance);
        MakeButton(_panel.transform, L("Настройки", "Settings"), OpenSettings);
        Label(_panel.transform, L("Зажми педаль внизу экрана", "Hold the pedal below"), 24);
    }

    private void ShowPause()
    {
        ShowPanel(L("Пауза", "Paused"), "pause");
        MakeButton(_panel.transform, L("Продолжить", "Resume"), _race.Resume);
        MakeButton(_panel.transform, L("Настройки", "Settings"), OpenSettings);
        MakeButton(_panel.transform, L("В главное меню", "Main menu"), _race.ReturnToMenu);
    }

    private void ShowResult()
    {
        ShowPanel(_race.PlayerWon ? L("Победа!", "You won!") : L("Попробуем ещё?", "Try again?"), "result");
        string reason = _race.Outcome == RaceOutcome.Overheated ? L("Перегрев", "Overheated")
            : _race.Outcome == RaceOutcome.OffRoad ? L("Вылет с трассы", "Off the road")
            : _race.Outcome == RaceOutcome.OpponentFinished ? L("Соперник финишировал первым", "Your rival finished first") : "";

        if (_race.PlayerWon == false)
            Label(_panel.transform, reason, 24);

        Label(_panel.transform, "+" + _race.LastReward + L(" очков", " points"), 30);

        if (_race.PlayerWon)
            MakeButton(_panel.transform, L("Следующий уровень", "Next level"), _race.NextLevel);

        MakeButton(_panel.transform, L("Повторить трассу", "Retry track"), _race.Retry);
        MakeButton(_panel.transform, L("В главное меню", "Main menu"), _race.ReturnToMenu);
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
        MakeButton(_panel.transform, _race.Progress.English ? "Русский" : "English", () =>
        {
            _race.Progress.English = !_race.Progress.English;
            _race.Progress.Save();
            OpenSettings();
        });
        MakeButton(_panel.transform, L("Назад", "Back"), Back);
    }

    private void AddNameField()
    {
        var rect = Rect("Player name", _panel.transform, Vector2.zero, Vector2.one);
        rect.gameObject.AddComponent<Image>().color = ButtonColor;
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 55f;
        var input = rect.gameObject.AddComponent<InputField>();
        input.textComponent = Label(rect, "", 24);
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
        Label(_panel.transform, title, 24);
        var rect = Rect(title, _panel.transform, Vector2.zero, Vector2.one);
        rect.gameObject.AddComponent<LayoutElement>().preferredHeight = 45f;
        rect.gameObject.AddComponent<Image>().color = ButtonColor;
        var slider = rect.gameObject.AddComponent<Slider>();
        var fill = Rect("Fill", rect, Vector2.zero, Vector2.one);
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

        Label(_panel.transform, rows, 24).GetComponent<LayoutElement>().preferredHeight = 170f;
        Label(_panel.transform, L("Рекорд этого устройства. Онлайн-рейтинг пока не подключён.", "Device record. Online rankings are not connected."), 23);
        MakeButton(_panel.transform, L("Назад", "Back"), Back);
    }

    private void ShowQuests()
    {
        ShowPanel(L("Задания", "Quests"), "quests");
        string[] names = { L("Сегодня: 3 гонки", "Today: 3 races"), L("Сегодня: 1 победа", "Today: 1 win"), L("Всего: 10 побед", "Total: 10 wins") };

        for (int i = 0; i < names.Length; i++)
        {
            int quest = i;
            string title = names[i] + "  " + _race.Progress.GetQuestProgress(i) + "/" + _race.Progress.GetQuestTarget(i);
            var button = MakeButton(_panel.transform, title, () =>
            {
                _race.Progress.ClaimQuest(quest);
                ShowQuests();
            });
            button.interactable = !_race.Progress.IsQuestClaimed(i) && _race.Progress.GetQuestProgress(i) >= _race.Progress.GetQuestTarget(i);
        }

        Label(_panel.transform, L("Нажми выполненное задание, чтобы получить очки", "Tap a completed quest to claim points"), 22);
        MakeButton(_panel.transform, L("Назад", "Back"), Back);
    }

    private void ShowAppearance()
    {
        ShowPanel(L("Внешний вид", "Appearance"), "appearance");
        MakeButton(_panel.transform, L("Сменить персонажа / транспорт", "Change character / vehicle"), () =>
        {
            _race.Progress.Appearance = (_race.Progress.Appearance + 1) % GetComponent<WorldPresentation>().AppearanceOptions;
            _race.Progress.Save();
            GetComponent<WorldPresentation>().ApplyAppearance();
        });
        MakeButton(_panel.transform, L("Сменить окружение", "Change scenery"), () =>
        {
            _race.Progress.Environment = (_race.Progress.Environment + 1) % 3;
            _race.Progress.Save();
            GetComponent<WorldPresentation>().Rebuild();
        });
        MakeButton(_panel.transform, L("Назад", "Back"), Back);
    }

    private void Back()
    {
        if (_race.State == RaceState.Paused)
            ShowPause();
        else
            ShowMenu();
    }
}
