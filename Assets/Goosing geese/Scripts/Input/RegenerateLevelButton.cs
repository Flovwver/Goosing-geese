using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RegenerateLevelButton : MonoBehaviour
{
    [SerializeField] private Level _level;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(RegenerateLevel);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(RegenerateLevel);
    }

    private void RegenerateLevel()
    {
        _level.BuildRoad();
    }
}
