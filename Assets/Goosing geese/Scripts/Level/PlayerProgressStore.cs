using System;
using UnityEngine;

public sealed class PlayerProgressStore
{
    private const string SaveKey = "GoosingGeese.Profile.v1";

    public PlayerProgress Load(RacerAppearanceCatalog catalog = null)
    {
        PlayerProgressData data = null;

        try
        {
            data = JsonUtility.FromJson<PlayerProgressData>(PlayerPrefs.GetString(SaveKey, ""));
        }
        catch (ArgumentException)
        {
            data = null;
        }

        return new PlayerProgress(data, catalog);
    }

    public void Save(PlayerProgress progress)
    {
        if (progress == null)
            throw new ArgumentNullException(nameof(progress));

        PlayerPrefs.SetString(SaveKey, progress.ToJson());
        PlayerPrefs.Save();
    }
}
