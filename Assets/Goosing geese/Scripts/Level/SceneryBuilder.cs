using System;
using UnityEngine;

[Serializable]
public sealed class SceneryBuilder : IDisposable
{
    [SerializeField] private CompatibleMaterials _materials = new CompatibleMaterials();
    [SerializeField] private GameObject _root;
    [SerializeField] private Material _groundMaterial;

    public void Dispose()
    {
        if (_root != null)
            UnityEngine.Object.Destroy(_root);

        if (_groundMaterial != null)
            UnityEngine.Object.Destroy(_groundMaterial);

        _materials.Dispose();
        _root = null;
        _groundMaterial = null;
    }

    public void Build(RoadPath path, int theme, GameObject[] scenery, float spacing, float roadClearance, float groundSize)
{
        if (_root != null)
        {
            _root.SetActive(false);
            UnityEngine.Object.Destroy(_root);
        }

        _root = new GameObject("Level scenery");
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.SetParent(_root.transform);
        ground.transform.position = Vector3.down * 1f;
        ground.transform.localScale = new Vector3(groundSize, 1f, groundSize);

        if (_groundMaterial == null)
            _groundMaterial = new Material(Shader.Find("Standard"));

        Color[] groundColors = { new Color(0.3f, 0.48f, 0.24f), new Color(0.45f, 0.48f, 0.45f), new Color(0.24f, 0.28f, 0.32f) };
        theme = Mathf.Clamp(theme, 0, groundColors.Length - 1);
        _groundMaterial.color = groundColors[theme];
        ground.GetComponent<Renderer>().sharedMaterial = _groundMaterial;

        if (scenery == null || theme >= scenery.Length || scenery[theme] == null)
            return;

        float clearance = roadClearance + GetDecorationRadius(scenery[theme]);

        for (float distance = spacing; distance < path.Curve.Length; distance += spacing)
        {
            Vector3 point = path.Curve.GetPointAtDistance(distance);
            Vector3 tangent = (path.Curve.GetPointAtDistance(distance + 1f) - point).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, tangent);

            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 candidate = point + right * (clearance * side);
                bool clear = true;

                foreach (Vector3 roadPoint in path.Curve.Points)
                {
                    if ((candidate - roadPoint).sqrMagnitude < clearance * clearance)
                    {
                        clear = false;
                        break;
                    }
                }

                if (clear == false)
                    continue;

                GameObject decoration = UnityEngine.Object.Instantiate(scenery[theme], candidate, Quaternion.identity, _root.transform);
                _materials.Apply(decoration, _groundMaterial);

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
}
