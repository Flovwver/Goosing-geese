#if PLUGIN_YG_2 && YandexGamesPlatform_yg && !GOOSING_GEESE_DISABLE_YANDEX
using GoosingGeese.Infrastructure;
using UnityEngine;
using YG;

namespace GoosingGeese.Platforms
{
    internal sealed class YandexGamePlatform : IGamePlatform
    {
        private bool _isReadyReported;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            GamePlatform.Register(new YandexGamePlatform());
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
    }
}
#endif
