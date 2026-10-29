using System;
using GoosingGeese.Racing;
using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "RaceResultView")]
    public class RaceResultView : MonoBehaviour
    {
        private const int CanvasSortingOrder = 30;
        private static readonly Color ButtonColor = new Color(1f, 0.79f, 0.28f);
        private static readonly Color TextColor = new Color(0.05f, 0.09f, 0.12f);
        [SerializeField]
        private RaceController _race;
        [SerializeField]
        private RacerOverheatView _overheatView;
        [SerializeField]
        private GameAudio _audio;
        private GameUiFactory _ui;
        private GameObject _canvasRoot;
        private RectTransform _safeArea;
        private GameObject _panel;
        private GameObject _skipButton;
        private bool _waitingForExplosion;

        private void Awake()
        {
            InitializeUi();
        }

        private void OnEnable()
        {
            if (_ui == null)
                InitializeUi();

            _canvasRoot.SetActive(true);
            _race.StateChanged += OnStateChanged;

            if (_overheatView != null)
                _overheatView.ExplosionCompleted += RevealResult;

            OnStateChanged(_race.State);
        }

        private void Update()
        {
            GameUiFactory.UpdateSafeArea(_safeArea);
        }

        private void OnDisable()
        {
            _race.StateChanged -= OnStateChanged;

            if (_overheatView != null)
                _overheatView.ExplosionCompleted -= RevealResult;

            Hide();
            _canvasRoot.SetActive(false);
        }

        private void InitializeUi()
        {
            if (_canvasRoot != null)
            {
                _canvasRoot.SetActive(false);
                Destroy(_canvasRoot);
            }

            _panel = null;
            _ui = new GameUiFactory(GameUiTheme.LoadFont(), _audio.PlayClick);
            _canvasRoot = GameUiFactory.CreateCanvas("Race result", transform, CanvasSortingOrder);
            _safeArea = GameUiFactory.CreateRect("Safe area", _canvasRoot.transform, Vector2.zero, Vector2.one);
            var rect = GameUiFactory.CreateRect("Show result on tap", _canvasRoot.transform, Vector2.zero, Vector2.one);
            _skipButton = rect.gameObject;
            _skipButton.AddComponent<Image>().color = Color.clear;
            var button = _skipButton.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(RevealResult);
            _skipButton.SetActive(false);
        }

        private void OnStateChanged(RaceState state)
        {
            Hide();

            if (state != RaceState.Result)
                return;

            if (_race.Outcome == RaceOutcome.Overheated && _overheatView != null && _overheatView.isActiveAndEnabled)
            {
                _waitingForExplosion = true;
                _skipButton.SetActive(true);
            }
            else
                ShowResult();
        }

        private void RevealResult()
        {
            if (_waitingForExplosion == false || _race.State != RaceState.Result)
                return;

            _waitingForExplosion = false;
            _skipButton.SetActive(false);
            ShowResult();
        }

        private void ShowResult()
        {
            var rect = GameUiFactory.CreateRect("Panel result", _safeArea, new Vector2(0.06f, 0.025f), new Vector2(0.94f, 0.48f));
            _panel = rect.gameObject;
            var layout = _panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 12f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            AddLabel(_race.PlayerWon ? _race.Session.Text.Choose("Победа!", "You won!") : _race.Session.Text.Choose("Попробуем ещё?", "Try again?"), 32);

            if (_race.PlayerWon == false)
                AddLabel(GetLossReason(), 24);

            AddLabel("+" + _race.LastReward + _race.Session.Text.Choose(" очков", " points"), 30);

            if (_race.PlayerWon && _race.HasNextLevel)
                AddButton(_race.Session.Text.Choose("Следующий уровень", "Next level"), _race.NextLevel);

            AddButton(_race.Session.Text.Choose("Повторить трассу", "Retry track"), _race.Retry);
            AddButton(_race.Session.Text.Choose("В главное меню", "Main menu"), _race.ReturnToMenu);
        }

        private string GetLossReason()
        {
            switch (_race.Outcome)
            {
                case RaceOutcome.Overheated:
                    return _race.Session.Text.Choose("Перегрев", "Overheated");
                case RaceOutcome.OffRoad:
                    return _race.Session.Text.Choose("Вылет с трассы", "Off the road");
                case RaceOutcome.OpponentFinished:
                    return _race.Session.Text.Choose("Соперник финишировал первым", "Your rival finished first");
                default:
                    return _race.Session.Text.Choose("Трасса закончилась до финиша", "Route ended before the finish");
            }
        }

        private void AddLabel(string text, int size)
        {
            Text label = _ui.CreateLabel(_panel.transform, text, size);
            var outline = label.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2f, -2f);
        }

        private void AddButton(string text, Action action)
        {
            Button button = _ui.CreateButton(_panel.transform, text, action);
            button.GetComponent<Image>().color = ButtonColor;
            button.GetComponent<LayoutElement>().preferredHeight = 80f;
            Text label = button.GetComponentInChildren<Text>();
            label.color = TextColor;
            label.fontStyle = FontStyle.Bold;
            var outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = TextColor;
            outline.effectDistance = new Vector2(3f, -3f);
        }

        private void Hide()
        {
            _waitingForExplosion = false;
            _skipButton.SetActive(false);

            if (_panel == null)
                return;

            _panel.SetActive(false);
            Destroy(_panel);
            _panel = null;
        }
    }
}
