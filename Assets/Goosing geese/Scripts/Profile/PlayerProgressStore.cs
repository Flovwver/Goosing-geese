using System;
using GoosingGeese.Infrastructure;
using GoosingGeese.Settings;
using UnityEngine;

namespace GoosingGeese.Profile
{
    public sealed class PlayerProgressStore
    {
        private const string SaveKey = "GoosingGeese.Profile.v1";

        private readonly IGamePlatform _platform;
        private readonly string _localKey;

        public PlayerProgressStore() : this(GamePlatform.Current)
        {
        }

        public PlayerProgressStore(IGamePlatform platform)
        {
            _platform = platform ?? throw new ArgumentNullException(nameof(platform));
            string scope = platform.ProfileScope;
            _localKey = string.IsNullOrEmpty(scope) ? SaveKey : SaveKey + "." + scope;
        }

        public PlayerProgress Load(RacerAppearanceCatalog catalog = null)
        {
            PlayerProgressData local = ReadProfile(PlayerPrefs.GetString(_localKey, ""));
            string cloudJson = _platform.ReadCloudProfile();
            PlayerProgressData cloud = ReadProfile(cloudJson);

            if (cloud != null && cloud.Level > 0 && string.IsNullOrWhiteSpace(cloud.PlayerId) == false && cloud.SavedAtUnixMs > (local?.SavedAtUnixMs ?? 0))
            {
                PlayerPrefs.SetString(_localKey, cloudJson);
                PlayerPrefs.Save();
                return new PlayerProgress(cloud, catalog);
            }

            return new PlayerProgress(local, catalog);
        }

        public void Save(PlayerProgress progress)
        {
            if (progress == null)
                throw new ArgumentNullException(nameof(progress));

            string json = progress.ToJson();

            if (json != PlayerPrefs.GetString(_localKey, ""))
            {
                json = progress.CreateSaveSnapshot();
                PlayerPrefs.SetString(_localKey, json);
                PlayerPrefs.Save();
            }

            _platform.WriteCloudProfile(json);
        }

        private static PlayerProgressData ReadProfile(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                return JsonUtility.FromJson<PlayerProgressData>(json);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
