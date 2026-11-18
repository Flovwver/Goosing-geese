using System;
using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    [RequireComponent(typeof(Button))]
    internal sealed class UiButtonAction : MonoBehaviour
    {
        private Button _button;
        private Action _playClick;
        private Action _action;

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(Invoke);
        }

        public void Bind(Action playClick, Action action)
        {
            _button = GetComponent<Button>();
            _button.onClick.RemoveListener(Invoke);
            _playClick = playClick;
            _action = action;
            _button.onClick.AddListener(Invoke);
        }

        private void Invoke()
        {
            _playClick?.Invoke();
            _action?.Invoke();
        }
    }
}
