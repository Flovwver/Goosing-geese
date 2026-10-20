using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Racer))]
public class HeatFeedback : MonoBehaviour
{
    private const int DropCount = 6;
    private const float DropRadius = 1.7f;
    private const float DropHeight = 5f;
    private const float DropTravel = 2f;
    private const float DropSize = 0.18f;
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private List<Material> _replacementMaterials = new List<Material>();
    [SerializeField] private Material _particleMaterial;
    private Racer _racer;
    private MeshRenderer[] _renderers;
    private Transform[] _drops;
    private Material _sweatMaterial;
    private MaterialPropertyBlock _properties;
    private float _progress;

    private void Awake()
    {
        _racer = GetComponent<Racer>();
        _renderers = GetComponentsInChildren<MeshRenderer>(true);
        _properties = new MaterialPropertyBlock();
        _drops = new Transform[DropCount];
        _sweatMaterial = new Material(Shader.Find("Standard"));
        _sweatMaterial.color = Color.cyan;

        for (int i = 0; i < DropCount; i++)
        {
            var drop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            drop.name = "Sweat";
            Destroy(drop.GetComponent<Collider>());
            drop.transform.SetParent(transform, false);
            drop.transform.localScale = Vector3.one * DropSize;
            drop.GetComponent<Renderer>().sharedMaterial = _sweatMaterial;
            _drops[i] = drop.transform;
            drop.SetActive(false);
        }

        foreach (ParticleSystemRenderer renderer in GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            Material original = renderer.sharedMaterial;

            if (original == null)
                continue;

            var replacement = _particleMaterial != null
                ? new Material(_particleMaterial)
                : new Material(Shader.Find("Particles/Standard Unlit"));
            replacement.mainTexture = original.mainTexture;
            renderer.sharedMaterial = replacement;
            _replacementMaterials.Add(replacement);
        }
    }

    private void OnEnable()
    {
        if (_properties == null)
            _properties = new MaterialPropertyBlock();

        _racer.OverheatProgressChanged += OnProgress;
        _racer.RaceRestarted += OnRestarted;
    }

    private void Update()
    {
        foreach (MeshRenderer renderer in _renderers)
        {
            if (renderer == null)
                continue;

            _properties.SetColor(ColorProperty, Color.Lerp(renderer.sharedMaterial.color, Color.red, _progress));
            renderer.SetPropertyBlock(_properties);
        }

        for (int i = 0; i < _drops.Length; i++)
        {
            _drops[i].gameObject.SetActive(_progress > 0f);
            float angle = i * Mathf.PI * 2f / DropCount;
            float fall = Mathf.Repeat(Time.time + (float)i / DropCount, 1f);
            _drops[i].localPosition = new Vector3(Mathf.Cos(angle) * DropRadius, DropHeight - fall * DropTravel, Mathf.Sin(angle) * DropRadius);
        }
    }

    private void OnDisable()
    {
        _racer.OverheatProgressChanged -= OnProgress;
        _racer.RaceRestarted -= OnRestarted;
    }

    private void OnDestroy()
    {
        Destroy(_sweatMaterial);

        foreach (Material material in _replacementMaterials)
            Destroy(material);
    }

    private void OnProgress(float progress)
    {
        _progress = Mathf.Clamp01(progress);
    }

    private void OnRestarted()
    {
        _progress = 0f;
    }
}
