using System;
using UnityEngine;

public sealed class ProfileCommands
{
    private readonly PlayerProgress _progress;

    public ProfileCommands(PlayerProgress progress)
    {
        _progress = progress ?? throw new ArgumentNullException(nameof(progress));
    }

    public void SetName(string name)
    {
        _progress.PlayerName = string.IsNullOrWhiteSpace(name) ? "Player" : name.Trim();
        Save();
    }

    public void SetMusicVolume(float value)
    {
        _progress.MusicVolume = Mathf.Clamp01(value);
    }

    public void SetEffectsVolume(float value)
    {
        _progress.EffectsVolume = Mathf.Clamp01(value);
    }

    public void ToggleLanguage()
    {
        _progress.English = _progress.English == false;
        Save();
    }

    public void NextScenery(int count)
    {
        if (count < 1)
            throw new ArgumentOutOfRangeException(nameof(count));

        _progress.Environment = (_progress.Environment + 1) % count;
        Save();
    }

    public bool ClaimQuest(int quest, DateTime date)
    {
        if (QuestTracker.TryClaim(_progress, quest, date) == false)
            return false;

        Save();
        LocalLeaderboard.Record(_progress.PlayerName, _progress.Points);
        return true;
    }

    public void Save()
    {
        PlayerProgressStore.Save(_progress);
    }
}
