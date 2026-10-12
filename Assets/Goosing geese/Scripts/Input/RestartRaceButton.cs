using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RestartRaceButton : MonoBehaviour
{
    [SerializeField] private Level _level;
    [SerializeField] private InputSwitch _inputSwitch;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(RestartRace);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(RestartRace);
    }

    private void RestartRace()
    {
        if (_inputSwitch != null)
            _inputSwitch.ReleasePedal();

        _level.RestartRace();
    }
}
