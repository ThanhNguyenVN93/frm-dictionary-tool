using System;
using System.Drawing;
using System.Windows.Forms;

namespace DictionaryTool.UI
{
    internal class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Hướng dẫn sử dụng";
            this.BackColor = UiPalette.PageBackground;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(520, 560);
            this.AutoScaleMode = AutoScaleMode.None;

            // ── Title ──────────────────────────────────────────────────────
            var lblTitle = new Label
            {
                Text = "📖  Hướng dẫn sử dụng",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = UiPalette.TextPrimary,
                Location = new Point(24, 20),
                Size = new Size(472, 36)
            };

            // ── Scrollable content ─────────────────────────────────────────
            var scroll = new Panel
            {
                Location = new Point(16, 62),
                Size = new Size(488, 446),
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            var flow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Width = 468,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 0, 0, 8)
            };

            AddSection(flow, "🔍  Tra từ",
                "Nhập từ vào ô tìm kiếm rồi nhấn Enter hoặc nút Tra từ.\n" +
                "Kết quả 3 ngôn ngữ (EN · VI · ZH) hiện cùng lúc.\n" +
                "Nhấn 🔊 để nghe phát âm.");

            AddSection(flow, "🌐  Nhận diện ngôn ngữ (chip EN · VI · ZH · Tự động)",
                "Chip ở góc trái ô tìm kiếm cho biết ngôn ngữ đang dùng.\n" +
                "Mặc định: Tự động — app tự phát hiện EN, VI hay ZH.\n" +
                "Click chip để chốt cứng một ngôn ngữ (EN → VI → ZH → Auto).\n" +
                "Dùng khi từ ngắn dễ nhầm (ví dụ: \"an\" — EN hay VI?).");

            AddSection(flow, "📋  Clipboard Watch",
                "Bật nút 📋 Bật để app tự tra từ bất cứ khi nào bạn copy\n" +
                "văn bản từ ứng dụng khác (trình duyệt, PDF, v.v.).\n" +
                "Nhấn lại để tắt.");

            AddSection(flow, "⌨️  Phím tắt toàn cục (Ctrl+Shift+D)",
                "Nhấn Ctrl+Shift+D bất cứ lúc nào (kể cả khi app bị thu nhỏ)\n" +
                "để đưa cửa sổ lên trước và focus vào ô tìm kiếm.\n" +
                "Đổi phím tắt bằng nút ⌨️ trên toolbar.");

            AddSection(flow, "⤢  Mini mode",
                "Nhấn nút ⤢ Mini để thu nhỏ cửa sổ — chỉ còn ô tìm kiếm\n" +
                "và kết quả gọn, tiện để xem cạnh tài liệu đang đọc.\n" +
                "Nhấn ⤢ Đầy đủ để trở lại chế độ thường.");

            AddSection(flow, "🔖  Đánh dấu & Lịch sử",
                "Nhấn nút 🔖 để đánh dấu từ đang xem.\n" +
                "Nhấn nút 🕐 để xem lịch sử tra từ và danh sách từ đã đánh dấu.\n" +
                "Trong cửa sổ lịch sử, click tên từ để tra lại ngay.");

            AddSection(flow, "ℹ️  Menu Giới thiệu",
                "Nhấn nút ℹ️ để mở menu:\n" +
                "  • Giới thiệu — thông tin về app\n" +
                "  • Góp ý — mở trang GitHub Issues để báo lỗi\n" +
                "  • Đóng góp — ủng hộ tác giả qua Momo / Ko-fi\n" +
                "  • Switch to English / Chuyển sang Tiếng Việt — đổi ngôn ngữ giao diện");

            scroll.Controls.Add(flow);

            // ── Close button ───────────────────────────────────────────────
            var btnClose = new Guna.UI2.WinForms.Guna2Button
            {
                Text = "Đóng",
                Font = new Font("Segoe UI", 9.5F),
                BorderRadius = 10,
                FillColor = UiPalette.NeutralSoft,
                HoverState = { FillColor = UiPalette.AccentSoftHover },
                ForeColor = UiPalette.TextSecondary,
                Location = new Point(210, 518),
                Size = new Size(100, 34),
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => Close();

            this.Controls.AddRange(new Control[] { lblTitle, scroll, btnClose });
            this.ResumeLayout(false);
        }

        private static void AddSection(FlowLayoutPanel parent, string heading, string body)
        {
            var sep = new Panel
            {
                Height = 1,
                Width = 468,
                BackColor = UiPalette.BorderSoft,
                Margin = new Padding(0, 6, 0, 10)
            };

            var lblHeading = new Label
            {
                Text = heading,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold),
                ForeColor = UiPalette.Accent,
                AutoSize = false,
                Width = 468,
                Height = 22,
                Margin = new Padding(0, 0, 0, 4)
            };

            var lblBody = new Label
            {
                Text = body,
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = UiPalette.TextSecondary,
                AutoSize = false,
                Width = 468,
                Height = MeasureHeight(body, new Font("Segoe UI", 9.5F), 468) + 6,
                Margin = new Padding(0, 0, 0, 12)
            };

            parent.Controls.Add(sep);
            parent.Controls.Add(lblHeading);
            parent.Controls.Add(lblBody);
        }

        private static int MeasureHeight(string text, Font font, int width)
        {
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                var sz = g.MeasureString(text, font, width);
                return (int)Math.Ceiling(sz.Height);
            }
        }
    }
}
