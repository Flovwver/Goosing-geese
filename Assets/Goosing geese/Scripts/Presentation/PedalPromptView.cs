using GoosingGeese.Racing;
using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "PedalPromptView")]
    public class PedalPromptView : MonoBehaviour
    {
        [SerializeField]
        private RaceController _race;
        [SerializeField]
        private Graphic _halo;
        [SerializeField]
        private Text _caption;
        [SerializeField]
        private string[] _russianPrompts =
        {
            "ЖМИ!",
            "ГАЗУЙ!!!",
            "ПОГНАЛИ!",
            "ДАЙ ГАЗУ!",
            "ПЕДАЛЬ В ПОЛ!",
            "ПОРА НА СТАРТ!"
        };
        [SerializeField]
        private string[] _englishPrompts =
        {
            "GO!",
            "FLOOR IT!!!",
            "LET'S RACE!",
            "GAS IT UP!",
            "PEDAL TO THE METAL!",
            "READY, SET, GO!"
        };
        [SerializeField, Min(0f)]
        private float _pulseFrequency = 1.4f;
        [SerializeField, Range(0f, 0.5f)]
        private float _captionAmplitude = 0.1f;
        [SerializeField]
        private Vector2 _haloScaleRange = new Vector2(0.9f, 1.12f);
        [SerializeField]
        private Vector2 _haloAlphaRange = new Vector2(0.4f, 0.72f);
        [SerializeField]
        private Color _haloColor = new Color(1f, 0.76f, 0.18f, 1f);
        private System.Random _random;
        private bool _isVisible;
        private int _promptIndex = -1;

        private void Update()
        {
            if (_isVisible == false)
                return;

            if (_race.State != RaceState.Waiting)
            {
                SetVisible(false);
                return;
            }

            UpdatePresentation(Time.unscaledTime);
        }

        private void OnDisable()
        {
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            visible = visible && _race.State == RaceState.Waiting;

            if (visible && _isVisible == false)
                ChoosePrompt();

            _isVisible = visible;
            _halo.gameObject.SetActive(visible);
            _caption.gameObject.SetActive(visible && _promptIndex >= 0);

            if (visible)
                UpdatePresentation(Time.unscaledTime);
        }

        private void ChoosePrompt()
        {
            int count = Mathf.Min(_russianPrompts.Length, _englishPrompts.Length);

            if (count == 0)
            {
                _promptIndex = -1;
                return;
            }

            if (_random == null)
                _random = new System.Random();

            if (count == 1)
            {
                _promptIndex = 0;
                return;
            }

            int next = _random.Next(count - (_promptIndex >= 0 && _promptIndex < count ? 1 : 0));

            if (_promptIndex >= 0 && next >= _promptIndex)
                next++;

            _promptIndex = next;
        }

        private void UpdatePresentation(float time)
        {
            float wave = Mathf.Sin(time * _pulseFrequency * Mathf.PI * 2f);
            float amount = (wave + 1f) * 0.5f;
            _halo.rectTransform.localScale = Vector3.one * Mathf.Lerp(_haloScaleRange.x, _haloScaleRange.y, amount);
            Color tint = _haloColor;
            tint.a *= Mathf.Lerp(_haloAlphaRange.x, _haloAlphaRange.y, amount);
            _halo.color = tint;
            _caption.rectTransform.localScale = Vector3.one * (1f + wave * _captionAmplitude);
            _caption.rectTransform.localRotation = Quaternion.identity;
            _caption.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            _caption.rectTransform.anchoredPosition = Vector2.zero;
            _caption.alignment = TextAnchor.MiddleCenter;

            if (_promptIndex >= 0)
                _caption.text = _race.Progress.UseEnglish ? _englishPrompts[_promptIndex] : _russianPrompts[_promptIndex];
        }
    }
}
