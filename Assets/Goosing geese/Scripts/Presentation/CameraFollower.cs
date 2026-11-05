using GoosingGeese.Racing;
using GoosingGeese.Settings;
using UnityEngine;

namespace GoosingGeese.Presentation
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "CameraFollower")]
    public class CameraFollower : MonoBehaviour
    {
        private const float MinimumSmoothTime = 0.01f;
        [SerializeField]
        private Transform _target;
        [SerializeField]
        private CameraSettings _settings;
        [SerializeField, Min(0f)]
        private float _menuFocusHeight = 3f;
        private Vector3 _velocity;
        private Racer _racer;
        private bool _followEnabled = true;
        [SerializeField]
        private RaceController _race;
        [SerializeField]
        private OpponentSpawner _opponents;
        private Quaternion _followingRotation;

        private void Awake()
        {
            _racer = _target.GetComponent<Racer>();

            if (_race == null || _opponents == null)
                throw new System.InvalidOperationException("Assign the camera race and opponents in the Inspector.");

            _followingRotation = transform.rotation;
        }

        private void OnEnable()
        {
            if (_racer == null)
                return;

            _race.RaceEnded += OnRaceEnded;
            _race.Prepared += OnRaceRestarted;
            _followEnabled = _race.State != RaceState.Result;
        }

        private void LateUpdate()
        {
            if (_followEnabled == false)
                return;

            Vector3 focus = _target.position;
            bool waiting = _race != null && _race.State == RaceState.Waiting;

            if (waiting && _opponents != null && _opponents.Opponent != null)
                focus = (focus + _opponents.Opponent.transform.position) * 0.5f;

            Vector3 desiredPosition = focus + _settings.Offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _settings.SmoothTime);
            Quaternion rotation = waiting ? Quaternion.LookRotation(focus + Vector3.up * _menuFocusHeight - transform.position) : _followingRotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime / Mathf.Max(MinimumSmoothTime, _settings.SmoothTime));
        }

        private void OnDisable()
        {
            if (_racer == null)
                return;

            _race.RaceEnded -= OnRaceEnded;
            _race.Prepared -= OnRaceRestarted;
        }

        private void OnRaceEnded(bool playerWon)
        {
            _followEnabled = false;
            _velocity = Vector3.zero;
        }

        private void OnRaceRestarted()
        {
            _followEnabled = true;
            _velocity = Vector3.zero;
            transform.position = _target.position + _settings.Offset;
        }
    }
}
