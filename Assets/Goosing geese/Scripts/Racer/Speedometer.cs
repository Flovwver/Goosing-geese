using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class Speedometer : MonoBehaviour
{
    [SerializeField] private Racer _racer;
    [SerializeField] private SpeedometerDialGraphic _dial;
    [SerializeField] private Text _maximumSpeedLabel;

    private float _displayedMaximumSpeed;
    private bool _hasDisplayedMaximumSpeed;

    private void OnEnable()
    {
        _hasDisplayedMaximumSpeed = false;
    }

    private void Update()
    {
        if (_racer == null || _dial == null)
            return;

        float speed = Application.isPlaying ? Mathf.Clamp01(_racer.NormalizedSpeed) : 0f;
        _dial.SetValues(speed, _racer.YellowZoneThreshold, _racer.RedZoneThreshold);
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
