using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HeatUI : MonoBehaviour
{
    [SerializeField] private Heat _heat;

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void Update()
    {
        _slider.value = _heat.NormalizedSpeed;
    }
}   