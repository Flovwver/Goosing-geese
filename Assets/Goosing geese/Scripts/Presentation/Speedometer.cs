using GoosingGeese.Racing;
using UnityEngine;

namespace GoosingGeese.Presentation
{
    [ExecuteAlways]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "Speedometer")]
    public class Speedometer : MonoBehaviour
    {
        [SerializeField]
        private Racer _racer;
        [SerializeField]
        private SpeedometerDialGraphic _dial;
        [SerializeField]
        private SpeedometerScaleView _scale;

        private void Update()
        {
            if (_racer == null || _dial == null)
                return;

            float speed = Application.isPlaying ? Mathf.Clamp01(_racer.NormalizedSpeed) : 0f;
            _dial.SetValues(speed, _racer.YellowZoneThreshold, _racer.RedZoneThreshold);

            if (_scale != null)
                _scale.SetValues(_racer.MaxSpeed, _racer.YellowZoneThreshold, _racer.RedZoneThreshold);
        }
    }
}
