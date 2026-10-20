using UnityEngine;
using UnityEngine.UI;

public class PedalView : MonoBehaviour
{
    [SerializeField] private RaceController _race;
    [SerializeField] private InputSwitch _input;
    [SerializeField] private RacerAppearanceCatalog _appearances;
    [SerializeField] private Image _artwork;
    [SerializeField] private Image _shadow;
    [SerializeField, Min(0f)] private float _amplitude = 0.06f;
    [SerializeField, Min(0f)] private float _frequency = 3f;
    [SerializeField, Min(0f)] private float _pressSpeed = 12f;
    [SerializeField, Min(0f)] private float _releaseSpeed = 8f;
    [SerializeField, Range(0.1f, 1f)] private float _pressedHeight = 0.62f;
    [SerializeField, Range(0.1f, 1f)] private float _pressedWidth = 0.94f;
    [SerializeField, Range(0f, 60f)] private float _pressedTilt = 18f;
    [SerializeField, Min(0f)] private float _pressedTravel = 10f;
    [SerializeField, Range(0f, 1f)] private float _pressedBrightness = 0.85f;

    private Vector3 _baseScale;
    private Vector2 _releasedPosition;
    private float _pressAmount;
    private int _appearanceIndex = -1;

    private void Awake()
    {
        _baseScale = transform.localScale;
        _releasedPosition = _artwork.rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        _appearanceIndex = -1;
        RefreshSprite();
        ResetPress();
    }

    private void Start()
    {
        _baseScale = transform.localScale;
    }

    private void Update()
    {
        RefreshSprite();
        bool acceptsInput = _race.State == RaceState.Waiting || _race.State == RaceState.Racing;
        bool pressed = acceptsInput && _input.IsThrottlePressed;
        UpdatePress(pressed, Time.unscaledDeltaTime);
        float pulse = _race.State == RaceState.Waiting && pressed == false
            ? 1f + Mathf.Sin(Time.unscaledTime * _frequency) * _amplitude
            : 1f;
        transform.localScale = _baseScale * pulse;
    }

    private void OnDisable()
    {
        ResetPress();
        transform.localScale = _baseScale;
    }

    private void RefreshSprite()
    {
        int selected = _race.Progress.Appearance;

        if (selected == _appearanceIndex)
            return;

        _appearanceIndex = selected;
        RacerAppearanceCatalog.Entry appearance = _appearances.GetEntry(selected);
        Sprite sprite = appearance != null ? appearance.PedalSprite : null;
        _artwork.sprite = sprite;
        _shadow.sprite = sprite;
        _artwork.enabled = sprite != null;
        _shadow.enabled = sprite != null;
    }

    private void UpdatePress(bool pressed, float deltaTime)
    {
        float speed = pressed ? _pressSpeed : _releaseSpeed;
        _pressAmount = Mathf.MoveTowards(_pressAmount, pressed ? 1f : 0f, speed * deltaTime);
        ApplyPress();
    }

    private void ApplyPress()
    {
        var scale = new Vector3(Mathf.Lerp(1f, _pressedWidth, _pressAmount), Mathf.Lerp(1f, _pressedHeight, _pressAmount), 1f);
        _artwork.rectTransform.localScale = scale;
        _artwork.rectTransform.localRotation = Quaternion.Euler(_pressedTilt * _pressAmount, 0f, 0f);
        _artwork.rectTransform.anchoredPosition = _releasedPosition + Vector2.down * (_pressedTravel * _pressAmount);
        float brightness = Mathf.Lerp(1f, _pressedBrightness, _pressAmount);
        _artwork.color = new Color(brightness, brightness, brightness, 1f);
        _shadow.rectTransform.localScale = new Vector3(scale.x, Mathf.Lerp(1f, 0.72f, _pressAmount), 1f);
        _shadow.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.28f, 0.4f, _pressAmount));
    }

    private void ResetPress()
    {
        _pressAmount = 0f;
        ApplyPress();
    }
}
