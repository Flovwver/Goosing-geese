using UnityEngine;

public class OpponentSpawner : MonoBehaviour
{
    private const float VehicleLength = 5f;
    [SerializeField] private Level _level;
    [SerializeField] private Racer _player;
    [SerializeField] private GameObject[] _vehiclePrefabs;

    private OpponentDriver _driver;
    private Racer _opponent;
    private RaceController _race;
    private MeshRenderer[] _bodyRenderers;
    private GameObject _vehicle;

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
        _opponent = Instantiate(_player, _player.transform.position, _player.transform.rotation);
        _opponent.name = "Opponent";
        _bodyRenderers = _opponent.GetComponentsInChildren<MeshRenderer>(true);
        _driver = _opponent.gameObject.AddComponent<OpponentDriver>();
        _opponent.Finished += OnOpponentFinished;

        foreach (Collider playerCollider in _player.GetComponentsInChildren<Collider>())
        {
            foreach (Collider opponentCollider in _opponent.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(playerCollider, opponentCollider);
        }

        foreach (Renderer renderer in _opponent.GetComponentsInChildren<Renderer>())
            renderer.material.color = new Color(1f, 0.7f, 0.2f);

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

        if (_vehicle != null)
        {
            _vehicle.SetActive(false);
            Destroy(_vehicle);
        }

        int index = (int)((uint)_level.Seed % (uint)_vehiclePrefabs.Length);
        GameObject prefab = _vehiclePrefabs[index];

        if (prefab == null)
            return;

        foreach (MeshRenderer renderer in _bodyRenderers)
            renderer.enabled = false;

        _vehicle = Instantiate(prefab, _opponent.transform);
        _vehicle.transform.localPosition = Vector3.zero;
        _vehicle.transform.localRotation = Quaternion.identity;
        Bounds bounds = new Bounds(_vehicle.transform.position, Vector3.zero);

        foreach (Renderer renderer in _vehicle.GetComponentsInChildren<Renderer>())
            bounds.Encapsulate(renderer.bounds);

        float length = Mathf.Max(bounds.size.x, bounds.size.z);

        if (length > Mathf.Epsilon)
            _vehicle.transform.localScale *= VehicleLength / length;

        foreach (Collider collider in _vehicle.GetComponentsInChildren<Collider>())
            collider.enabled = false;
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
