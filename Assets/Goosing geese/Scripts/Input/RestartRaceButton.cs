using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RestartRaceButton : MonoBehaviour
{
    [SerializeField] private Level _level;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(_level.RestartRace);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(_level.RestartRace);
    }
}
