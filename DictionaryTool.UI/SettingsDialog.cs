using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace DictionaryTool.UI
{
    /// <summary>
    /// Cửa sổ Cài đặt gom các tùy chọn vốn rải rác trong menu ℹ️ về một chỗ:
    /// giao diện, cỡ chữ, tự động tra (+ độ trễ), clipboard, chạy nền ở khay, phím tắt.
    /// Mọi thay đổi áp dụng ngay qua MainForm (giống menu), không có OK/Cancel.
    /// Dựng bằng code theo theme hiện tại, cùng style với HistoryBookmarkForm.
    /// </summary>
    public class SettingsDialog : Form
    {
        private static readonly float[] FontFactors = { 0.85f, 1.0f, 1.25f, 1.5f };

        private readonly MainForm _owner;
        private Guna2Button[] _themeButtons;
        private Guna2Button[] _fontButtons;
        private CheckBox _chkAutoSearch;
        private NumericUpDown _numDelay;
        private CheckBox _chkClipboard;
        private CheckBox _chkCloseToTray;
        private Label _lblHotkeyValue;

        public SettingsDialog(MainForm owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));

            Text = UiStrings.SettingsTitle;
            BackColor = UiPalette.PageBackground;
            ForeColor = UiPalette.TextPrimary;
            ClientSize = new Size(470, 404);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            // --- Giao diện (theme) ---
            AddRowLabel(UiStrings.SettingsTheme, 24);
            _themeButtons = new[]
            {
                MakeSegment(UiStrings.SettingsThemeLight, 190, 20, 120),
                MakeSegment(UiStrings.SettingsThemeDark,  314, 20, 120)
            };
            _themeButtons[0].Click += (s, e) => { _owner.SetTheme(true);  UpdateThemeSelection(); };
            _themeButtons[1].Click += (s, e) => { _owner.SetTheme(false); UpdateThemeSelection(); };

            // --- Cỡ chữ ---
            AddRowLabel(UiStrings.SettingsFontSize, 70);
            var fontLabels = new[] { UiStrings.FontSmall, UiStrings.FontMedium, UiStrings.FontLarge, UiStrings.FontXLarge };
            _fontButtons = new Guna2Button[FontFactors.Length];
            int fx = 150;
            for (int i = 0; i < FontFactors.Length; i++)
            {
                var factor = FontFactors[i];
                _fontButtons[i] = MakeSegment(fontLabels[i], fx, 66, 72);
                _fontButtons[i].Click += (s, e) => { _owner.SetFontScale(factor); UpdateFontSelection(); };
                fx += 76;
            }

            // --- Tự động tra khi gõ + độ trễ ---
            _chkAutoSearch = MakeCheck(UiStrings.SettingsAutoSearch, 118, _owner.AutoSearchEnabled);
            _numDelay = new NumericUpDown
            {
                Location = new Point(300, 116),
                Size = new Size(96, 28),
                Minimum = 500,
                Maximum = 5000,
                Increment = 250,
                Value = Math.Min(5000, Math.Max(500, _owner.AutoSearchDelay)),
                BackColor = UiPalette.CardBackground,
                ForeColor = UiPalette.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = _owner.AutoSearchEnabled
            };
            var lblMs = new Label { Text = "ms", AutoSize = true, ForeColor = UiPalette.TextSecondary, Location = new Point(400, 120) };
            _chkAutoSearch.CheckedChanged += (s, e) =>
            {
                _numDelay.Enabled = _chkAutoSearch.Checked;
                _owner.SetAutoSearch(_chkAutoSearch.Checked, (int)_numDelay.Value);
            };
            _numDelay.ValueChanged += (s, e) => _owner.SetAutoSearch(_chkAutoSearch.Checked, (int)_numDelay.Value);

            // --- Tự động tra khi copy (clipboard) ---
            _chkClipboard = MakeCheck(UiStrings.SettingsClipboard, 160, _owner.ClipboardWatchEnabled);
            _chkClipboard.CheckedChanged += (s, e) => _owner.SetClipboardWatch(_chkClipboard.Checked);

            // --- Chạy nền ở khay ---
            _chkCloseToTray = MakeCheck(UiStrings.SettingsCloseToTray, 202, _owner.CloseToTray);
            _chkCloseToTray.CheckedChanged += (s, e) => _owner.SetCloseToTray(_chkCloseToTray.Checked);

            // --- Phím tắt gọi nhanh ---
            // Nhãn "Phím tắt gọi nhanh" khá dài (~150px) nên tổ hợp phím đặt ở x=210
            // để không đè lên nhãn; nút "Đổi…" dời sang phải tương ứng.
            AddRowLabel(UiStrings.SettingsHotkey, 250);
            _lblHotkeyValue = new Label
            {
                Text = _owner.HotkeyCombo,
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = UiPalette.Accent,
                Location = new Point(210, 253)
            };
            var btnHotkey = MakeSegment(UiStrings.SettingsChangeHotkey, 342, 246, 104);
            btnHotkey.Click += (s, e) =>
            {
                _owner.OpenHotkeyConfigDialog();
                _lblHotkeyValue.Text = _owner.HotkeyCombo;
            };

            // --- Đóng ---
            var btnClose = new Guna2Button
            {
                Text = UiStrings.SettingsClose,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BorderRadius = 14,
                FillColor = UiPalette.Accent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Location = new Point(340, 352),
                Size = new Size(106, 40)
            };
            btnClose.HoverState.FillColor = UiPalette.AccentHover;
            btnClose.ShadowDecoration.Enabled = false;
            btnClose.Click += (s, e) => Close();

            Controls.Add(_numDelay);
            Controls.Add(lblMs);
            Controls.Add(_chkAutoSearch);
            Controls.Add(_chkClipboard);
            Controls.Add(_chkCloseToTray);
            Controls.Add(_lblHotkeyValue);
            Controls.Add(btnHotkey);
            Controls.Add(btnClose);

            UpdateThemeSelection();
            UpdateFontSelection();
        }

        private void AddRowLabel(string text, int y)
        {
            Controls.Add(new Label
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = UiPalette.TextSecondary,
                Location = new Point(24, y + 4)
            });
        }

        private Guna2Button MakeSegment(string text, int x, int y, int width)
        {
            var b = new Guna2Button
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5F),
                BorderRadius = 10,
                FillColor = UiPalette.NeutralSoft,
                ForeColor = UiPalette.TextSecondary,
                Cursor = Cursors.Hand,
                Location = new Point(x, y),
                Size = new Size(width, 34)
            };
            b.HoverState.FillColor = UiPalette.AccentSoftHover;
            b.ShadowDecoration.Enabled = false;
            Controls.Add(b);
            return b;
        }

        private CheckBox MakeCheck(string text, int y, bool @checked)
        {
            return new CheckBox
            {
                Text = text,
                AutoSize = true,
                Checked = @checked,
                Font = new Font("Segoe UI", 10F),
                ForeColor = UiPalette.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(24, y)
            };
        }

        private void SetSelected(Guna2Button b, bool selected)
        {
            b.FillColor = selected ? UiPalette.Accent : UiPalette.NeutralSoft;
            b.ForeColor = selected ? Color.White : UiPalette.TextSecondary;
        }

        private void UpdateThemeSelection()
        {
            SetSelected(_themeButtons[0], _owner.IsLightTheme);
            SetSelected(_themeButtons[1], !_owner.IsLightTheme);
        }

        private void UpdateFontSelection()
        {
            for (int i = 0; i < _fontButtons.Length; i++)
                SetSelected(_fontButtons[i], Math.Abs(UiFont.Scale - FontFactors[i]) < 0.01f);
        }
    }
}
