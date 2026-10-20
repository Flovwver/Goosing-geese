using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RacerAppearanceCatalog", menuName = "Racer/Appearance Catalog")]
public class RacerAppearanceCatalog : ScriptableObject
{
    [SerializeField] private Entry[] _entries;

    public int Count => _entries != null ? _entries.Length : 0;

    public Entry GetEntry(int index)
    {
        if (Count == 0)
            return null;

        return _entries[Mathf.Clamp(index, 0, Count - 1)];
    }

    [Serializable]
    public class Entry
    {
        [SerializeField] private GameObject _model;
        [SerializeField] private Color _tint = Color.white;
        [SerializeField] private Sprite _pedalSprite;

        public GameObject Model => _model;
        public Color Tint => _tint;
        public Sprite PedalSprite => _pedalSprite;
    }
}
