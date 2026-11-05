#if PLUGIN_YG_2 && YandexGamesPlatform_yg && !GOOSING_GEESE_DISABLE_YANDEX
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GoosingGeese.Infrastructure;
using GoosingGeese.Profile;
using UnityEngine;
using YG;
#if Leaderboards_yg && Authorization_yg
using YG.Utils.LB;
#endif

namespace GoosingGeese.Platforms
{
    internal sealed class YandexGamePlatform : MonoBehaviour, IGamePlatform
    {
        private const string LeaderboardName = "goosing_geese_points";
        private const string ReceiverName = "GoosingGeese.YandexPlatform";
        private bool _isReadyReported;
#if Leaderboards_yg && Authorization_yg
        private const float LeaderboardReadInterval = 16f;
        private string _localPlayerId;
        private string _requestedAccountId;
        private string _pendingScoreAccountId;
        private int _pendingScore;
        private float _nextLeaderboardRead;
        private bool _needsLeaderboard;
        private Action<List<LeaderboardEntry>> _onLeaderboardLoaded;
#endif

        public string Language
        {
            get
            {
#if Localization_yg
                if (YG2.isSDKEnabled && string.IsNullOrEmpty(YG2.lang) == false)
                    return YG2.lang == "ru" ? "ru" : "en";

                return null;
#else
                return null;
#endif
            }
        }

        public string ProfileScope
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return GoosingGeese_YandexReadProfile(0);
#else
                return null;
#endif
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            var receiver = new GameObject(ReceiverName);
            DontDestroyOnLoad(receiver);
            GamePlatform.Register(receiver.AddComponent<YandexGamePlatform>());
        }

        private void OnDestroy()
        {
#if Leaderboards_yg && Authorization_yg
            YG2.onGetLeaderboard -= OnLeaderboardLoaded;
#endif
        }

        public string ReadCloudProfile()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return GoosingGeese_YandexReadProfile(1);
#else
            return null;
#endif
        }

        public void ReportGameplay(bool isReady, bool isActive)
        {
            if (YG2.isSDKEnabled == false)
                return;

#if Leaderboards_yg && Authorization_yg
            TryRequestLeaderboard();
#endif

            if (isReady && _isReadyReported == false)
            {
                YG2.GameReadyAPI();
                _isReadyReported = true;
            }

            bool shouldPlay = isReady && isActive && YG2.isFocusWindowGame && YG2.isPauseGame == false && YG2.nowAdsShow == false;

            if (shouldPlay)
                YG2.GameplayStart();
            else
                YG2.GameplayStop();
        }

        public void RequestLeaderboard(string localPlayerId, Action<List<LeaderboardEntry>> onLoaded)
        {
#if Leaderboards_yg && Authorization_yg
            _localPlayerId = localPlayerId;
            _onLeaderboardLoaded = onLoaded;
            _needsLeaderboard = true;
            TryRequestLeaderboard();
#endif
        }

        public void SetLanguage(string language)
        {
#if Localization_yg
            if (YG2.isSDKEnabled)
                YG2.SwitchLanguage(language);
#endif
        }

        public void SubmitScore(int score)
        {
#if Leaderboards_yg && Authorization_yg
            if (YG2.isSDKEnabled == false || YG2.player.auth == false || score <= 0)
                return;

            _pendingScore = _pendingScoreAccountId == YG2.player.id ? Math.Max(_pendingScore, score) : score;
            _pendingScoreAccountId = YG2.player.id;
            _needsLeaderboard = true;
            TryRequestLeaderboard();
#endif
        }

        public void WriteCloudProfile(string json)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            GoosingGeese_YandexWriteProfile(json);
#endif
        }

#if Leaderboards_yg && Authorization_yg
        private void TryRequestLeaderboard()
        {
            if (_needsLeaderboard == false || YG2.isSDKEnabled == false || Time.realtimeSinceStartup < _nextLeaderboardRead)
                return;

            _needsLeaderboard = false;
            _nextLeaderboardRead = Time.realtimeSinceStartup + LeaderboardReadInterval;
            _requestedAccountId = YG2.player.id;
            YG2.onGetLeaderboard -= OnLeaderboardLoaded;
            YG2.onGetLeaderboard += OnLeaderboardLoaded;
            YG2.GetLeaderboard(LeaderboardName, 5, 1);
        }

        private void OnLeaderboardLoaded(LBData data)
        {
            if (data == null || data.technoName != LeaderboardName || YG2.player.id != _requestedAccountId)
                return;

            YG2.onGetLeaderboard -= OnLeaderboardLoaded;
            Action<List<LeaderboardEntry>> onLoaded = _onLeaderboardLoaded;
            _onLeaderboardLoaded = null;

            if (data.players == null || (data.players.Length == 1 && data.players[0].rank <= 0))
                return;

            int previousScore = data.currentPlayer?.score ?? 0;
            var entries = new List<LeaderboardEntry>();

            foreach (LBPlayerData player in data.players)
            {
                bool isCurrentPlayer = YG2.player.auth && player.uniqueID == _requestedAccountId;

                if (isCurrentPlayer)
                    previousScore = Math.Max(previousScore, player.score);

                entries.Add(new LeaderboardEntry
                {
                    PlayerId = isCurrentPlayer ? _localPlayerId : "",
                    Name = string.IsNullOrWhiteSpace(player.name) ? "—" : player.name,
                    Score = player.score,
                    Rank = player.rank
                });
            }

            if (YG2.player.auth && _pendingScoreAccountId == _requestedAccountId && _pendingScore > previousScore)
                YG2.SetLeaderboard(LeaderboardName, _pendingScore);

            _pendingScore = 0;

            if (entries.Count > 0)
                onLoaded?.Invoke(entries);
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GoosingGeese_YandexReadProfile(int readSnapshot);

        [DllImport("__Internal")]
        private static extern void GoosingGeese_YandexWriteProfile(string json);

#endif
    }
}
#endif
