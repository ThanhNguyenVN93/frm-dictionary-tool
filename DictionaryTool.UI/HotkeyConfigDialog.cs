using System;
using System.Windows.Forms;
using DictionaryTool.UI.Interop;

namespace DictionaryTool.UI
{
    /// <summary>
    /// Dialog cấu hình global hotkey — nhấn tổ hợp phím mới, Save/Cancel.
    /// Dựng thủ công bằng code (không qua Designer), theo quy ước các form phụ.
    /// </summary>
    internal class HotkeyConfigDialog : Form
    {
        public uint NewModifiers { get; private set; }
        public uint NewKey { get; private set; }

        private uint _capturedMods;
        private uint _capturedKey;
        private bool _hasCapture;

        private Label _lblInstruction;
        private Label _lblCurrent;
        private TextBox _txtCapture;
        private Label _lblError;
        private Button _btnSave;
        private Button _btnCancel;

        public HotkeyConfigDialog(uint currentModifiers, uint currentKey)
        {
            NewModifiers = currentModifiers;
            NewKey = currentKey;
            _capturedMods = currentModifiers;
            _capturedKey = currentKey;
            _hasCapture = true;

            BuildUI();
            UpdateCaptureDisplay();
        }

        private void BuildUI()
        {
            Text = "Cấu hình phím tắt";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new System.Drawing.Size(360, 220);
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = System.Drawing.Color.FromArgb(30, 30, 30);

            _lblInstruction = new Label
            {
                Text = "Nhấn tổ hợp phím mới (cần ít nhất 1 phím Ctrl/Shift/Alt):",
                ForeColor = System.Drawing.Color.FromArgb(200, 200, 200),
                Location = new System.Drawing.Point(16, 20),
                Size = new System.Drawing.Size(328, 36),
                AutoSize = false
            };

            _lblCurrent = new Label
            {
                Text = "Phím hiện tại:",
                ForeColor = System.Drawing.Color.FromArgb(160, 160, 160),
                Location = new System.Drawing.Point(16, 62),
                AutoSize = true
            };

            _txtCapture = new TextBox
            {
                Location = new System.Drawing.Point(16, 84),
                Size = new System.Drawing.Size(328, 32),
                Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold),
                ReadOnly = true,
                BackColor = System.Drawing.Color.FromArgb(50, 50, 50),
                ForeColor = System.Drawing.Color.White,
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Default,
                ShortcutsEnabled = false
            };
            _txtCapture.KeyDown += OnCaptureKeyDown;
            _txtCapture.PreviewKeyDown += OnCapturePreviewKeyDown;

            _lblError = new Label
            {
                Text = "",
                ForeColor = System.Drawing.Color.FromArgb(255, 100, 100),
                Location = new System.Drawing.Point(16, 124),
                Size = new System.Drawing.Size(328, 20),
                AutoSize = false
            };

            _btnSave = new Button
            {
                Text = "Lưu",
                Location = new System.Drawing.Point(176, 160),
                Size = new System.Drawing.Size(80, 36),
                BackColor = System.Drawing.Color.FromArgb(0, 120, 215),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.None
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += OnSaveClick;

            _btnCancel = new Button
            {
                Text = "Hủy",
                Location = new System.Drawing.Point(264, 160),
                Size = new System.Drawing.Size(80, 36),
                BackColor = System.Drawing.Color.FromArgb(60, 60, 60),
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            _btnCancel.FlatAppearance.BorderSize = 0;

            Controls.Add(_lblInstruction);
            Controls.Add(_lblCurrent);
            Controls.Add(_txtCapture);
            Controls.Add(_lblError);
            Controls.Add(_btnSave);
            Controls.Add(_btnCancel);

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            Shown += (s, e) => _txtCapture.Focus();
        }

        private void OnCapturePreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            // Cho phép xử lý phím Tab, arrow... qua KeyDown thay vì bị nuốt bởi form.
            e.IsInputKey = true;
        }

        private void OnCaptureKeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;

            var key = e.KeyCode;

            // Bỏ qua phím modifier đứng một mình.
            if (key == Keys.ControlKey || key == Keys.LControlKey || key == Keys.RControlKey ||
                key == Keys.ShiftKey || key == Keys.LShiftKey || key == Keys.RShiftKey ||
                key == Keys.Menu || key == Keys.LMenu || key == Keys.RMenu ||
                key == Keys.LWin || key == Keys.RWin)
            {
                return;
            }

            uint mods = 0;
            if ((e.Modifiers & Keys.Control) != 0) mods |= NativeMethods.MOD_CONTROL;
            if ((e.Modifiers & Keys.Shift) != 0) mods |= NativeMethods.MOD_SHIFT;
            if ((e.Modifiers & Keys.Alt) != 0) mods |= NativeMethods.MOD_ALT;

            _capturedMods = mods;
            _capturedKey = (uint)key;
            _hasCapture = true;
            _lblError.Text = "";

            UpdateCaptureDisplay();
        }

        private void OnSaveClick(object sender, EventArgs e)
        {
            if (!_hasCapture || _capturedMods == 0)
            {
                _lblError.Text = "Phải có ít nhất một phím Ctrl, Shift hoặc Alt.";
                return;
            }

            NewModifiers = _capturedMods;
            NewKey = _capturedKey;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void UpdateCaptureDisplay()
        {
            _txtCapture.Text = FormatCombo(_capturedMods, _capturedKey);
            _lblCurrent.Text = "Phím hiện tại: " + _txtCapture.Text;
        }

        /// <summary>
        /// Chuyển modifiers + VK thành chuỗi dễ đọc, vd "Ctrl+Shift+D".
        /// Static để MainForm có thể dùng khi hiện thông báo lỗi đăng ký.
        /// </summary>
        public static string FormatCombo(uint modifiers, uint vk)
        {
            var parts = new System.Collections.Generic.List<string>();
            if ((modifiers & NativeMethods.MOD_CONTROL) != 0) parts.Add("Ctrl");
            if ((modifiers & NativeMethods.MOD_ALT) != 0) parts.Add("Alt");
            if ((modifiers & NativeMethods.MOD_SHIFT) != 0) parts.Add("Shift");
            if ((modifiers & NativeMethods.MOD_WIN) != 0) parts.Add("Win");

            var keyName = ((Keys)vk).ToString();
            parts.Add(keyName);

            return string.Join("+", parts);
        }
    }
}
