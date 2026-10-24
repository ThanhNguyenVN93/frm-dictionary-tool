using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DictionaryTool.UI
{
    internal class DonateForm : Form
    {
        private const string KofiUrl = "https://ko-fi.com/thanhnguyen150993";

        public DonateForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            this.Text = "Đóng góp 🤝";
            this.BackColor = UiPalette.PageBackground;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(340, 460);
            this.AutoScaleMode = AutoScaleMode.None;

            var lblTitle = new Label
            {
                Text = "Ủng hộ tác giả ☕",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = UiPalette.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 48
            };

            var picMomo = new PictureBox
            {
                Width = 280,
                Height = 280,
                Left = 30,
                Top = 54,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            var momoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "momo.jpg");
            if (File.Exists(momoPath))
            {
                try { picMomo.Image = Image.FromFile(momoPath); }
                catch { picMomo.BackColor = UiPalette.CardBackground; }
            }
            else
            {
                picMomo.BackColor = UiPalette.CardBackground;
                var lblNoImg = new Label
                {
                    Text = "momo.jpg\nnot found",
                    ForeColor = UiPalette.TextSecondary,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                picMomo.Controls.Add(lblNoImg);
            }

            var lblOr = new Label
            {
                Text = "— hoặc —",
                Font = new Font("Segoe UI", 9F),
                ForeColor = UiPalette.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 30,
                Top = 342,
                Width = 280,
                Height = 22
            };

            var lnkKofi = new LinkLabel
            {
                Text = "☕  Ủng hộ qua Ko-fi",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Left = 30,
                Top = 368,
                Width = 280,
                Height = 36,
                LinkColor = UiPalette.Accent,
                ActiveLinkColor = UiPalette.AccentHover,
                VisitedLinkColor = UiPalette.Accent
            };
            lnkKofi.LinkClicked += (s, e) =>
            {
                try { System.Diagnostics.Process.Start(KofiUrl); }
                catch { }
            };

            var btnClose = new Guna.UI2.WinForms.Guna2Button
            {
                Text = "Đóng",
                Font = new Font("Segoe UI", 9.5F),
                BorderRadius = 10,
                FillColor = UiPalette.NeutralSoft,
                HoverState = { FillColor = UiPalette.AccentSoftHover },
                ForeColor = UiPalette.TextSecondary,
                Left = 120,
                Top = 410,
                Width = 100,
                Height = 36,
                Cursor = Cursors.Hand
            };
            btnClose.Click += (s, e) => Close();

            this.Controls.AddRange(new Control[] { lblTitle, picMomo, lblOr, lnkKofi, btnClose });
            this.ResumeLayout(false);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            foreach (Control c in Controls)
            {
                if (c is PictureBox pb) pb.Image?.Dispose();
            }
            base.OnFormClosed(e);
        }
    }
}
