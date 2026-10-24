using System.Drawing;

namespace DictionaryTool.UI
{
    internal static class UiPalette
    {
        public static readonly Color Accent = Color.FromArgb(124, 140, 255);
        public static readonly Color AccentHover = Color.FromArgb(145, 159, 255);
        public static readonly Color AccentPressed = Color.FromArgb(102, 118, 230);
        public static readonly Color AccentSoft = Color.FromArgb(51, 56, 84);
        public static readonly Color AccentSoftHover = Color.FromArgb(64, 70, 104);

        public static readonly Color PageBackground = Color.FromArgb(18, 20, 26);
        public static readonly Color CardBackground = Color.FromArgb(30, 33, 42);
        public static readonly Color BorderSoft = Color.FromArgb(48, 52, 63);

        public static readonly Color TextPrimary = Color.FromArgb(237, 238, 242);
        public static readonly Color TextSecondary = Color.FromArgb(156, 163, 175);

        public static readonly Color ErrorText = Color.FromArgb(255, 118, 118);
        public static readonly Color ErrorSoft = Color.FromArgb(58, 32, 32);
        public static readonly Color NeutralText = Color.FromArgb(156, 163, 175);
        public static readonly Color NeutralSoft = Color.FromArgb(46, 50, 61);
    }
}
