using UnityEngine;

namespace GoosingGeese.Presentation
{
    [RequireComponent(typeof(GameAudio))]
    public sealed class GameUiBootstrap : MonoBehaviour
    {
        private GameUiFactory _factory;

        public GameUiFactory Factory => _factory ??= new GameUiFactory(new GameUiTheme(), GetComponent<GameAudio>().PlayClick);
    }
}
