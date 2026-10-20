using UnityEngine;

public class WorldPresentation : MonoBehaviour
{
    private const int AppearanceCount = 3;
    private const float VehicleLength = 5f;
    [SerializeField] private Level _level;
    [SerializeField] private Racer _player;
    [SerializeField] private RaceController _race;
    [SerializeField] private GameObject[] _scenery;
    [SerializeField] private GameObject[] _vehicles;
    [SerializeField, Min(1f)] private float _decorationSpacing = 45f;
    [SerializeField, Min(1f)] private float _roadClearance = 20f;
    [SerializeField, Min(1f)] private float _groundSize = 10000f;

    private MeshRenderer[] _playerRenderers;
    private GameObject _model;
    private GameObject _root;
    private RoadPath _path;
    private Material _groundMaterial;

    public int AppearanceOptions => AppearanceCount + (_vehicles != null ? _vehicles.Length : 0);

    private void Awake()
    {
        _playerRenderers = _player.GetComponentsInChildren<MeshRenderer>(true);
    }

    private void OnEnable()
    {
        _level.PathBuilt += OnPathBuilt;
    }

    private void Start()
    {
        ApplyAppearance();
    }

    private void OnDisable()
    {
        _level.PathBuilt -= OnPathBuilt;
    }

    private void OnDestroy()
    {
        if (_groundMaterial != null)
            Destroy(_groundMaterial);
    }

    public void ApplyAppearance()
    {
        int appearance = Mathf.Clamp(_race.Progress.Appearance, 0, AppearanceOptions - 1);

        if (_model != null)
        {
            _model.SetActive(false);
            Destroy(_model);
        }

        foreach (MeshRenderer renderer in _playerRenderers)
            renderer.enabled = appearance < AppearanceCount;

        Color[] colors = { Color.white, new Color(0.65f, 0.85f, 1f), new Color(1f, 0.65f, 0.75f) };

        if (appearance < AppearanceCount)
        {
            foreach (MeshRenderer renderer in _playerRenderers)
                renderer.material.color = colors[appearance];

            return;
        }

        GameObject prefab = _vehicles[appearance - AppearanceCount];

        if (prefab == null)
            return;

        _model = Instantiate(prefab, _player.transform);
        _model.transform.localPosition = Vector3.zero;
        _model.transform.localRotation = Quaternion.identity;
        Bounds bounds = new Bounds(_model.transform.position, Vector3.zero);

        foreach (Renderer renderer in _model.GetComponentsInChildren<Renderer>())
            bounds.Encapsulate(renderer.bounds);

        float length = Mathf.Max(bounds.size.x, bounds.size.z);

        if (length > Mathf.Epsilon)
            _model.transform.localScale *= VehicleLength / length;

        foreach (Collider collider in _model.GetComponentsInChildren<Collider>())
            collider.enabled = false;
    }

    public void Rebuild()
    {
        if (_path == null)
            return;

        if (_root != null)
        {
            _root.SetActive(false);
            Destroy(_root);
        }

        _root = new GameObject("Level scenery");
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(_root.transform);
        ground.transform.position = Vector3.down * 1f;
        ground.transform.localScale = new Vector3(_groundSize, 1f, _groundSize);

        if (_groundMaterial == null)
            _groundMaterial = new Material(Shader.Find("Standard"));

        Color[] groundColors = { new Color(0.3f, 0.48f, 0.24f), new Color(0.45f, 0.48f, 0.45f), new Color(0.24f, 0.28f, 0.32f) };
        int theme = Mathf.Clamp(_race.Progress.Environment, 0, 2);
        _groundMaterial.color = groundColors[theme];
        ground.GetComponent<Renderer>().sharedMaterial = _groundMaterial;

        if (_scenery == null || theme >= _scenery.Length || _scenery[theme] == null)
            return;

        for (float distance = _decorationSpacing; distance < _path.Curve.Length; distance += _decorationSpacing)
        {
            Vector3 point = _path.Curve.GetPointAtDistance(distance);
            Vector3 tangent = (_path.Curve.GetPointAtDistance(distance + 1f) - point).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, tangent);

            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 candidate = point + right * (_roadClearance * side);
                bool clear = true;

                foreach (Vector3 roadPoint in _path.Curve.Points)
                {
                    if ((candidate - roadPoint).sqrMagnitude < _roadClearance * _roadClearance)
                    {
                        clear = false;
                        break;
                    }
                }

                if (clear == false)
                    continue;

                GameObject decoration = Instantiate(_scenery[theme], candidate, Quaternion.identity, _root.transform);

                foreach (Collider collider in decoration.GetComponentsInChildren<Collider>())
                    collider.enabled = false;
            }
        }
    }

    private void OnPathBuilt(RoadPath path, RoadSettings settings)
    {
        _path = path;
        Rebuild();
    }
}
