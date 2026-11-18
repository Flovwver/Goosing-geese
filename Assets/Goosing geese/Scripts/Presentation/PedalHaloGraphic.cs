using UnityEngine;
using UnityEngine.UI;

namespace GoosingGeese.Presentation
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PedalHaloGraphic : RawImage
    {
        private const string TextureResource = "PedalHalo";

        protected override void Awake()
        {
            base.Awake();
            texture = Resources.Load<Texture2D>(TextureResource);
        }
    }
}
