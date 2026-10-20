using UnityEngine;

public class OpponentSpawner : MonoBehaviour
{
    [SerializeField] private Level _level;
    [SerializeField] private Racer _player;
    [SerializeField] private Racer _opponentPrefab;
    [SerializeField] private GameObject[] _vehiclePrefabs;

    private OpponentDriver _driver;
    private Racer _opponent;
    private RaceController _race;
    private RacerAppearance _appearance;

    public Racer Opponent => _opponent;

    private void Awake()
    {
        _race = GetComponent<RaceController>();
    }

    private void OnEnable()
    {
        _level.PathBuilt += OnPathBuilt;
        _player.RaceRestarted += OnRestarted;
        _race.RaceEnded += OnRaceEnded;
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

        if (_level.TryGetBuiltPath(out RoadPath path, out RoadSettings settings))
        {
            _driver.SetPath(path);
            UpdateVehicle();
        }
    }

    private void OnDisable()
    {
        _level.PathBuilt -= OnPathBuilt;
        _player.RaceRestarted -= OnRestarted;
        _race.RaceEnded -= OnRaceEnded;

        if (_opponent != null)
            _opponent.Finished -= OnOpponentFinished;
    }

    private void UpdateVehicle()
    {
        if (_vehiclePrefabs == null || _vehiclePrefabs.Length == 0)
            return;

        int index = (int)((uint)_level.Seed % (uint)_vehiclePrefabs.Length);
        _appearance.ShowModel(_vehiclePrefabs[index]);
    }

    private void OnPathBuilt(RoadPath path, RoadSettings settings)
    {
        if (_driver != null)
        {
            _driver.SetPath(path);
            UpdateVehicle();
        }
    }

    private void OnRestarted()
    {
        if (_driver != null)
        {
            _driver.ResetOpponent();
            UpdateVehicle();
        }
    }

    private void OnOpponentFinished()
    {
        _race.EndRace(false);
    }

    private void OnRaceEnded(bool playerWon)
    {
        _opponent.StopMovement();
    }
}
