using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(600)]
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
    private RacerAppearance _appearance;
    private MeshRenderer[] _renderers;
    private Color[] _baseColors;
    private Transform[] _drops;
    private Material _sweatMaterial;
    private MaterialPropertyBlock _properties;
    private float _progress;

    private void Awake()
    {
        _racer = GetComponent<Racer>();
        _appearance = GetComponent<RacerAppearance>();
        RefreshRenderers();
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

        if (_appearance != null)
            _appearance.AppearanceChanged += RefreshRenderers;

        RefreshRenderers();
    }

    private void LateUpdate()
    {
        for (int index = 0; index < _renderers.Length; index++)
        {
            MeshRenderer renderer = _renderers[index];

            if (renderer == null)
                continue;

            renderer.GetPropertyBlock(_properties);
            _properties.SetColor(ColorProperty, Color.Lerp(_baseColors[index], Color.red, _progress));
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

        if (_appearance != null)
            _appearance.AppearanceChanged -= RefreshRenderers;
    }

    private void OnDestroy()
    {
        Destroy(_sweatMaterial);

        foreach (Material material in _replacementMaterials)
            Destroy(material);
    }

    private void RefreshRenderers()
    {
        var renderers = new List<MeshRenderer>();
        var colors = new List<Color>();

        foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>(true))
        {
            if (renderer.name == "Sweat" || renderer.enabled == false || renderer.gameObject.activeInHierarchy == false || renderer.sharedMaterial == null)
                continue;

            renderers.Add(renderer);
            colors.Add(renderer.sharedMaterial.color);
        }

        _renderers = renderers.ToArray();
        _baseColors = colors.ToArray();
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
