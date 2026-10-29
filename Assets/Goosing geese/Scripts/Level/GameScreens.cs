using System;
using UnityEngine;
using UnityEngine.UI;

public class GameScreens : MonoBehaviour
{
    private const int CanvasSortingOrder = 20;
    private static readonly Color PanelColor = GameUiTheme.Panel;

    [SerializeField] private RaceController _race;
    [SerializeField] private Racer _player;
    [SerializeField] private OpponentSpawner _opponents;
    [SerializeField] private GarageView _garage;
    [SerializeField] private CampaignView _campaignView;

    private GameObject _pedal;
    private PedalPromptView _pedalPrompt;
    private RectTransform _safeArea;
    private GameObject _panel;
    private Text _status;
    private SettingsView _settingsView;
    private LeaderboardView _leaderboardView;
    private QuestsView _questsView;
    private ScenerySelectionView _sceneryView;
    private GameUiFactory _ui;
    private GameObject _canvasRoot;
    [SerializeField] private Speedometer _speedometer;
    [SerializeField] private PedalButton _pedalButton;
    private RoadNavigator _playerNavigator;
    private Racer _displayOpponent;
    private RoadNavigator _opponentNavigator;
    private GameObject _gear;
    private GameObject _trophy;
    private GameObject _garageButton;
    private GameObject _menuBackdrop;
    private Text _wallet;
    private Text _garageLabel;
    private Text _trophyLabel;
    private GameObject _levelButton;
    private Text _levelLabel;
    private string _lastStatus;
    private RaceState _lastStatusState;

    private void Awake()
    {
        _playerNavigator = _player.GetComponent<RoadNavigator>();
        InitializeUi();
    }

    private void OnEnable()
    {
        _playerNavigator = _player.GetComponent<RoadNavigator>();
        bool rebuilt = _ui == null;

        if (rebuilt)
            InitializeUi();

        _race.StateChanged += OnStateChanged;
        _race.Progress.Changed += RefreshChrome;
        _garage.VisibilityChanged += OnGarageVisibilityChanged;
        _campaignView.VisibilityChanged += OnCampaignVisibilityChanged;

        OnStateChanged(_race.State);
        UpdateInputAvailability();
    }

    private void Start()
    {
        OnStateChanged(_race.State);
    }

    private void Update()
    {
        GameUiFactory.UpdateSafeArea(_safeArea);
        UpdateRaceStatus();
    }

    private void OnDisable()
    {
        _race.StateChanged -= OnStateChanged;
        _race.Progress.Changed -= RefreshChrome;
        _garage.VisibilityChanged -= OnGarageVisibilityChanged;
        _campaignView.VisibilityChanged -= OnCampaignVisibilityChanged;
        _settingsView.Save();
        _race.SetInputBlocked(false);

        if (_pedalPrompt != null)
            _pedalPrompt.SetVisible(false);
    }

    private void RefreshChrome()
    {
        if (_wallet == null || _pedal == null)
            return;

        bool menusHidden = _garage.IsVisible == false && _campaignView.IsVisible == false;
        bool showNavigation = _panel == null && menusHidden && _race.State != RaceState.Result;
        bool waiting = _race.State == RaceState.Waiting;
        bool showStartMenu = showNavigation && waiting;
        _garageLabel.text = _race.Session.Text.Choose("ГАРАЖ", "GARAGE");
        _trophyLabel.text = _race.Session.Text.Choose("РЕКОРД", "RECORD");
        _pedal.SetActive(showNavigation);
        _pedalPrompt.SetVisible(showStartMenu);
        _menuBackdrop.SetActive(showStartMenu);
        _levelButton.SetActive(showStartMenu);
        _levelLabel.text = _race.Session.Text.Choose("УРОВЕНЬ ", "LEVEL ") + _race.Progress.Level + " →";
        _wallet.text = _race.Progress.Wallet + _race.Session.Text.Choose(" ОЧК.", " PTS");
        _gear.SetActive(showNavigation);
        _trophy.SetActive(showNavigation && waiting);
        _garageButton.SetActive(showNavigation && waiting);
        _status.gameObject.SetActive(menusHidden && waiting == false);
        _speedometer.gameObject.SetActive(menusHidden);
        _lastStatus = null;
        UpdateRaceStatus();
    }

