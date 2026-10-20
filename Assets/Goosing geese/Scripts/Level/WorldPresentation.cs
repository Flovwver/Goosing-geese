using System.Collections.Generic;
using UnityEngine;

public class WorldPresentation : MonoBehaviour
{
    private readonly Dictionary<Material, Material> _compatibleMaterials = new Dictionary<Material, Material>();

    [SerializeField] private Level _level;
    [SerializeField] private Racer _player;
    [SerializeField] private RaceController _race;
    [SerializeField] private GameObject[] _scenery;
    [SerializeField] private RacerAppearanceCatalog _appearances;
    [SerializeField, Min(1f)] private float _decorationSpacing = 45f;
    [SerializeField, Min(1f)] private float _roadClearance = 20f;
    [SerializeField, Min(1f)] private float _groundSize = 10000f;

    private List<Material> _compatibleMaterialSources = new List<Material>();
    private List<Material> _compatibleMaterialCopies = new List<Material>();
    private RacerAppearance _appearance;
    private GameObject _root;
    private RoadPath _path;
    private Material _groundMaterial;

    public int AppearanceOptions => _appearances.Count;

    private void Awake()
    {
        _appearance = _player.GetComponent<RacerAppearance>();
    }

    private void OnEnable()
    {
        for (int index = 0; index < _compatibleMaterialSources.Count; index++)
        {
            Material source = _compatibleMaterialSources[index];
            Material copy = _compatibleMaterialCopies[index];

            if (source != null && copy != null)
                _compatibleMaterials[source] = copy;
        }

        _level.PathBuilt += OnPathBuilt;
        _player.RaceRestarted += ApplyAppearance;
    }

    private void Start()
    {
        ApplyAppearance();
    }

    private void OnDisable()
    {
        _level.PathBuilt -= OnPathBuilt;
        _player.RaceRestarted -= ApplyAppearance;
    }

    private void OnDestroy()
    {
        foreach (Material material in _compatibleMaterialCopies)
            Destroy(material);

        if (_groundMaterial != null)
            Destroy(_groundMaterial);
    }

    public void ApplyAppearance()
    {
        RacerAppearanceCatalog.Entry appearance = _appearances.GetEntry(_race.Progress.Appearance);

        if (appearance == null)
            return;

        if (appearance.Model != null)
            _appearance.ShowModel(appearance.Model);
        else
            _appearance.ShowOriginal(appearance.Tint);
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

        float clearance = _roadClearance + GetDecorationRadius(_scenery[theme]);

        for (float distance = _decorationSpacing; distance < _path.Curve.Length; distance += _decorationSpacing)
        {
            Vector3 point = _path.Curve.GetPointAtDistance(distance);
            Vector3 tangent = (_path.Curve.GetPointAtDistance(distance + 1f) - point).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, tangent);

            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 candidate = point + right * (clearance * side);
                bool clear = true;

                foreach (Vector3 roadPoint in _path.Curve.Points)
                {
                    if ((candidate - roadPoint).sqrMagnitude < clearance * clearance)
                    {
                        clear = false;
                        break;
                    }
                }

                if (clear == false)
                    continue;

                GameObject decoration = Instantiate(_scenery[theme], candidate, Quaternion.identity, _root.transform);
                RepairMaterials(decoration);

                foreach (Collider collider in decoration.GetComponentsInChildren<Collider>())
                    collider.enabled = false;
            }
        }
    }

    private float GetDecorationRadius(GameObject prefab)
    {
        var bounds = new Bounds(prefab.transform.position, Vector3.zero);

        foreach (Renderer renderer in prefab.GetComponentsInChildren<Renderer>(true))
            bounds.Encapsulate(renderer.bounds);

        Vector3 centerOffset = bounds.center - prefab.transform.position;
        var radius = new Vector2(bounds.extents.x + Mathf.Abs(centerOffset.x), bounds.extents.z + Mathf.Abs(centerOffset.z));
        return radius.magnitude;
    }

    private void RepairMaterials(GameObject decoration)
    {
        foreach (Renderer renderer in decoration.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                Material source = materials[i];

                if (source == null || (source.shader != null && source.shader.isSupported && source.shader.name != "Hidden/InternalErrorShader"))
                    continue;

                if (_compatibleMaterials.TryGetValue(source, out Material compatible) == false)
                {
                    compatible = new Material(_groundMaterial);
                    compatible.name = source.name + " (compatible)";
                    compatible.color = Color.white;
                    compatible.mainTexture = source.mainTexture;
                    compatible.mainTextureScale = source.mainTextureScale;
                    compatible.mainTextureOffset = source.mainTextureOffset;
                    _compatibleMaterials.Add(source, compatible);
                    _compatibleMaterialSources.Add(source);
                    _compatibleMaterialCopies.Add(compatible);
                }

                materials[i] = compatible;
            }

            renderer.sharedMaterials = materials;
        }
    }

    private void OnPathBuilt(RoadPath path, RoadSettings settings)
    {
        _path = path;
        Rebuild();
    }
}
