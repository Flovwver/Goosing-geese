using GoosingGeese.Levels;
using GoosingGeese.Presentation;
using GoosingGeese.Roads;
using GoosingGeese.Settings;
using UnityEngine;

namespace GoosingGeese.Racing
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "OpponentSpawner")]
    public class OpponentSpawner : MonoBehaviour
    {
        private const float CornerGripMargin = 0.02f;
        [SerializeField]
        private Level _level;
        [SerializeField]
        private Racer _player;
        [SerializeField]
        private Racer _opponentPrefab;
        [SerializeField]
        private GameObject[] _vehiclePrefabs;
        [SerializeField, Min(0f)]
        private float _laneOffset = 4f;
        [SerializeField, Min(0f)]
        private float _brakingPreviewDistance = 45f;
        private OpponentDriver _driver;
        private Racer _opponent;
        private RaceController _race;
        private RacerAppearance _appearance;
        [SerializeField, Min(1f)]
        private float _turnRateMultiplier = 2f;

        public Racer Opponent => _opponent;
        public float LaneOffset => _laneOffset;

        private void Awake()
        {
            _race = GetComponent<RaceController>();
        }

        private void OnEnable()
        {
            _level.PathBuilt += OnPathBuilt;
            _player.RaceRestarted += OnRestarted;
            _race.RaceEnded += OnRaceEnded;
            _race.Disabled += OnControllerDisabled;

            if (_opponent != null)
            {
                _opponent.Finished += OnOpponentFinished;

                if (_level.TryGetBuiltPath(out RoadPath path, out RoadConfiguration settings))
                    OnPathBuilt(path, settings);
            }
        }

        private void Start()
        {
            _opponent = Instantiate(_opponentPrefab, _player.transform.position, _player.transform.rotation);
            _opponent.name = "Opponent";
            _appearance = _opponent.GetComponent<RacerAppearance>();
            _driver = _opponent.gameObject.AddComponent<OpponentDriver>();
            _opponent.Finished += OnOpponentFinished;

            foreach (Collider playerCollider in _player.GetComponentsInChildren<Collider>())
            {
                foreach (Collider opponentCollider in _opponent.GetComponentsInChildren<Collider>())
                    Physics.IgnoreCollision(playerCollider, opponentCollider);
            }

            _appearance.ShowOriginal(new Color(1f, 0.7f, 0.2f));

            if (_level.TryGetBuiltPath(out RoadPath path, out RoadConfiguration settings))
            {
                UpdateVehicle();
                _driver.SetPath(path);
                _level.PrepareOpponent(_opponent, path);
            }
        }

        private void OnDisable()
        {
            OnControllerDisabled();
            _level.PathBuilt -= OnPathBuilt;
            _player.RaceRestarted -= OnRestarted;
            _race.RaceEnded -= OnRaceEnded;
            _race.Disabled -= OnControllerDisabled;

            if (_opponent != null)
                _opponent.Finished -= OnOpponentFinished;
        }

        private void OnDestroy()
        {
            if (_opponent != null)
                Destroy(_opponent.gameObject);
        }

        public void Tick(float deltaTime)
        {
            if (isActiveAndEnabled && _driver != null)
                _driver.Tick(deltaTime);
        }

        public OpponentDefinition CaptureDefinition(CampaignStage stage, int seed, Vector3 position, Quaternion rotation)
        {
            RacerStats source = _opponentPrefab.BaseStats;
            RacerStats stats = Instantiate(source);

            try
            {
                stats.MaxSpeed = stage.OpponentMaxSpeed;
                stats.Acceleration = stage.OpponentAcceleration;
                stats.TurnSpeed = source.TurnSpeed * _turnRateMultiplier * stage.OpponentMaxSpeed / Mathf.Max(1f, source.MaxSpeed);
                stats.YellowZoneThreshold = Mathf.Min(stats.RedZoneThreshold,
                    Mathf.Max(source.YellowZoneThreshold,
                    Mathf.Max(stage.CruiseSpeed,
                    stage.CornerSpeed)
                    + CornerGripMargin));
                var racer = new RacerDefinition(new RacerConfiguration(stats),
                    _opponentPrefab.GetComponent<RoadNavigator>().CaptureConfiguration(),
                    position,
                    rotation);
                GameObject vehicle = null;

                if (_vehiclePrefabs != null && _vehiclePrefabs.Length > 0)
                    vehicle = _vehiclePrefabs[(int)((uint)seed % (uint)_vehiclePrefabs.Length)];

                return new OpponentDefinition(racer, stage.CruiseSpeed, stage.CornerSpeed, LaneOffset, _brakingPreviewDistance, vehicle);
            }
            finally
            {
                Destroy(stats);
            }
        }

        private void UpdateVehicle()
        {
            OpponentDefinition definition = _level.CurrentDefinition.Opponent;
            _opponent.gameObject.SetActive(definition != null);

            if (definition == null)
                return;

            _driver.Configure(_race, definition);

            if (definition.Vehicle != null)
                _appearance.ShowModel(definition.Vehicle);
        }

        private void OnPathBuilt(RoadPath path, RoadConfiguration settings)
        {
            if (_driver != null)
            {
                UpdateVehicle();
                _driver.SetPath(path);
                _level.PrepareOpponent(_opponent, path);
            }
        }

        private void OnRestarted()
        {
            if (_driver != null)
            {
                if (_level.TryGetBuiltPath(out RoadPath path, out RoadConfiguration settings))
                    OnPathBuilt(path, settings);
            }
        }

        private void OnOpponentFinished()
        {
            _race.EndRace(RaceOutcome.OpponentFinished);
        }

        private void OnRaceEnded(bool playerWon)
        {
            OnControllerDisabled();
        }

        private void OnControllerDisabled()
        {
            if (_opponent != null)
                _opponent.StopMovement();
        }
    }
}
