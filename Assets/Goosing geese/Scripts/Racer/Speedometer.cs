using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Slider))]
public class Speedometer : MonoBehaviour
{
    [SerializeField] private Racer _racer;
    [SerializeField] private Slider _greenZoneSlider;
    [SerializeField] private Slider _yellowZoneSlider;
    [SerializeField] private Text _maximumSpeedLabel;

    private Slider _slider;
    private float _displayedMaximumSpeed;
    private bool _hasDisplayedMaximumSpeed;

    private void OnEnable()
    {
        _slider = GetComponent<Slider>();
        _hasDisplayedMaximumSpeed = false;
    }

    private void Update()
    {
        if (_racer == null || _slider == null)
            return;

        if (_greenZoneSlider != null)
            _greenZoneSlider.SetValueWithoutNotify(_racer.YellowZoneThreshold);

        if (_yellowZoneSlider != null)
            _yellowZoneSlider.SetValueWithoutNotify(_racer.RedZoneThreshold);

        float speed = Application.isPlaying ? Mathf.Clamp01(_racer.NormalizedSpeed) : 0f;
        _slider.SetValueWithoutNotify(speed);
        UpdateMaximumSpeed();
    }

    private void UpdateMaximumSpeed()
    {
        if (_maximumSpeedLabel == null)
            return;

        float maximumSpeed = _racer.MaxSpeed;

        if (_hasDisplayedMaximumSpeed && Mathf.Approximately(_displayedMaximumSpeed, maximumSpeed))
            return;

        _displayedMaximumSpeed = maximumSpeed;
        _hasDisplayedMaximumSpeed = true;
        _maximumSpeedLabel.text = "MAX " + maximumSpeed.ToString("0.#");
    }
}
