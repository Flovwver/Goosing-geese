using UnityEngine;

namespace GoosingGeese.Presentation
{
    public sealed class GameUiTheme
    {
        private const string FontResourcePath = "Fonts/RussoOne/RussoOne-Regular";
        private const string HeadingFontResourcePath = "Fonts/RubikMonoOne/RubikMonoOne-Regular";

        public Color Panel { get; } = new Color(0.25f, 0.81f, 0.94f, 0.98f);
        public Color Header { get; } = new Color(0.12f, 0.72f, 0.89f, 1f);
        public Color Button { get; } = new Color(0.04f, 0.62f, 0.83f, 1f);
        public Color LightRow { get; } = new Color(0.9f, 0.98f, 1f, 1f);
        public Color AlternateRow { get; } = new Color(0.7f, 0.92f, 0.98f, 1f);
        public Color Ink { get; } = new Color(0.02f, 0.32f, 0.46f, 1f);
        public Color Accent { get; } = new Color(1f, 0.88f, 0.16f, 1f);

        public Font LoadFont()
        {
            return Resources.Load<Font>(FontResourcePath);
        }

        public Font LoadHeadingFont()
        {
            return Resources.Load<Font>(HeadingFontResourcePath);
        }
    }
}
