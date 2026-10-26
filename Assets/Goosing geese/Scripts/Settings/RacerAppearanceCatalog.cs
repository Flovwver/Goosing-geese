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
        [SerializeField] private string _upgradeId;
        [SerializeField] private string _russianName;
        [SerializeField] private string _englishName;
        [SerializeField] private RacerStats _stats;
        [SerializeField] private GameObject _model;
        [SerializeField] private Color _tint = Color.white;
        [SerializeField] private Sprite _pedalSprite;

        [Header("Progression")]
        [SerializeField, Min(0)] private int _purchasePrice;
        [SerializeField, Min(0)] private int _unlockLevel;
        [SerializeField, Range(0f, 1f)] private float _maximumGreenZone = 0.8f;
        [SerializeField, Range(0f, 1f)] private float _maximumRedStart = 0.92f;
        [SerializeField, Min(0.1f)] private float _upgradePriceMultiplier = 1f;

        public string UpgradeId => _upgradeId;
        public int PurchasePrice => _purchasePrice;
        public int UnlockLevel => _unlockLevel;
        public float MaximumGreenZone => _maximumGreenZone;
        public float MaximumRedStart => _maximumRedStart;
        public float UpgradePriceMultiplier => _upgradePriceMultiplier;
        public RacerStats Stats => _stats;
        public GameObject Model => _model;
        public Color Tint => _tint;
        public Sprite PedalSprite => _pedalSprite;

        public string GetName(bool english)
        {
            return english ? _englishName : _russianName;
        }
    }
}
