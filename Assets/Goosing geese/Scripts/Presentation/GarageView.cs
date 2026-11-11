using System;
using GoosingGeese.Garage;
using GoosingGeese.Racing;
using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "GarageView")]
    public class GarageView : MonoBehaviour
    {
        private const int CanvasSortingOrder = 25;
        private const float SwatchSize = 48f;

        [SerializeField] private RaceController _race;
        [SerializeField] private RacerGarage _garage;
        [SerializeField] private GameAudio _audio;
        private GameUiFactory _ui;
        private Canvas _canvasRoot;
        private RectTransform _safeArea;
        private RectTransform _header;
        private RectTransform _footer;

        public event Action<bool> VisibilityChanged;

        public bool IsVisible => _canvasRoot != null && _canvasRoot.gameObject.activeSelf;

        private void Awake()
        {
            InitializeUi();
        }

        private void OnEnable()
        {
            bool reopen = IsVisible;

            if (_ui == null)
                InitializeUi();

            _garage.PreviewChanged += Refresh;
            _garage.UpgradesChanged += Refresh;
            _race.StateChanged += OnStateChanged;

            if (reopen && _race.State == RaceState.Waiting)
                Show();
        }

        private void Update()
        {
            if (IsVisible)
                _ui.UpdateSafeArea(_safeArea);
        }

        private void OnDisable()
        {
            _garage.PreviewChanged -= Refresh;
            _garage.UpgradesChanged -= Refresh;
            _race.StateChanged -= OnStateChanged;
            Hide();
        }

        public void Show()
        {
            if (_race.State != RaceState.Waiting)
                return;

            _canvasRoot.gameObject.SetActive(true);
            Refresh();
            VisibilityChanged?.Invoke(true);
        }

        public void Hide()
        {
            if (IsVisible == false)
                return;

            _canvasRoot.gameObject.SetActive(false);
            _garage.EndPreview();
            VisibilityChanged?.Invoke(false);
        }

        private void InitializeUi()
        {
            if (_canvasRoot != null)
            {
                _canvasRoot.gameObject.SetActive(false);
                Destroy(_canvasRoot.gameObject);
            }

            _header = null;
            _footer = null;
            _ui = GetComponent<GameUiBootstrap>().Factory;
            _canvasRoot = _ui.CreateCanvas("Garage", transform, CanvasSortingOrder);
            _canvasRoot.gameObject.AddComponent<Image>().color = Color.clear;
            _safeArea = _ui.CreateRect("Safe area", _canvasRoot.transform, Vector2.zero, Vector2.one);
            _canvasRoot.gameObject.SetActive(false);
        }

        private void Refresh()
        {
            if (IsVisible == false)
                return;

            ClearPanels();
            RectTransform header = CreatePanel("Garage header", new Vector2(0f, 0.69f), Vector2.one);
            _header = header;
            _ui.CreateHeading(header, _race.Session.Text.Get("garage"), 38);
            Text balance = _ui.CreateLabel(header,
                _race.Session.Text.Get("available")
                + _garage.Balance
                + _race.Session.Text.Get("total")
                + _garage.TotalPoints,
                24);
            balance.color = _ui.Theme.Accent;
            balance.fontStyle = FontStyle.Bold;
            var balanceOutline = balance.gameObject.AddComponent<Outline>();
            balanceOutline.effectColor = _ui.Theme.Ink;
            balanceOutline.effectDistance = new Vector2(1f, -1f);
            BuildModelSelector(header);
            BuildColorSelector(header);
            BuildPurchaseButton(header);
            RectTransform footer = CreatePanel("Garage upgrades", Vector2.zero, new Vector2(1f, 0.4f));
            _footer = footer;

            foreach (RacerUpgradeKind kind in Enum.GetValues(typeof(RacerUpgradeKind)))
                AddUpgradeButton(footer, kind);

            _ui.CreateButton(footer, _race.Session.Text.Get("back.to.race"), Hide);
        }

        private RectTransform CreatePanel(string name, Vector2 min, Vector2 max)
        {
            var rect = _ui.CreateRect(name, _safeArea, min, max);
            rect.gameObject.AddComponent<Image>().color = _ui.Theme.Panel;
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
            var row = _ui.CreateRect("Model selector", parent, Vector2.zero, Vector2.one);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 60f;
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            Button previous = _ui.CreateButton(row, "<", () => SelectRelative(-1));
            previous.GetComponent<LayoutElement>().preferredWidth = 70f;
            Text name = _ui.CreateLabel(row,
                _garage.PreviewEntry.GetModelName(_race.Progress.UseEnglish)
                + "\n"
                + _race.Session.Text.Get("speed")
                + _garage.PreviewEntry.Stats.MaxSpeed.ToString("0")
                + _race.Session.Text.Get("green.cap")
                + (_garage.PreviewEntry.MaximumGreenZone * 100f).ToString("0")
                + "%",
                24);
            name.GetComponent<LayoutElement>().flexibleWidth = 1f;
            Button next = _ui.CreateButton(row, ">", () => SelectRelative(1));
            next.GetComponent<LayoutElement>().preferredWidth = 70f;
        }

        private void BuildColorSelector(Transform parent)
        {
            int[] colors = _garage.GetPreviewColorIndices();

            if (colors.Length < 2)
                return;

            var row = _ui.CreateRect("Color selector", parent, Vector2.zero, Vector2.one);
            row.gameObject.AddComponent<LayoutElement>().minHeight = SwatchSize;
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            foreach (int index in colors)
            {
                var rect = _ui.CreateRect("Color swatch " + index, row, Vector2.zero, Vector2.one);
                var size = rect.gameObject.AddComponent<LayoutElement>();
                size.minWidth = SwatchSize;
                size.minHeight = SwatchSize;
                var graphic = rect.gameObject.AddComponent<GarageColorSwatch>();
                graphic.Show(_ui.Theme, _garage.GetColor(index), index == _garage.PreviewIndex, index == _garage.SelectedIndex);
                var button = rect.gameObject.AddComponent<Button>();
                button.targetGraphic = graphic;
                button.transition = Selectable.Transition.None;
                graphic.Bind(button, _garage, _audio, index);
            }
        }

        private void BuildPurchaseButton(Transform parent)
        {
            bool owned = _garage.IsOwned(_garage.PreviewIndex);
            bool selected = _garage.PreviewIndex == _garage.SelectedIndex;
            string text = GetPurchaseLabel(owned, selected);
            Button button = _ui.CreateButton(parent, text, () =>
            {
                if (owned)
                    _garage.Select(_garage.PreviewIndex);
                else
                    _garage.TryPurchase();
            });
            button.GetComponent<LayoutElement>().preferredHeight = 48f;
            button.GetComponentInChildren<Text>().fontSize = 22;
            button.interactable = owned ? selected == false : _garage.CanPurchase();
        }

        private string GetPurchaseLabel(bool owned, bool selected)
        {
            if (owned)
                return selected ? _race.Session.Text.Get("selected") : _race.Session.Text.Get("select");

            if (_garage.HighestCompletedLevel < _garage.PreviewEntry.UnlockLevel)
                return _race.Session.Text.Get("after.level") + _garage.PreviewEntry.UnlockLevel + " • " + _garage.PreviewEntry.PurchasePrice;

            return _race.Session.Text.Get("buy") + _garage.PreviewEntry.PurchasePrice + _race.Session.Text.Get("pts");
        }

        private string GetUpgradePrefix(RacerUpgradeKind kind)
        {
            switch (kind)
            {
                case RacerUpgradeKind.ThermalResistance:
                    return _race.Session.Text.Get("red.zone");
                case RacerUpgradeKind.Handling:
                    return _race.Session.Text.Get("green.zone");
                case RacerUpgradeKind.Cooling:
                    return _race.Session.Text.Get("cooling.rate");
                default:
                    return "";
            }
        }

        private void SelectRelative(int direction)
        {
            _garage.BrowseRelativeModel(direction);
        }

        private void AddUpgradeButton(Transform parent, RacerUpgradeKind kind)
        {
            string text = GetUpgradeText(kind, out bool maximum);
            Button button = _ui.CreateButton(parent, text, () => _garage.TryUpgrade(kind));
            StyleUpgradeButton(button, kind, maximum);
            button.interactable = _garage.CanUpgrade(kind);
        }

        private string GetUpgradeText(RacerUpgradeKind kind, out bool maximum)
        {
            int level = _garage.GetUpgradeLevel(kind);
            bool thermalLock = kind == RacerUpgradeKind.Handling && _garage.IsHandlingThermallyLimited();
            bool unchanged = Mathf.Approximately(_garage.GetUpgradeValue(kind), _garage.GetUpgradeValue(kind, 1));
            maximum = level >= _garage.MaximumUpgradeLevel || (unchanged && thermalLock == false);
            string price = maximum ? _race.Session.Text.Get("max") : _garage.GetUpgradePrice(kind) + _race.Session.Text.Get("pts");
            string preview = unchanged && thermalLock ? _race.Session.Text.Get("improve.heat.resistance") : GetPreview(kind, maximum);

            if (_garage.HighestCompletedLevel < _garage.GetUpgradeRequiredStage(kind) && maximum == false)
                preview = _race.Session.Text.Get("after.level") + _garage.GetUpgradeRequiredStage(kind);

            return GetUpgradeName(kind) + "  " + level + "/" + _garage.MaximumUpgradeLevel + " • " + price + "\n" + preview;
        }

        private void StyleUpgradeButton(Button button, RacerUpgradeKind kind, bool maximum)
        {
            button.GetComponent<LayoutElement>().preferredHeight = 68f;
            button.GetComponent<Image>().color = (int)kind % 2 == 0 ? _ui.Theme.LightRow : _ui.Theme.AlternateRow;
            Text label = button.GetComponentInChildren<Text>();
            label.fontSize = 22;
            label.color = _ui.Theme.Ink;
            label.alignment = TextAnchor.MiddleLeft;
            _ui.Place(label.rectTransform, new Vector2(0.04f, 0.04f), new Vector2(0.84f, 0.96f));
            var action = _ui.CreateRect("Upgrade action", button.transform, new Vector2(0.86f, 0f), Vector2.one);
            var actionImage = action.gameObject.AddComponent<Image>();
            actionImage.color = _ui.Theme.Button;
            actionImage.raycastTarget = false;
            Text actionLabel = _ui.CreateLabel(action, maximum ? _race.Session.Text.Get("max") : "+", maximum ? 16 : 42);
            actionLabel.color = _ui.Theme.Accent;
        }

        private string GetUpgradeName(RacerUpgradeKind kind)
        {
            switch (kind)
            {
                case RacerUpgradeKind.Acceleration:
                    return _race.Session.Text.Get("acceleration");
                case RacerUpgradeKind.TopSpeed:
                    return _race.Session.Text.Get("top.speed");
                case RacerUpgradeKind.Cooling:
                    return _race.Session.Text.Get("cooling");
                case RacerUpgradeKind.Handling:
                    return _race.Session.Text.Get("handling");
                case RacerUpgradeKind.ThermalResistance:
                    return _race.Session.Text.Get("heat.resistance");
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private string GetPreview(RacerUpgradeKind kind, bool maximum)
        {
            float current = _garage.GetUpgradeValue(kind);
            float next = _garage.GetUpgradeValue(kind, 1);

            if (kind == RacerUpgradeKind.ThermalResistance)
            {
                current = 1f - current;
                next = 1f - next;
            }

            string prefix = GetUpgradePrefix(kind);
            float scale = kind == RacerUpgradeKind.Handling || kind == RacerUpgradeKind.ThermalResistance ? 100f : 1f;
            string suffix = kind == RacerUpgradeKind.Handling || kind == RacerUpgradeKind.ThermalResistance ? "%" : "";
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
                _header.gameObject.SetActive(false);
                Destroy(_header.gameObject);
            }

            if (_footer != null)
            {
                _footer.gameObject.SetActive(false);
                Destroy(_footer.gameObject);
            }
        }
    }
}
