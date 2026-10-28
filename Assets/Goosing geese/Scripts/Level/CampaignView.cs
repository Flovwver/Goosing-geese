using System;
using UnityEngine;
using UnityEngine.UI;

public class CampaignView : MonoBehaviour
{
    private const int PageSize = 6;
    private const int SortingOrder = 24;

    [SerializeField] private RaceController _race;
    [SerializeField] private GameAudio _audio;

    private GameUiFactory _ui;
    private GameObject _canvas;
    private RectTransform _safeArea;
    private GameObject _panel;
    private int _page;

    public event Action<bool> VisibilityChanged;

    public bool IsVisible => _canvas != null && _canvas.activeSelf;

    private int VisibleStageCount => Mathf.Max(_race.Campaign.Count, _race.Progress.GetUnlockedLevel(int.MaxValue));

    private void Awake()
    {
        InitializeUi();
    }

    private void OnEnable()
    {
        if (_ui == null)
            InitializeUi();

        _race.StateChanged += OnStateChanged;
    }

    private void Update()
    {
        if (IsVisible)
            GameUiFactory.UpdateSafeArea(_safeArea);
    }

    private void OnDisable()
    {
        _race.StateChanged -= OnStateChanged;
        Hide();
    }

    public void Show()
    {
        if (_race.State != RaceState.Waiting)
            return;

        _page = (_race.Progress.Level - 1) / PageSize;
        _canvas.SetActive(true);
        Refresh();
        VisibilityChanged?.Invoke(true);
    }

    public void Hide()
    {
        if (IsVisible == false)
            return;

        _canvas.SetActive(false);
        VisibilityChanged?.Invoke(false);
    }

    private string Localize(string russian, string english)
    {
        return _race.Progress.English ? english : russian;
    }

    private void InitializeUi()
    {
        if (_canvas != null)
        {
            _canvas.SetActive(false);
            Destroy(_canvas);
        }

        _panel = null;
        _ui = new GameUiFactory(GameUiTheme.LoadFont(), _audio.PlayClick);
        _canvas = GameUiFactory.CreateCanvas("Campaign", transform, SortingOrder);
        _canvas.AddComponent<Image>().color = GameUiTheme.Panel;
        _safeArea = GameUiFactory.CreateRect("Safe area", _canvas.transform, Vector2.zero, Vector2.one);
        _canvas.SetActive(false);
    }

    private void Refresh()
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
            Destroy(_panel);
        }

        var rect = GameUiFactory.CreateRect("Level selection", _safeArea, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f));
        _panel = rect.gameObject;
        var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 16, 16);
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        _ui.CreateHeading(rect, Localize("УРОВНИ", "LEVELS"), 32);

        for (int index = _page * PageSize; index < Mathf.Min(VisibleStageCount, (_page + 1) * PageSize); index++)
        {
            int number = index + 1;
            CampaignStage stage = _race.Campaign.GetStage(number);
            bool completed = number <= _race.Progress.HighestCompletedLevel;
            bool unlocked = number <= _race.Progress.GetUnlockedLevel(VisibleStageCount);
            string reward = stage.Reward + (completed ? "" : " +" + stage.FirstClearBonus);
            string text = number + " • " + stage.GetDifficultyName(_race.Progress.English)
                + " • " + reward + Localize(" очк.", " pts")
                + (completed ? Localize(" • Пройден", " • Cleared") : unlocked ? "" : Localize(" • Закрыт", " • Locked"));
            Button button = _ui.CreateButton(rect, text, () => Select(number));
            button.GetComponent<LayoutElement>().preferredHeight = 90f;
            button.interactable = unlocked;
        }

        var row = GameUiFactory.CreateRect("Pages", rect, Vector2.zero, Vector2.one);
        row.gameObject.AddComponent<LayoutElement>().preferredHeight = 68f;
        var horizontal = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        horizontal.spacing = 8f;
        horizontal.childControlWidth = true;
        horizontal.childControlHeight = true;
        horizontal.childForceExpandHeight = false;
        _ui.CreateButton(row, "<", () => ChangePage(-1)).interactable = _page > 0;
        _ui.CreateLabel(row, (_page + 1) + " / " + Mathf.CeilToInt((float)VisibleStageCount / PageSize), 24);
        _ui.CreateButton(row, ">", () => ChangePage(1)).interactable = (_page + 1) * PageSize < VisibleStageCount;
        _ui.CreateButton(rect, Localize("Назад", "Back"), Hide);
    }

    private void Select(int number)
    {
        if (_race.SelectLevel(number))
            Hide();
    }

    private void ChangePage(int direction)
    {
        _page += direction;
        Refresh();
    }

    private void OnStateChanged(RaceState state)
    {
        if (state != RaceState.Waiting)
            Hide();
    }
}
