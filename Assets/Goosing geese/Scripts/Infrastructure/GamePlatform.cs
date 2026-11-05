using System;
using System.Collections.Generic;
using GoosingGeese.Profile;
using UnityEngine;

namespace GoosingGeese.Infrastructure
{
    public static class GamePlatform
    {
        public static IGamePlatform Current { get; private set; } = new LocalGamePlatform();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Current = new LocalGamePlatform();
        }

        public static void Register(IGamePlatform platform)
        {
            Current = platform ?? throw new ArgumentNullException(nameof(platform));
        }

        private sealed class LocalGamePlatform : IGamePlatform
        {
            public string Language => null;
            public string ProfileScope => null;

            public string ReadCloudProfile()
            {
                return null;
            }

            public void ReportGameplay(bool isReady, bool isActive)
            {
            }

            public void RequestLeaderboard(string localPlayerId, Action<List<LeaderboardEntry>> onLoaded)
            {
                onLoaded?.Invoke(null);
            }

            public void SetLanguage(string language)
            {
            }

            public void SubmitScore(int score)
            {
            }

            public void WriteCloudProfile(string json)
            {
            }
        }
    }
}
