using System.Drawing;

namespace DictionaryTool.UI
{
    /// <summary>
    /// Cỡ chữ toàn cục có thể phóng to/thu nhỏ (#3). Mọi font tạo động khi render
    /// kết quả đi qua <see cref="Get"/> để co giãn theo <see cref="Scale"/>.
    /// </summary>
    internal static class UiFont
    {
        public const string Family = "Segoe UI";

        /// <summary>Hệ số phóng chữ hiện tại (0.85 = nhỏ, 1.0 = vừa, 1.25 = lớn...).</summary>
        public static float Scale { get; set; } = 1.0f;

        public static Font Get(float size, FontStyle style = FontStyle.Regular)
        {
            return new Font(Family, size * Scale, style);
        }
    }
}
