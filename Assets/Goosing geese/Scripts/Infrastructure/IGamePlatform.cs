using System;
using System.Collections.Generic;
using GoosingGeese.Profile;

namespace GoosingGeese.Infrastructure
{
    public interface IGamePlatform
    {
        string Language { get; }

        string ProfileScope { get; }

        string ReadCloudProfile();

        void ReportGameplay(bool isReady, bool isActive);

        void RequestLeaderboard(string localPlayerId, Action<List<LeaderboardEntry>> onLoaded);

        void SubmitScore(int score);

        void WriteCloudProfile(string json);
    }
}