    private void UpdateRaceStatus()
    {
        string lead = "";

        if (_race.State == RaceState.Racing && _opponents.Opponent != null)
        {
            if (_displayOpponent != _opponents.Opponent)
            {
                _displayOpponent = _opponents.Opponent;
                _opponentNavigator = _displayOpponent.GetComponent<RoadNavigator>();
            }

            bool ahead = _playerNavigator.DistanceAlongPath >= _opponentNavigator.DistanceAlongPath;
            lead = ahead ? _race.Session.Text.Choose("Ты впереди", "You are ahead")
                : _race.Session.Text.Choose("Соперник впереди", "Rival is ahead");
        }

        if (_lastStatus == lead && _lastStatusState == _race.State)
            return;

        _lastStatus = lead;
        _lastStatusState = _race.State;
        _status.text = GetStateText() + "\n" + lead;
    }

    private string GetStateText()
    {
        switch (_race.State)
        {
            case RaceState.Waiting:
                return _race.Session.Text.Choose("УРОВЕНЬ ", "LEVEL ") + _race.Session.LevelNumber;
            case RaceState.Result:
                return _race.Session.Text.Choose("Результат", "Result");
            case RaceState.Paused:
                return _race.Session.Text.Choose("Пауза", "Paused");
            default:
                return _race.Session.Text.Choose("Гонка", "Race");
        }
    }

    private void InitializeUi()
    {
        Font font = GameUiTheme.LoadFont();
        _ui = new GameUiFactory(font, GetComponent<GameAudio>().PlayClick);
        _settingsView = new SettingsView(_race.Progress, _ui, _race.Session.Commands);
        _leaderboardView = new LeaderboardView(_race.Progress, _ui);
        _questsView = new QuestsView(_race.Progress, _ui, _race.Session.Commands);
        _sceneryView = new ScenerySelectionView(_race.Progress, _ui, _race.Session.Commands);

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
        _garageLabel = _ui.CreateLabel(_garageButton.transform, _race.Session.Text.Choose("ГАРАЖ", "GARAGE"), 18);
        GameUiFactory.Place(_garageLabel.rectTransform, new Vector2(0f, -0.25f), new Vector2(1f, 0.02f));
        _trophyLabel = _ui.CreateLabel(_trophy.transform, _race.Session.Text.Choose("РЕКОРД", "RECORD"), 18);
        GameUiFactory.Place(_trophyLabel.rectTransform, new Vector2(-0.1f, -0.25f), new Vector2(1.1f, 0.02f));
        Button levelButton = _ui.CreateButton(_safeArea, "", OpenCampaign);
        _levelButton = levelButton.gameObject;
        _levelLabel = levelButton.GetComponentInChildren<Text>();
        _levelLabel.fontSize = 22;
        GameUiFactory.Place((RectTransform)_levelButton.transform, new Vector2(0.31f, 0.815f), new Vector2(0.79f, 0.855f));
        BuildMenuChrome();
        ConfigureRaceHud();
    }

    private void BuildMenuChrome()
    {
        var rect = GameUiFactory.CreateRect("Start menu chrome", _safeArea, Vector2.zero, Vector2.one);
        _menuBackdrop = rect.gameObject;
        rect.SetAsFirstSibling();
        var header = GameUiFactory.CreateRect("Sky header", rect, new Vector2(0f, 0.875f), Vector2.one);
        var headerImage = header.gameObject.AddComponent<Image>();
        headerImage.color = GameUiTheme.Header;
        headerImage.raycastTarget = false;
        var footer = GameUiFactory.CreateRect("Navigation dock", rect, Vector2.zero, new Vector2(1f, 0.17f));
        var footerImage = footer.gameObject.AddComponent<Image>();
        footerImage.color = GameUiTheme.Header;
        footerImage.raycastTarget = false;
        Text title = _ui.CreateHeading(rect, "GOOSING\nGEESE", 68);
        title.fontStyle = FontStyle.Normal;
        title.lineSpacing = 0.85f;
        GameUiFactory.Place(title.rectTransform, new Vector2(0.12f, 0.65f), new Vector2(0.88f, 0.81f));
        var outline = title.gameObject.AddComponent<Outline>();
        outline.effectColor = GameUiTheme.Ink;
        outline.effectDistance = new Vector2(3f, -3f);
        _wallet = _ui.CreateLabel(rect, "", 24);
        _wallet.color = GameUiTheme.Accent;
        _wallet.alignment = TextAnchor.MiddleRight;
        _wallet.fontStyle = FontStyle.Bold;
        GameUiFactory.Place(_wallet.rectTransform, new Vector2(0.55f, 0.96f), new Vector2(0.96f, 0.995f));
    }

