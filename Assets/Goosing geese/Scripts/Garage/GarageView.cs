using System;
using UnityEngine;
using UnityEngine.UI;

public class GarageView : MonoBehaviour
{
    private const int CanvasSortingOrder = 25;
    private static readonly Color PanelColor = GameUiTheme.Panel;

    [SerializeField] private RaceController _race;
    [SerializeField] private RacerGarage _garage;
    [SerializeField] private GameAudio _audio;

    private GameUiFactory _ui;
    private GameObject _canvasRoot;
    private RectTransform _safeArea;
    private GameObject _header;
    private GameObject _footer;

    public event Action<bool> VisibilityChanged;

    public bool IsVisible => _canvasRoot != null && _canvasRoot.activeSelf;

    private void Awake()
    {
        InitializeUi();
    }

    private void OnEnable()
    {
        bool reopen = IsVisible;

        if (_ui == null)
            InitializeUi();

        _garage.SelectionChanged += Refresh;
        _garage.UpgradesChanged += Refresh;
        _race.StateChanged += OnStateChanged;

        if (reopen && _race.State == RaceState.Waiting)
            Show();
    }

    private void Update()
    {
        if (IsVisible)
            GameUiFactory.UpdateSafeArea(_safeArea);
    }

    private void OnDisable()
    {
        _garage.SelectionChanged -= Refresh;
        _garage.UpgradesChanged -= Refresh;
        _race.StateChanged -= OnStateChanged;
        Hide();
    }

    public void Show()
    {
        if (_race.State != RaceState.Waiting)
            return;

        _canvasRoot.SetActive(true);
        Refresh();
        VisibilityChanged?.Invoke(true);
    }

    public void Hide()
    {
        if (IsVisible == false)
            return;

        _canvasRoot.SetActive(false);
        VisibilityChanged?.Invoke(false);
    }

    private string L(string russian, string english)
    {
        return _race.Progress.English ? english : russian;
    }

    private void InitializeUi()
    {
        if (_canvasRoot != null)
        {
            _canvasRoot.SetActive(false);
            Destroy(_canvasRoot);
        }

        _header = null;
        _footer = null;
        _ui = new GameUiFactory(GameUiTheme.LoadFont(), _audio.PlayClick);
        _canvasRoot = GameUiFactory.CreateCanvas("Garage", transform, CanvasSortingOrder);
        _canvasRoot.AddComponent<Image>().color = Color.clear;
        _safeArea = GameUiFactory.CreateRect("Safe area", _canvasRoot.transform, Vector2.zero, Vector2.one);
        _canvasRoot.SetActive(false);
    }

    private void Refresh()
    {
        if (IsVisible == false)
            return;

        ClearPanels();
        RectTransform header = CreatePanel("Garage header", new Vector2(0f, 0.76f), Vector2.one);
        _header = header.gameObject;
        _ui.CreateHeading(header, L("ГАРАЖ", "GARAGE"), 38);
        Text balance = _ui.CreateLabel(header, L("Доступно: ", "Available: ") + _garage.AvailablePoints + L(" • Всего: ", " • Total: ") + _garage.TotalPoints, 24);
        balance.color = GameUiTheme.Accent;
        balance.fontStyle = FontStyle.Bold;
        var balanceOutline = balance.gameObject.AddComponent<Outline>();
        balanceOutline.effectColor = GameUiTheme.Ink;
        balanceOutline.effectDistance = new Vector2(1f, -1f);
        BuildModelSelector(header);
        RectTransform footer = CreatePanel("Garage upgrades", Vector2.zero, new Vector2(1f, 0.4f));
        _footer = footer.gameObject;

        foreach (RacerUpgradeKind kind in Enum.GetValues(typeof(RacerUpgradeKind)))
            AddUpgradeButton(footer, kind);

        _ui.CreateButton(footer, L("Назад к гонке", "Back to race"), Hide);
    }

