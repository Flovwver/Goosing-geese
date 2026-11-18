using System.Collections.Generic;
using GoosingGeese.Racing;
using UnityEngine;

namespace GoosingGeese.Presentation
{
    [DefaultExecutionOrder(600)]
    [RequireComponent(typeof(Racer))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, "", "Assembly-CSharp", "HeatFeedback")]
    public class HeatFeedback : MonoBehaviour
    {
        private const int DropCount = 6;
        private const float DropRadius = 1.7f;
        private const float DropHeight = 5f;
        private const float DropTravel = 2f;
        private const float DropSize = 0.18f;

        private static readonly int ColorProperty = Shader.PropertyToID("_Color");
        private readonly List<(MeshRenderer Renderer, int Slot, Color BaseColor)> _surfaceColors = new();
        private List<Material> _replacementMaterials = new List<Material>();
        [SerializeField] private Material _particleMaterial;
        private Racer _racer;
        private RacerAppearance _appearance;
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
                Destroy(drop.GetComponent<SphereCollider>());
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

                var replacement = _particleMaterial != null ? new Material(_particleMaterial) : new Material(Shader.Find("Particles/Standard Unlit"));
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
            foreach (var surface in _surfaceColors)
            {
                MeshRenderer renderer = surface.Renderer;

                if (renderer == null)
                    continue;

                renderer.GetPropertyBlock(_properties, surface.Slot);
                _properties.SetColor(ColorProperty, Color.Lerp(surface.BaseColor, Color.red, _progress));
                renderer.SetPropertyBlock(_properties, surface.Slot);
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
            _surfaceColors.Clear();

            foreach (MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>(true))
            {
                if (renderer.name == "Sweat" || renderer.enabled == false || renderer.gameObject.activeInHierarchy == false)
                    continue;

                Material[] materials = renderer.sharedMaterials;

                for (int slot = 0; slot < materials.Length; slot++)
                {
                    Material material = materials[slot];

                    if (material == null || material.HasProperty(ColorProperty) == false)
                        continue;

                    _surfaceColors.Add((renderer, slot, material.GetColor(ColorProperty)));
                }
            }
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
}