    private GameObject MakeIconButton(string name, MenuSymbol symbol, Action action, Vector2 min, Vector2 max, Color tint)
    {
        Button button = _ui.CreateButton(_safeArea, name, action);
        Text label = button.GetComponentInChildren<Text>();
        label.gameObject.SetActive(false);
        Destroy(label.gameObject);
        button.GetComponent<Image>().color = GameUiTheme.Button;
        GameUiFactory.Place((RectTransform)button.transform, min, max);
        var iconRect = GameUiFactory.CreateRect("Symbol", button.transform, Vector2.zero, Vector2.one);
        var icon = iconRect.gameObject.AddComponent<MenuSymbolGraphic>();
        icon.color = tint;
        icon.SetSymbol(symbol);
        return button.gameObject;
    }

    private void ConfigureRaceHud()
    {
        if (_speedometer == null || _pedalButton == null)
            throw new InvalidOperationException("Assign the speedometer and pedal in the Inspector.");

        RectTransform bar = _speedometer.GetComponent<RectTransform>();
        bar.SetParent(_safeArea, false);
        bar.localScale = Vector3.one;
        GameUiFactory.Place(bar, new Vector2(0.27f, 0.875f), new Vector2(0.83f, 0.995f));
        PedalButton pedal = _pedalButton;
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
        _campaignView.Hide();
        var rect = GameUiFactory.CreateRect("Panel " + page, _safeArea, new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.86f));
        _panel = rect.gameObject;

        _panel.AddComponent<Image>().color = PanelColor;
        var layout = _panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 12f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        _ui.CreateHeading(rect, title, 32);
        UpdateInputAvailability();
    }

    private void ClosePanel()
    {
        _settingsView.Save();

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
        _race.SetInputBlocked(_panel != null || _garage.IsVisible || _campaignView.IsVisible);
        RefreshChrome();
    }

    private void OnGarageVisibilityChanged(bool visible)
    {
        if (visible)
        {
            ClosePanel();
            _campaignView.Hide();
        }

        UpdateInputAvailability();
    }

    private void OnCampaignVisibilityChanged(bool visible)
    {
        if (visible)
        {
            ClosePanel();
            _garage.Hide();
        }

        UpdateInputAvailability();
    }

    private void OpenCampaign()
    {
        ClosePanel();
        _garage.Hide();
        _campaignView.Show();
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
        ShowPanel(_race.Session.Text.Choose("Пауза", "Paused"), "pause");
        _ui.CreateButton(_panel.transform, _race.Session.Text.Choose("Продолжить", "Resume"), _race.Resume);
        _ui.CreateButton(_panel.transform, _race.Session.Text.Choose("Настройки", "Settings"), OpenSettings);
        _ui.CreateButton(_panel.transform, _race.Session.Text.Choose("В главное меню", "Main menu"), _race.ReturnToMenu);
    }

    private void OpenSettings()
    {
        if (_race.State == RaceState.Racing)
            _race.Pause();

        if (_race.State == RaceState.Result)
            return;

        ShowPanel(_race.Session.Text.Choose("Настройки", "Settings"), "settings");
        _settingsView.Render(_panel.transform, OpenSettings, ShowQuests, ShowScenery, Back);
    }

    private void ShowLeaderboard()
    {
        ShowPanel(_race.Session.Text.Choose("Локальный рекорд", "Local leaderboard"), "leaderboard");
        _leaderboardView.Render(_panel.transform, _race.Session.ReadLeaderboard(), Back);
    }

    private void ShowQuests()
    {
        ShowPanel(_race.Session.Text.Choose("Задания", "Quests"), "quests");
        _questsView.Render(_panel.transform, Back, ShowQuests);
    }

    private void ShowScenery()
    {
        ShowPanel(_race.Session.Text.Choose("Окружение", "Scenery"), "appearance");
        _sceneryView.Render(_panel.transform, Back, GetComponent<WorldPresentation>().Rebuild);
    }

    private void Back()
    {
        if (_race.State == RaceState.Paused)
            ShowPause();
        else
            ShowMenu();
    }
}
