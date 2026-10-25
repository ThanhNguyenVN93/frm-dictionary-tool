using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DictionaryTool.UI
{
    /// <summary>
    /// Đổi theme (dark ↔ light) lúc runtime cho các control tĩnh do Designer tạo.
    /// Designer ghi màu dưới dạng ARGB literal trùng đúng các token trong UiPalette,
    /// nên ta remap theo cặp màu dark↔light. Control tạo động khi render đọc UiPalette
    /// trực tiếp nên không cần remap (chỉ cần render lại sau khi đổi theme).
    /// </summary>
    internal static class ThemeManager
    {
        // Cặp màu dark/light — PHẢI giữ đồng bộ với UiPalette.ApplyDark/ApplyLight.
        private static readonly (Color Dark, Color Light)[] Pairs =
        {
            (Color.FromArgb(124, 140, 255), Color.FromArgb(79, 96, 220)),
            (Color.FromArgb(145, 159, 255), Color.FromArgb(99, 116, 230)),
            (Color.FromArgb(102, 118, 230), Color.FromArgb(60, 78, 200)),
            (Color.FromArgb(51, 56, 84),    Color.FromArgb(226, 230, 250)),
            (Color.FromArgb(64, 70, 104),   Color.FromArgb(212, 218, 248)),
            (Color.FromArgb(18, 20, 26),    Color.FromArgb(245, 246, 250)),
            (Color.FromArgb(30, 33, 42),    Color.FromArgb(255, 255, 255)),
            (Color.FromArgb(48, 52, 63),    Color.FromArgb(222, 226, 233)),
            (Color.FromArgb(237, 238, 242), Color.FromArgb(28, 30, 38)),
            (Color.FromArgb(156, 163, 175), Color.FromArgb(90, 98, 112)),
            (Color.FromArgb(255, 118, 118), Color.FromArgb(200, 40, 40)),
            (Color.FromArgb(58, 32, 32),    Color.FromArgb(252, 230, 230)),
            (Color.FromArgb(46, 50, 61),    Color.FromArgb(232, 235, 240)),
        };

        private static readonly string[] ColorProps =
        {
            "FillColor", "BorderColor", "PlaceholderForeColor"
        };

        /// <summary>Đặt palette theo theme rồi duyệt cây control remap màu.</summary>
        public static void ApplyTheme(Control root, bool light)
        {
            UiPalette.Apply(light);

            var map = new Dictionary<Color, Color>();
            foreach (var pair in Pairs)
            {
                var from = light ? pair.Dark : pair.Light;
                var to   = light ? pair.Light : pair.Dark;
                map[from] = to;
            }

            Walk(root, map);
        }

        private static void Walk(Control control, Dictionary<Color, Color> map)
        {
            Remap(control, "BackColor", map);
            Remap(control, "ForeColor", map);
            foreach (var name in ColorProps)
                Remap(control, name, map);

            // Trạng thái hover của Guna2Button (set trong code lúc render).
            var hover = GetMember(control, "HoverState");
            if (hover != null)
                Remap(hover, "FillColor", map);

            foreach (Control child in control.Controls)
                Walk(child, map);
        }

        private static void Remap(object target, string propName, Dictionary<Color, Color> map)
        {
            try
            {
                var prop = target.GetType().GetProperty(propName);
                if (prop == null || !prop.CanRead || !prop.CanWrite || prop.PropertyType != typeof(Color))
                    return;

                var current = (Color)prop.GetValue(target);
                if (map.TryGetValue(Color.FromArgb(current.R, current.G, current.B), out var replacement))
                    prop.SetValue(target, replacement);
            }
            catch (Exception)
            {
                // Bỏ qua control/property không set được — không để đổi theme làm crash.
            }
        }

        private static object GetMember(object target, string propName)
        {
            try
            {
                var prop = target.GetType().GetProperty(propName);
                return prop?.GetValue(target);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
