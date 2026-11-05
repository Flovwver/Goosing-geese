#if PLUGIN_YG_2 && YandexGamesPlatform_yg && !GOOSING_GEESE_DISABLE_YANDEX
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GoosingGeese.Infrastructure;
using GoosingGeese.Profile;
using UnityEngine;
using UnityEngine.Scripting;
using YG;

namespace GoosingGeese.Platforms
{
    internal sealed class YandexGamePlatform : MonoBehaviour, IGamePlatform
    {
        private const string LeaderboardName = "goosing_geese_points";
        private const string ReceiverName = "GoosingGeese.YandexPlatform";
        private bool _isReadyReported;
        private int _requestId;
        private Action<List<LeaderboardEntry>> _onLeaderboardLoaded;

        public string Language
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                int language = GoosingGeese_YandexLanguage();
                return language == 0 ? null : language == 1 ? "ru" : "en";
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
#if UNITY_WEBGL && !UNITY_EDITOR
            _onLeaderboardLoaded = onLoaded;
            GoosingGeese_YandexReadLeaderboard(LeaderboardName, localPlayerId, ReceiverName, ++_requestId);
#endif
        }

        public void SubmitScore(int score)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            GoosingGeese_YandexSubmitScore(LeaderboardName, score);
#endif
        }

        public void WriteCloudProfile(string json)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            GoosingGeese_YandexWriteProfile(json);
#endif
        }

        [Preserve]
        public void ReceiveLeaderboard(string json)
        {
            var response = JsonUtility.FromJson<LeaderboardResponse>(json);

            if (response == null || response.RequestId != _requestId)
                return;

            Action<List<LeaderboardEntry>> onLoaded = _onLeaderboardLoaded;
            _onLeaderboardLoaded = null;

            if (response.Entries != null && response.Entries.Count > 0)
                onLoaded?.Invoke(response.Entries);
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GoosingGeese_YandexReadProfile(int readSnapshot);

        [DllImport("__Internal")]
        private static extern void GoosingGeese_YandexWriteProfile(string json);

        [DllImport("__Internal")]
        private static extern int GoosingGeese_YandexLanguage();

        [DllImport("__Internal")]
        private static extern void GoosingGeese_YandexReadLeaderboard(string board, string localPlayerId, string receiver, int requestId);

        [DllImport("__Internal")]
        private static extern void GoosingGeese_YandexSubmitScore(string board, int score);
#endif

        [Serializable]
        private sealed class LeaderboardResponse
        {
            public int RequestId;
            public List<LeaderboardEntry> Entries;
        }
    }
}
#endif
