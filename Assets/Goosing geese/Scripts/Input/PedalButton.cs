using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GoosingGeese.Input
{
    [RequireComponent(typeof(Button))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "PedalButton")]
    public class PedalButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("_inputSwitch")]
        private ThrottleInput _input;

        private void OnDisable()
        {
            if (_input != null)
                _input.SetPedalPressed(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _input.SetPedalPressed(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _input.SetPedalPressed(false);
        }
    }
}
