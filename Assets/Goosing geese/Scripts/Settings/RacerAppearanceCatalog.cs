using UnityEngine;

[CreateAssetMenu(fileName = "RacerAppearanceCatalog", menuName = "Racer/Appearance Catalog")]
public class RacerAppearanceCatalog : ScriptableObject
{
    [SerializeField] private RacerAppearanceDefinition[] _entries;

    public int Count => _entries != null ? _entries.Length : 0;

    public int GetRelativeModelIndex(int index, int direction)
    {
        string currentId = GetEntry(index).UpgradeId;
        int next = index;

        for (int count = 0; count < Count; count++)
        {
            next = (next + direction + Count) % Count;

            if (GetEntry(next).UpgradeId != currentId)
                return next;
        }

        return index;
    }

    public int[] GetColorIndices(int index)
    {
        var colors = new System.Collections.Generic.List<int>();
        string modelId = GetEntry(index).UpgradeId;

        for (int candidate = 0; candidate < Count; candidate++)
        {
            if (GetEntry(candidate).UpgradeId == modelId)
                colors.Add(candidate);
        }

        return colors.ToArray();
    }

    public RacerAppearanceDefinition GetEntry(int index)
    {
        if (Count == 0)
            return null;

        return _entries[Mathf.Clamp(index, 0, Count - 1)];
    }
}