    private RectTransform CreatePanel(string name, Vector2 min, Vector2 max)
    {
        var rect = GameUiFactory.CreateRect(name, _safeArea, min, max);
        rect.gameObject.AddComponent<Image>().color = PanelColor;
        var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 16, 16);
        layout.spacing = 4f;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        return rect;
    }

    private void BuildModelSelector(Transform parent)
    {
        var row = GameUiFactory.CreateRect("Model selector", parent, Vector2.zero, Vector2.one);
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = 60f;
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 4f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        Button previous = _ui.CreateButton(row, "<", () => SelectRelative(-1));
        previous.GetComponent<LayoutElement>().preferredWidth = 70f;
        Text name = _ui.CreateLabel(row, _garage.SelectedEntry.GetName(_race.Progress.English), 24);
        name.GetComponent<LayoutElement>().flexibleWidth = 1f;
        Button next = _ui.CreateButton(row, ">", () => SelectRelative(1));
        next.GetComponent<LayoutElement>().preferredWidth = 70f;
    }

    private void SelectRelative(int direction)
    {
        int next = (_garage.SelectedIndex + direction + _garage.ModelCount) % _garage.ModelCount;
        _garage.Select(next);
    }

    private void AddUpgradeButton(Transform parent, RacerUpgradeKind kind)
    {
        int level = _garage.GetUpgradeLevel(kind);
        bool maximum = level >= _garage.MaximumUpgradeLevel
            || Mathf.Approximately(_garage.GetUpgradeValue(kind), _garage.GetUpgradeValue(kind, 1));
        string price = maximum ? L("МАКС.", "MAX") : _garage.GetUpgradePrice(kind) + L(" очк.", " pts");
        string text = GetUpgradeName(kind) + "  " + level + "/" + _garage.MaximumUpgradeLevel + " • " + price
            + "\n" + GetPreview(kind, maximum);
        Button button = _ui.CreateButton(parent, text, () => _garage.TryUpgrade(kind));
        button.GetComponent<LayoutElement>().preferredHeight = 82f;
        button.GetComponent<Image>().color = (int)kind % 2 == 0 ? GameUiTheme.LightRow : GameUiTheme.AlternateRow;
        Text label = button.GetComponentInChildren<Text>();
        label.fontSize = 26;
        label.color = GameUiTheme.Ink;
        label.alignment = TextAnchor.MiddleLeft;
        GameUiFactory.Place(label.rectTransform, new Vector2(0.04f, 0.04f), new Vector2(0.84f, 0.96f));
        var action = GameUiFactory.CreateRect("Upgrade action", button.transform, new Vector2(0.86f, 0f), Vector2.one);
        var actionImage = action.gameObject.AddComponent<Image>();
        actionImage.color = GameUiTheme.Button;
        actionImage.raycastTarget = false;
        Text actionLabel = _ui.CreateLabel(action, maximum ? "MAX" : "+", maximum ? 16 : 42);
        actionLabel.color = GameUiTheme.Accent;
        button.interactable = _garage.CanUpgrade(kind);
    }

    private string GetUpgradeName(RacerUpgradeKind kind)
    {
        switch (kind)
        {
            case RacerUpgradeKind.Acceleration:
                return L("Разгон", "Acceleration");
            case RacerUpgradeKind.TopSpeed:
                return L("Максимальная скорость", "Top speed");
            case RacerUpgradeKind.Cooling:
                return L("Охлаждение", "Cooling");
            case RacerUpgradeKind.Handling:
                return L("Управляемость", "Handling");
            default:
                throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    private string GetPreview(RacerUpgradeKind kind, bool maximum)
    {
        float current = _garage.GetUpgradeValue(kind);
        float next = _garage.GetUpgradeValue(kind, 1);
        string prefix = kind == RacerUpgradeKind.Handling ? L("Зелёная зона: ", "Green zone: ")
            : kind == RacerUpgradeKind.Cooling ? L("Спад нагрева: ×", "Cooling rate: ×") : "";
        float scale = kind == RacerUpgradeKind.Handling ? 100f : 1f;
        string suffix = kind == RacerUpgradeKind.Handling ? "%" : "";
        string preview = prefix + (current * scale).ToString("0.##") + suffix;

        if (maximum == false)
            preview += " → " + (next * scale).ToString("0.##") + suffix;

        return preview;
    }

    private void OnStateChanged(RaceState state)
    {
        if (state != RaceState.Waiting)
            Hide();
    }

    private void ClearPanels()
    {
        if (_header != null)
        {
            _header.SetActive(false);
            Destroy(_header);
        }

        if (_footer != null)
        {
            _footer.SetActive(false);
            Destroy(_footer);
        }
    }
}
