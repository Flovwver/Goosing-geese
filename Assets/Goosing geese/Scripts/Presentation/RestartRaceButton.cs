using GoosingGeese.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    [RequireComponent(typeof(Button))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "RestartRaceButton")]
    public class RestartRaceButton : MonoBehaviour
    {
        [SerializeField]
        private Level _level;
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
            _level.RestartRace();
        }
    }
}
