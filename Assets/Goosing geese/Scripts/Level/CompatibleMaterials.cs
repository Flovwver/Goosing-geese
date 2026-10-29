using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CompatibleMaterials : IDisposable
{
    private readonly Dictionary<Material, Material> _cache = new Dictionary<Material, Material>();
    [SerializeField] private List<Material> _sources = new List<Material>();
    [SerializeField] private List<Material> _copies = new List<Material>();

    public void Dispose()
    {
        foreach (Material material in _copies)
            UnityEngine.Object.Destroy(material);

        _copies.Clear();
        _sources.Clear();
        _cache.Clear();
    }

    public void Apply(GameObject decoration, Material fallback)
{
        if (_cache.Count == 0)
        {
            for (int index = 0; index < _sources.Count; index++)
            {
                if (_sources[index] != null && _copies[index] != null)
                    _cache[_sources[index]] = _copies[index];
            }
        }

        foreach (Renderer renderer in decoration.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;

            for (int i = 0; i < materials.Length; i++)
            {
                Material source = materials[i];

                if (source == null || (source.shader != null && source.shader.isSupported && source.shader.name != "Hidden/InternalErrorShader"))
                    continue;

                if (_cache.TryGetValue(source, out Material compatible) == false)
                {
                    compatible = new Material(fallback);
                    compatible.name = source.name + " (compatible)";
                    compatible.color = Color.white;
                    compatible.mainTexture = source.mainTexture;
                    compatible.mainTextureScale = source.mainTextureScale;
                    compatible.mainTextureOffset = source.mainTextureOffset;
                    _cache.Add(source, compatible);
                    _sources.Add(source);
                    _copies.Add(compatible);
                }

                materials[i] = compatible;
            }

            renderer.sharedMaterials = materials;
        }
    }
}
