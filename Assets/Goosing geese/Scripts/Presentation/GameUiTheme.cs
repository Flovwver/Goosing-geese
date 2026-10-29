using UnityEngine;

namespace GoosingGeese.Presentation
{
    public static class GameUiTheme
    {
        private const string FontResourcePath = "Fonts/RussoOne/RussoOne-Regular";
        private const string HeadingFontResourcePath = "Fonts/RubikMonoOne/RubikMonoOne-Regular";
        public static readonly Color Panel = new Color(0.25f, 0.81f, 0.94f, 0.98f);
        public static readonly Color Header = new Color(0.12f, 0.72f, 0.89f, 1f);
        public static readonly Color Button = new Color(0.04f, 0.62f, 0.83f, 1f);
        public static readonly Color LightRow = new Color(0.9f, 0.98f, 1f, 1f);
        public static readonly Color AlternateRow = new Color(0.7f, 0.92f, 0.98f, 1f);
        public static readonly Color Ink = new Color(0.02f, 0.32f, 0.46f, 1f);
        public static readonly Color Accent = new Color(1f, 0.88f, 0.16f, 1f);

        public static Font LoadFont()
        {
            return Resources.Load<Font>(FontResourcePath);
        }

        public static Font LoadHeadingFont()
        {
            return Resources.Load<Font>(HeadingFontResourcePath);
        }
    }
}
