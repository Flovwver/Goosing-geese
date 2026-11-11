using System.Collections.Generic;
using UnityEngine;

namespace GoosingGeese.Roads
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "RoadPiece")]
    public class RoadPiece : MonoBehaviour
    {
        [SerializeField] private RoadPieceType _type;
        [SerializeField] private Transform _entryPoint;
        [SerializeField] private Transform _exitPoint;
        [Header("Local path curve")]
        [SerializeField] private Transform _curveStartControl;
        [SerializeField] private Transform _curveEndControl;

        public RoadPieceType Type => _type;
        public Transform EntryPoint => _entryPoint;
        public Transform ExitPoint => _exitPoint;

        public List<Vector3> GetCurvePoints(int samples)
        {
            samples = Mathf.Max(1, samples);
            var points = new List<Vector3>(samples + 1);

            for (int i = 0; i <= samples; i++)
                points.Add(EvaluateCurve((float)i / samples));

            return points;
        }

        private Vector3 EvaluateCurve(float t)
        {
            Vector3 start = _entryPoint.position;
            Vector3 end = _exitPoint.position;

            if (_curveStartControl == null || _curveEndControl == null)
                return Vector3.Lerp(start, end, t);

            float u = 1f - t;
            return u * u * u * start + 3f * u * u * t * _curveStartControl.position + 3f * u * t * t * _curveEndControl.position + t * t * t * end;
        }
    }
}
