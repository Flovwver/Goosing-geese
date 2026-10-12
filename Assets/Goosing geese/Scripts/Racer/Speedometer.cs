using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class Speedometer : MonoBehaviour
{
    [SerializeField] private Racer _racer;

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Update()
    {
        _slider.value = Mathf.Clamp01(_racer.NormalizedSpeed);
    }
}
