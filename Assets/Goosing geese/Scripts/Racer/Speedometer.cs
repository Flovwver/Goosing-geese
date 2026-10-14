using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Slider))]
public class Speedometer : MonoBehaviour
{
    [SerializeField] private Racer _racer;
    [SerializeField] private Slider _greenZoneSlider;
    [SerializeField] private Slider _yellowZoneSlider;

    private Slider _slider;

    private void OnEnable()
    {
        _slider = GetComponent<Slider>();
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
    }
}
