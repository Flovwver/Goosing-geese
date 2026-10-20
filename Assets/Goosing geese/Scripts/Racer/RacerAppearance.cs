using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(500)]
public class RacerAppearance : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float _modelLength = 5f;

    private Renderer[] _originalRenderers;
    private GameObject _replacement;
    private OverheatTracker _overheat;

    private void Awake()
    {
        var originals = new List<Renderer>();

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            if (renderer is ParticleSystemRenderer || renderer.name == "Sweat")
                continue;

            originals.Add(renderer);
        }

        _originalRenderers = originals.ToArray();
        _overheat = GetComponent<OverheatTracker>();
    }

    private void LateUpdate()
    {
        if (_replacement == null)
            return;

        SetOriginalVisibility(false);
        _replacement.SetActive(_overheat == null || _overheat.IsExploded == false);
    }

    public void ShowOriginal(Color tint)
    {
        ClearReplacement();
        SetOriginalVisibility(true);

        foreach (Renderer renderer in _originalRenderers)
            renderer.material.color = tint;
    }

    public void ShowModel(GameObject prefab)
    {
        if (prefab == null)
            return;

        ClearReplacement();
        SetOriginalVisibility(false);
        _replacement = Instantiate(prefab, transform, false);
        _replacement.name = "Selected model";
        _replacement.transform.localPosition = Vector3.zero;
        _replacement.transform.localRotation = Quaternion.identity;
        Bounds bounds = new Bounds(_replacement.transform.position, Vector3.zero);

        foreach (Renderer renderer in _replacement.GetComponentsInChildren<Renderer>())
            bounds.Encapsulate(renderer.bounds);

        float length = Mathf.Max(bounds.size.x, bounds.size.z);

        if (length > Mathf.Epsilon)
            _replacement.transform.localScale *= _modelLength / length;

        foreach (Collider collider in _replacement.GetComponentsInChildren<Collider>())
            collider.enabled = false;
    }

    private void ClearReplacement()
    {
        if (_replacement == null)
            return;

        _replacement.SetActive(false);
        Destroy(_replacement);
        _replacement = null;
    }

    private void SetOriginalVisibility(bool visible)
    {
        foreach (Renderer renderer in _originalRenderers)
        {
            if (renderer != null)
                renderer.enabled = visible;
        }
    }
}
