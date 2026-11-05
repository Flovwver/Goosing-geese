using GoosingGeese.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    [RequireComponent(typeof(Button))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "RegenerateLevelButton")]
    public class RebuildLevelButton : MonoBehaviour
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
            _button.onClick.AddListener(RebuildLevel);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(RebuildLevel);
        }

        private void RebuildLevel()
        {
            _level.BuildRoad();
        }
    }
}
