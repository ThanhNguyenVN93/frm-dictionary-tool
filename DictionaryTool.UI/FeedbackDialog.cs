using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DictionaryTool.Core.Repositories;
using OfficeOpenXml;

namespace DictionaryTool.UI
{
    /// <summary>
    /// Dialog góp ý bản dịch: Tab 1 nhập góp ý mới (pre-filled từ kết quả đang xem),
    /// Tab 2 xem danh sách + xuất .xlsx / copy sang Google Sheets.
    /// Xây dựng hoàn toàn trong code (không có .Designer.cs) để tránh phụ thuộc
    /// WinForms Designer khi tạo dialog đơn giản.
    /// </summary>
    public class FeedbackDialog : Form
    {
        private readonly UserFeedbackRepository _repo;

        // Tab 1
        private TextBox _txtQuery;
        private TextBox _txtWrong;
        private TextBox _txtSuggestion;
        private TextBox _txtNote;

        // Tab 2
        private DataGridView _grid;
        private List<UserFeedbackEntry> _entries;

        public FeedbackDialog(UserFeedbackRepository repo, string prefillQuery = null, string prefillWrong = null)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));

            BuildUi(prefillQuery, prefillWrong);
        }

        private void BuildUi(string prefillQuery, string prefillWrong)
        {
            Text            = UiStrings.IsVietnamese ? "Góp ý bản dịch" : "Contribute Translation";
            Size            = new Size(560, 440);
            MinimumSize     = new Size(480, 380);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = Color.FromArgb(22, 25, 34);
            ForeColor       = Color.FromArgb(210, 215, 230);
            Font            = new Font("Segoe UI", 9.5f);

            var tabs = new TabControl
            {
                Dock      = DockStyle.Fill,
                Padding   = new Point(12, 4),
                BackColor = Color.FromArgb(22, 25, 34),
                ForeColor = Color.FromArgb(210, 215, 230),
            };

            tabs.TabPages.Add(BuildInputTab(prefillQuery, prefillWrong));
            tabs.TabPages.Add(BuildListTab());

            tabs.Selected += (s, e) =>
            {
                if (e.TabPageIndex == 1)
                    _ = RefreshListAsync();
            };

            Controls.Add(tabs);
        }

        // ─── Tab 1: Góp ý mới ────────────────────────────────────────────────

        private TabPage BuildInputTab(string prefillQuery, string prefillWrong)
        {
            var page = new TabPage(UiStrings.IsVietnamese ? "Góp ý mới" : "New feedback")
            {
                BackColor = Color.FromArgb(22, 25, 34),
                ForeColor = Color.FromArgb(210, 215, 230),
                Padding   = new Padding(12)
            };

            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 5,
                Padding     = new Padding(8),
                BackColor   = Color.FromArgb(22, 25, 34)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _txtQuery      = MakeTextBox(prefillQuery ?? "");
            _txtWrong      = MakeTextBox(prefillWrong ?? "");
            _txtSuggestion = MakeTextBox("");
            _txtNote       = MakeTextBox("");

            AddRow(layout, 0, UiStrings.IsVietnamese ? "Từ đã tra:" : "Query:", _txtQuery);
            AddRow(layout, 1, UiStrings.IsVietnamese ? "Kết quả sai:" : "Wrong result:", _txtWrong);
            AddRow(layout, 2, UiStrings.IsVietnamese ? "Đề xuất sửa:" : "Suggestion:", _txtSuggestion);
            AddRow(layout, 3, UiStrings.IsVietnamese ? "Ghi chú:" : "Note:", _txtNote);

            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor     = Color.FromArgb(22, 25, 34),
                Padding       = new Padding(0, 8, 0, 0)
            };

            var btnCancel = MakeButton(UiStrings.IsVietnamese ? "Hủy" : "Cancel", UiPalette.NeutralSoft);
            var btnSave   = MakeButton(UiStrings.IsVietnamese ? "Lưu" : "Save", UiPalette.Accent);

            btnCancel.Click += (s, e) => Close();
            btnSave.Click   += async (s, e) => await SaveFeedbackAsync();

            btnPanel.Controls.Add(btnCancel);
            btnPanel.Controls.Add(btnSave);

            layout.Controls.Add(btnPanel, 0, 4);
            layout.SetColumnSpan(btnPanel, 2);

            page.Controls.Add(layout);
            return page;
        }

        private async Task SaveFeedbackAsync()
        {
            var query = _txtQuery.Text.Trim();
            if (string.IsNullOrEmpty(query) && string.IsNullOrEmpty(_txtSuggestion.Text.Trim()))
            {
                MessageBox.Show(
                    UiStrings.IsVietnamese ? "Vui lòng nhập từ đã tra hoặc đề xuất sửa." : "Please enter the query or a suggestion.",
                    UiStrings.IsVietnamese ? "Thiếu thông tin" : "Missing info",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await _repo.SaveAsync(query, _txtWrong.Text.Trim(), _txtSuggestion.Text.Trim(), _txtNote.Text.Trim());

            MessageBox.Show(
                UiStrings.IsVietnamese ? "Đã lưu. Cảm ơn!" : "Saved. Thank you!",
                UiStrings.IsVietnamese ? "Thành công" : "Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            _txtWrong.Text      = "";
            _txtSuggestion.Text = "";
            _txtNote.Text       = "";
        }

        // ─── Tab 2: Danh sách & xuất ─────────────────────────────────────────

        private TabPage BuildListTab()
        {
            var page = new TabPage(UiStrings.IsVietnamese ? "Danh sách" : "All feedback")
            {
                BackColor = Color.FromArgb(22, 25, 34),
                ForeColor = Color.FromArgb(210, 215, 230),
                Padding   = new Padding(8)
            };

            _grid = new DataGridView
            {
                Dock                     = DockStyle.Fill,
                ReadOnly                 = true,
                AllowUserToAddRows       = false,
                AllowUserToDeleteRows    = false,
                AllowUserToResizeRows    = false,
                SelectionMode            = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect              = false,
                AutoSizeColumnsMode      = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible        = false,
                BackgroundColor          = Color.FromArgb(30, 33, 42),
                ForeColor                = Color.FromArgb(200, 205, 220),
                GridColor                = Color.FromArgb(45, 48, 60),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(38, 41, 52),
                    ForeColor = Color.FromArgb(160, 165, 180),
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular)
                },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor         = Color.FromArgb(30, 33, 42),
                    ForeColor         = Color.FromArgb(200, 205, 220),
                    SelectionBackColor = Color.FromArgb(50, 100, 180),
                    SelectionForeColor = Color.White
                }
            };

            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id",          HeaderText = "ID",          FillWeight = 5,  Tag = "Id" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Query",        HeaderText = UiStrings.IsVietnamese ? "Từ tra" : "Query",         FillWeight = 20, Tag = "Query" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "WrongResult",  HeaderText = UiStrings.IsVietnamese ? "Kết quả sai" : "Wrong",     FillWeight = 25, Tag = "WrongResult" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Suggestion",   HeaderText = UiStrings.IsVietnamese ? "Đề xuất" : "Suggestion",    FillWeight = 25, Tag = "Suggestion" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Note",         HeaderText = UiStrings.IsVietnamese ? "Ghi chú" : "Note",          FillWeight = 15, Tag = "Note" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedAt",    HeaderText = UiStrings.IsVietnamese ? "Ngày" : "Date",             FillWeight = 15, Tag = "CreatedAt" });

            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 44,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor     = Color.FromArgb(22, 25, 34),
                Padding       = new Padding(0, 4, 0, 0)
            };

            var btnDelete      = MakeButton(UiStrings.IsVietnamese ? "Xóa" : "Delete", Color.FromArgb(160, 50, 50));
            var btnSheets      = MakeButton(UiStrings.IsVietnamese ? "→ Google Sheets" : "→ Google Sheets", UiPalette.NeutralSoft);
            var btnExportXlsx  = MakeButton(UiStrings.IsVietnamese ? "Xuất .xlsx" : "Export .xlsx", UiPalette.Accent);

            btnExportXlsx.Click += async (s, e) => await ExportXlsxAsync();
            btnSheets.Click     += async (s, e) => await CopyToSheetsAsync();
            btnDelete.Click     += async (s, e) => await DeleteSelectedAsync();

            btnPanel.Controls.Add(btnDelete);
            btnPanel.Controls.Add(btnSheets);
            btnPanel.Controls.Add(btnExportXlsx);

            page.Controls.Add(_grid);
            page.Controls.Add(btnPanel);
            return page;
        }

        private async Task RefreshListAsync()
        {
            _entries = await _repo.GetAllAsync();
            _grid.Rows.Clear();
            foreach (var e in _entries)
            {
                _grid.Rows.Add(
                    e.Id,
                    e.Query,
                    e.WrongResult,
                    e.Suggestion,
                    e.Note,
                    e.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
            }
        }

        private async Task DeleteSelectedAsync()
        {
            if (_grid.CurrentRow == null || _entries == null) return;
            var idx = _grid.CurrentRow.Index;
            if (idx < 0 || idx >= _entries.Count) return;

            var entry = _entries[idx];
            var confirm = MessageBox.Show(
                UiStrings.IsVietnamese ? $"Xóa góp ý về \"{entry.Query}\"?" : $"Delete feedback for \"{entry.Query}\"?",
                UiStrings.IsVietnamese ? "Xác nhận" : "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            await _repo.DeleteAsync(entry.Id);
            await RefreshListAsync();
        }

        private async Task ExportXlsxAsync()
        {
            if (_entries == null) await RefreshListAsync();

            using (var dlg = new SaveFileDialog
            {
                Title      = UiStrings.IsVietnamese ? "Lưu file Excel" : "Save Excel file",
                Filter     = "Excel (*.xlsx)|*.xlsx",
                FileName   = "feedback_ban_dich.xlsx",
                DefaultExt = "xlsx"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var pkg = new ExcelPackage())
                {
                    var ws = pkg.Workbook.Worksheets.Add("Feedback");
                    ws.Cells[1, 1].Value = "ID";
                    ws.Cells[1, 2].Value = UiStrings.IsVietnamese ? "Từ tra" : "Query";
                    ws.Cells[1, 3].Value = UiStrings.IsVietnamese ? "Kết quả sai" : "Wrong Result";
                    ws.Cells[1, 4].Value = UiStrings.IsVietnamese ? "Đề xuất" : "Suggestion";
                    ws.Cells[1, 5].Value = UiStrings.IsVietnamese ? "Ghi chú" : "Note";
                    ws.Cells[1, 6].Value = UiStrings.IsVietnamese ? "Ngày" : "Date";

                    int row = 2;
                    foreach (var e in _entries)
                    {
                        ws.Cells[row, 1].Value = e.Id;
                        ws.Cells[row, 2].Value = e.Query;
                        ws.Cells[row, 3].Value = e.WrongResult;
                        ws.Cells[row, 4].Value = e.Suggestion;
                        ws.Cells[row, 5].Value = e.Note;
                        ws.Cells[row, 6].Value = e.CreatedAt.ToString("dd/MM/yyyy HH:mm");
                        row++;
                    }

                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    pkg.SaveAs(new System.IO.FileInfo(dlg.FileName));
                }

                MessageBox.Show(
                    UiStrings.IsVietnamese ? "Đã xuất thành công." : "Exported successfully.",
                    UiStrings.IsVietnamese ? "Xong" : "Done",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async Task CopyToSheetsAsync()
        {
            if (_entries == null) await RefreshListAsync();
            if (_entries.Count == 0)
            {
                MessageBox.Show(
                    UiStrings.IsVietnamese ? "Chưa có dữ liệu để xuất." : "No data to export.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine(UiStrings.IsVietnamese
                ? "ID\tTừ tra\tKết quả sai\tĐề xuất\tGhi chú\tNgày"
                : "ID\tQuery\tWrong Result\tSuggestion\tNote\tDate");

            foreach (var e in _entries)
                sb.AppendLine($"{e.Id}\t{e.Query}\t{e.WrongResult}\t{e.Suggestion}\t{e.Note}\t{e.CreatedAt:dd/MM/yyyy HH:mm}");

            try { Clipboard.SetText(sb.ToString()); }
            catch { /* clipboard locked */ }

            var msg = UiStrings.IsVietnamese
                ? "Đã copy dữ liệu vào clipboard.\nMở sheet.new, paste vào cột đầu tiên."
                : "Data copied to clipboard.\nOpen sheet.new and paste into the first column.";

            var result = MessageBox.Show(
                msg + "\n\n" + (UiStrings.IsVietnamese ? "Mở sheet.new ngay?" : "Open sheet.new now?"),
                UiStrings.IsVietnamese ? "Copy xong" : "Copied",
                MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
                try { System.Diagnostics.Process.Start("https://sheet.new"); }
                catch { }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private static void AddRow(TableLayoutPanel layout, int row, string labelText, TextBox textBox)
        {
            var lbl = new Label
            {
                Text      = labelText,
                Dock      = DockStyle.Fill,
                ForeColor = Color.FromArgb(150, 155, 175),
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = new Font("Segoe UI", 9f)
            };
            layout.Controls.Add(lbl, 0, row);
            layout.Controls.Add(textBox, 1, row);
        }

        private static TextBox MakeTextBox(string text)
        {
            return new TextBox
            {
                Text       = text,
                Dock       = DockStyle.Fill,
                BackColor  = Color.FromArgb(38, 41, 52),
                ForeColor  = Color.FromArgb(210, 215, 230),
                BorderStyle = BorderStyle.FixedSingle,
                Font       = new Font("Segoe UI", 9.5f),
                Margin     = new Padding(0, 6, 8, 6)
            };
        }

        private static Button MakeButton(string text, Color backColor)
        {
            return new Button
            {
                Text      = text,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Height    = 30,
                AutoSize  = true,
                Padding   = new Padding(8, 0, 8, 0),
                Font      = new Font("Segoe UI", 9f),
                Margin    = new Padding(4, 2, 0, 2),
                Cursor    = Cursors.Hand
            };
        }
    }
}
