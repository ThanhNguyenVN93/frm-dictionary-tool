using System.Drawing;

namespace DictionaryTool.UI
{
    /// <summary>
    /// Bảng màu giao diện. Trước đây chỉ có dark theme (static readonly); nay hỗ trợ
    /// cả light theme (#4) nên các màu là static mutable, đổi đồng loạt qua
    /// <see cref="Apply"/>. Mọi control tạo động khi render đọc màu tại thời điểm vẽ
    /// nên tự động đúng theme; các control tĩnh (Designer) được <see cref="ThemeManager"/>
    /// đổi màu bằng cách duyệt cây control.
    /// </summary>
    internal static class UiPalette
    {
        public static bool IsLight { get; private set; }

        public static Color Accent;
        public static Color AccentHover;
        public static Color AccentPressed;
        public static Color AccentSoft;
        public static Color AccentSoftHover;

        public static Color PageBackground;
        public static Color CardBackground;
        public static Color BorderSoft;

        public static Color TextPrimary;
        public static Color TextSecondary;

        public static Color ErrorText;
        public static Color ErrorSoft;
        public static Color NeutralText;
        public static Color NeutralSoft;

        static UiPalette()
        {
            Apply(false);
        }

        public static void Apply(bool light)
        {
            IsLight = light;
            if (light) ApplyLight();
            else ApplyDark();
        }

        private static void ApplyDark()
        {
            Accent          = Color.FromArgb(124, 140, 255);
            AccentHover     = Color.FromArgb(145, 159, 255);
            AccentPressed   = Color.FromArgb(102, 118, 230);
            AccentSoft      = Color.FromArgb(51, 56, 84);
            AccentSoftHover = Color.FromArgb(64, 70, 104);

            PageBackground  = Color.FromArgb(18, 20, 26);
            CardBackground  = Color.FromArgb(30, 33, 42);
            BorderSoft      = Color.FromArgb(48, 52, 63);

            TextPrimary     = Color.FromArgb(237, 238, 242);
            TextSecondary   = Color.FromArgb(156, 163, 175);

            ErrorText       = Color.FromArgb(255, 118, 118);
            ErrorSoft       = Color.FromArgb(58, 32, 32);
            NeutralText     = Color.FromArgb(156, 163, 175);
            NeutralSoft     = Color.FromArgb(46, 50, 61);
        }

        private static void ApplyLight()
        {
            Accent          = Color.FromArgb(79, 96, 220);
            AccentHover     = Color.FromArgb(99, 116, 230);
            AccentPressed   = Color.FromArgb(60, 78, 200);
            AccentSoft      = Color.FromArgb(226, 230, 250);
            AccentSoftHover = Color.FromArgb(212, 218, 248);

            PageBackground  = Color.FromArgb(245, 246, 250);
            CardBackground  = Color.FromArgb(255, 255, 255);
            BorderSoft      = Color.FromArgb(222, 226, 233);

            TextPrimary     = Color.FromArgb(28, 30, 38);
            TextSecondary   = Color.FromArgb(90, 98, 112);

            ErrorText       = Color.FromArgb(200, 40, 40);
            ErrorSoft       = Color.FromArgb(252, 230, 230);
            NeutralText     = Color.FromArgb(90, 98, 112);
            NeutralSoft     = Color.FromArgb(232, 235, 240);
        }
    }
}
