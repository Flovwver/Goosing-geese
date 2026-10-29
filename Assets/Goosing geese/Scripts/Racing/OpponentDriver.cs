using GoosingGeese.Roads;
using UnityEngine;

namespace GoosingGeese.Racing
{
    [RequireComponent(typeof(Racer))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "OpponentDriver")]
    public class OpponentDriver : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)]
        private float _cruiseSpeed = 0.72f;
        [SerializeField, Range(0f, 1f)]
        private float _cornerSpeed = 0.45f;
        [SerializeField, Min(0f)]
        private float _laneOffset = 4f;
        [SerializeField, Min(0f)]
        private float _brakingPreviewDistance = 45f;
        private RaceController _race;
        private Racer _racer;
        private RoadNavigator _navigator;
        private RoadPath _path;

        public float LaneOffset => _laneOffset;

        private void Awake()
        {
            _racer = GetComponent<Racer>();
            _navigator = GetComponent<RoadNavigator>();
            _navigator.SetLaneOffset(_laneOffset);
        }

        private void OnDisable()
        {
            _racer.StopMovement();
        }

        public void Tick(float deltaTime)
        {
            if (isActiveAndEnabled == false
                || _race == null
                || _race.isActiveAndEnabled == false
                || _race.State != RaceState.Racing
                || _racer.IsFinished
                || _path == null)
                return;

            int segment = Mathf.Min(_navigator.CurrentSegment, _path.Pieces.Count - 1);
            bool corner = _path.Pieces[segment].Type != RoadPieceType.Straight;

            if (segment + 1 < _path.Pieces.Count && _path.Curve.GetSegmentEndDistance(segment) - _navigator.DistanceAlongPath < _brakingPreviewDistance)
                corner |= _path.Pieces[segment + 1].Type != RoadPieceType.Straight;

            float targetSpeed = corner ? _cornerSpeed : _cruiseSpeed;
            _racer.Tick(_racer.NormalizedSpeed < targetSpeed, deltaTime);
        }

        public void Configure(RaceController race, OpponentDefinition definition)
        {
            _race = race;
            _cruiseSpeed = definition.CruiseSpeed;
            _cornerSpeed = definition.CornerSpeed;
            _laneOffset = definition.LaneOffset;
            _brakingPreviewDistance = definition.BrakingPreviewDistance;
            _navigator.SetLaneOffset(_laneOffset);
        }

        public void SetPath(RoadPath path)
        {
            _path = path;
        }
    }
}
