using System;
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
            public void ReportGameplay(bool isReady, bool isActive)
            {
            }
        }
    }
}
