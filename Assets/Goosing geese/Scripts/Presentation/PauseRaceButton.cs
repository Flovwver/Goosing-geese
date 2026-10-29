using GoosingGeese.Racing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    [RequireComponent(typeof(Button))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "PauseRaceButton")]
    public class PauseRaceButton : MonoBehaviour
    {
        [SerializeField]
        private RaceController _race;
        [SerializeField]
        private TMP_Text _label;
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(TogglePause);
            _race.StateChanged += OnStateChanged;
            OnStateChanged(_race.State);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(TogglePause);
            _race.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(RaceState state)
        {
            _label.text = state == RaceState.Paused ? "Продолжить" : "Пауза";
        }

        private void TogglePause()
        {
            if (_race.State == RaceState.Paused)
                _race.Resume();
            else
                _race.Pause();
        }
    }
}
