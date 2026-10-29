using System;
using GoosingGeese.Infrastructure;
using GoosingGeese.Roads;
using GoosingGeese.Settings;
using UnityEngine;

namespace GoosingGeese.Racing
{
    [RequireComponent(typeof(OverheatTracker))]
    [RequireComponent(typeof(Mover))]
    [RequireComponent(typeof(RoadNavigator))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "Racer")]
    public class Racer : MonoBehaviour
    {
        private readonly EventPublisher _events = new EventPublisher();
        [SerializeField]
        private RacerStats _stats;
        private GripModel _gripModel;
        private OverheatTracker _overheatTracker;
        private Mover _mover;
        private RoadNavigator _roadNavigator;
        private RacerConfiguration _configuration;

        public event Action<float> OverheatProgressChanged;
        public event Action Exploded;
        public event Action PathCompleted;
        public event Action RaceRestarted;
        public event Action Finished;
        public event Action ConfigurationChanged;

        public bool IsFinished { get; private set; }
        public bool CanRace => IsFinished == false && isActiveAndEnabled && _overheatTracker.IsExploded == false && _roadNavigator.IsPathCompleted == false;
        public RacerConfiguration Configuration => _configuration;
        public RacerStats BaseStats => _stats;
        public float MaxSpeed => _configuration?.MaxSpeed ?? _stats.MaxSpeed;
        public float YellowZoneThreshold => _configuration?.YellowZoneThreshold ?? _stats.YellowZoneThreshold;
        public float RedZoneThreshold => _configuration?.RedZoneThreshold ?? _stats.RedZoneThreshold;
        public float NormalizedSpeed => MaxSpeed > 0f ? GetCurrentSpeed() / MaxSpeed : 0f;

        private void Awake()
        {
            InitializeComponents();
        }

        private void OnEnable()
        {
            InitializeComponents();
            _overheatTracker.ProgressChanged += OnOverheatProgressChanged;
            _overheatTracker.Exploded += OnExploded;
            _roadNavigator.PathCompleted += OnPathCompleted;
        }

        private void OnDisable()
        {
            _mover.Stop();
            _overheatTracker.ProgressChanged -= OnOverheatProgressChanged;
            _overheatTracker.Exploded -= OnExploded;
            _roadNavigator.PathCompleted -= OnPathCompleted;
        }

        public float GetCurrentSpeed()
        {
            return _mover.GetCurrentSpeed();
        }

        public void ConfigureStats(RacerStats stats)
        {
            _configuration = new RacerConfiguration(stats);
            _mover.Configure(_configuration);
            _events.Publish(ConfigurationChanged, this);
        }

        public void ValidatePreparation(RoadPath path, RacerDefinition definition)
        {
            if (path == null || path.Curve == null || definition == null)
                throw new ArgumentException("A complete path and racer definition are required.");

            if (_mover == null || _roadNavigator == null || _overheatTracker == null)
                throw new InvalidOperationException("Racer components must be initialized before preparation.");
        }

        public void Prepare(RoadPath path, RacerDefinition definition)
        {
            ValidatePreparation(path, definition);
            IsFinished = false;
            _configuration = definition.Stats;
            _mover.Configure(_configuration);
            _roadNavigator.Configure(definition.Navigation);
            _mover.TeleportTo(definition.Position, definition.Rotation);
            _roadNavigator.SetPath(path);
            _overheatTracker.ResetOverheat();
            _events.Publish(RaceRestarted, this);
        }

        public RacerDefinition CaptureDefinition(Vector3 position, Quaternion rotation)
        {
            return new RacerDefinition(_configuration, _roadNavigator.CaptureConfiguration(), position, rotation);
        }

        public void StopMovement()
        {
            _mover.Stop();
        }

        public void ReleaseThrottle()
        {
            _mover?.ReleaseThrottle();
        }

        public void FinishRace()
        {
            if (IsFinished || _overheatTracker.IsExploded || (_roadNavigator.IsPathCompleted && _roadNavigator.IsFinishReached == false))
                return;

            IsFinished = true;
            _overheatTracker.ResetOverheat();
            _mover.SetDrag(_configuration.FinishDrag);
            _mover.MoveTowards(_roadNavigator.TargetPosition, false, 0f);
            _events.Publish(Finished, this);
        }

        public void Tick(bool isAccelerate, float deltaTime)
        {
            if (isActiveAndEnabled == false || _overheatTracker.IsExploded || _roadNavigator.IsPathCompleted)
                return;

            if (IsFinished == false)
            {
                _overheatTracker.Tick(GetCurrentSpeed(), _configuration, deltaTime);

                if (_overheatTracker.IsExploded)
                    return;
            }

            _roadNavigator.UpdateTarget(transform.position);

            if (IsFinished == false && _roadNavigator.IsFinishReached)
                FinishRace();

            if (_roadNavigator.IsPathCompleted)
                return;

            float slip = IsFinished ? 0f : _gripModel.CalculateSlipFactor(GetCurrentSpeed(), _configuration);
            _mover.MoveTowards(_roadNavigator.TargetPosition, isAccelerate && IsFinished == false, slip);
        }

        private void InitializeComponents()
        {
            _overheatTracker = GetComponent<OverheatTracker>();
            _mover = GetComponent<Mover>();
            _roadNavigator = GetComponent<RoadNavigator>();

            if (_gripModel == null)
                _gripModel = new GripModel();

            if (_configuration == null)
                _configuration = new RacerConfiguration(_stats);

            _mover.Configure(_configuration);
        }

        private void OnPathCompleted()
        {
            if (IsFinished == false && _roadNavigator.IsFinishReached)
                FinishRace();

            _mover.Stop();
            _events.Publish(PathCompleted, this);
        }

        private void OnOverheatProgressChanged(float progress)
        {
            _events.Publish(OverheatProgressChanged, progress, this);
        }

        private void OnExploded()
        {
            _mover.Stop();
            _events.Publish(Exploded, this);
        }
    }
}
