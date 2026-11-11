using System;
using System.Collections.Generic;
using GoosingGeese.Profile;
using UnityEngine;

namespace GoosingGeese.Presentation
{
    public sealed class Localizer
    {
        private const string RussianResource = "Localization/ru";
        private const string EnglishResource = "Localization/en";

        private readonly PlayerProgress _profile;
        private readonly Locale _russian;
        private readonly Locale _english;

        public Localizer(PlayerProgress profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
            _russian = Load(RussianResource);
            _english = Load(EnglishResource);
        }

        public int PedalPromptCount => Math.Min(_russian.PedalPrompts.Length, _english.PedalPrompts.Length);
        private Locale Current => _profile.UseEnglish ? _english : _russian;

        public string Get(string key)
        {
            return Current.Text[key];
        }

        public string GetPedalPrompt(int index)
        {
            return Current.PedalPrompts[index];
        }

        private Locale Load(string path)
        {
            TextAsset asset = Resources.Load<TextAsset>(path);

            if (asset == null)
                throw new ArgumentException(null, nameof(path));

            Locale locale = JsonUtility.FromJson<Locale>(asset.text);
            locale.Index();
            return locale;
        }

        [Serializable]
        private sealed class Locale
        {
            [SerializeField] private Entry[] _entries;
            [SerializeField] private string[] _pedalPrompts;

            public Dictionary<string, string> Text { get; private set; }
            public string[] PedalPrompts => _pedalPrompts;

            public void Index()
            {
                Text = new Dictionary<string, string>(StringComparer.Ordinal);

                foreach (Entry entry in _entries)
                    Text.Add(entry.Key, entry.Text);
            }
        }

        [Serializable]
        private sealed class Entry
        {
            [SerializeField] private string _key;
            [SerializeField] private string _text;

            public string Key => _key;
            public string Text => _text;
        }
    }
}
