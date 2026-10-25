using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DictionaryTool.Core.Export;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Repositories;
using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DictionaryTool.UI
{
    /// <summary>
    /// Dialog hiện lịch sử tra từ + danh sách bookmark, cho phép tra lại 1 click,
    /// xoá bookmark, và xuất bookmark ra .xlsx (spec Phase 3). Không phải Designer
    /// form (không có file .Designer.cs riêng) — toàn bộ layout dựng bằng code
    /// trong constructor, theo đúng style các panel động ở MainForm.
    /// </summary>
    public class HistoryBookmarkForm : Form
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly IBookmarkRepository _bookmarkRepository;
        private readonly IBookmarkXlsxExporter _bookmarkXlsxExporter;

        private readonly FlickerFreeFlowLayoutPanel _historyList;
        private readonly FlickerFreeFlowLayoutPanel _bookmarkList;
        private readonly Guna2TextBox _txtFilter;

        // Dữ liệu đã nạp, giữ lại để lọc tại chỗ (không query DB mỗi lần gõ).
        private List<HistoryEntry> _allHistory = new List<HistoryEntry>();
        private List<BookmarkEntry> _allBookmarks = new List<BookmarkEntry>();

        /// <summary>Raised khi người dùng chọn 1 dòng để tra lại — MainForm nghe sự kiện này.</summary>
        public event Action<string, Language> SearchRequested;

        public HistoryBookmarkForm(
            IHistoryRepository historyRepository,
            IBookmarkRepository bookmarkRepository,
            IBookmarkXlsxExporter bookmarkXlsxExporter)
        {
            _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
            _bookmarkRepository = bookmarkRepository ?? throw new ArgumentNullException(nameof(bookmarkRepository));
            _bookmarkXlsxExporter = bookmarkXlsxExporter ?? throw new ArgumentNullException(nameof(bookmarkXlsxExporter));

            Text = "Lịch sử & Bookmark";
            BackColor = UiPalette.PageBackground;
            ClientSize = new Size(700, 480);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            // --- Ô lọc nhanh: lọc đồng thời lịch sử + bookmark theo từ khoá ---
            _txtFilter = new Guna2TextBox
            {
                Location = new Point(20, 14),
                Size = new Size(660, 34),
                BorderRadius = 10,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "🔎 Lọc theo từ…",
                FillColor = UiPalette.CardBackground,
                ForeColor = UiPalette.TextPrimary,
                BorderColor = UiPalette.BorderSoft,
                PlaceholderForeColor = UiPalette.TextSecondary
            };
            _txtFilter.TextChanged += (s, e) => ApplyFilter();

            var lblHistoryHeader = new Label
            {
                Text = "🕘 LỊCH SỬ TRA TỪ",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = UiPalette.TextSecondary,
                Location = new Point(20, 58)
            };

            var pnlHistory = new FlickerFreePanel
            {
                Location = new Point(20, 86),
                Size = new Size(320, 330),
                BorderRadius = 14,
                FillColor = UiPalette.CardBackground,
                AutoScroll = true,
                Padding = new Padding(12)
            };
            pnlHistory.ShadowDecoration.Enabled = false;

            _historyList = new FlickerFreeFlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top
            };
            pnlHistory.Controls.Add(_historyList);

            var lblBookmarkHeader = new Label
            {
                Text = "⭐ BOOKMARK",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = UiPalette.TextSecondary,
                Location = new Point(360, 58)
            };

            var pnlBookmark = new FlickerFreePanel
            {
                Location = new Point(360, 86),
                Size = new Size(320, 330),
                BorderRadius = 14,
                FillColor = UiPalette.CardBackground,
                AutoScroll = true,
                Padding = new Padding(12)
            };
            pnlBookmark.ShadowDecoration.Enabled = false;

            _bookmarkList = new FlickerFreeFlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Top
            };
            pnlBookmark.Controls.Add(_bookmarkList);

            var btnExport = new Guna2Button
            {
                Text = "📤 XLS",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BorderRadius = 14,
                FillColor = UiPalette.NeutralSoft,
                ForeColor = UiPalette.TextPrimary,
                Cursor = Cursors.Hand,
                Location = new Point(20, 428),
                Size = new Size(100, 40)
            };
            btnExport.HoverState.FillColor = UiPalette.AccentSoftHover;
            btnExport.Click += async (s, e) => await ExportBookmarksAsync().ConfigureAwait(true);

            var btnBackup = new Guna2Button
            {
                Text = "💾 Sao lưu",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BorderRadius = 14,
                FillColor = UiPalette.Accent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Location = new Point(128, 428),
                Size = new Size(130, 40)
            };
            btnBackup.HoverState.FillColor = UiPalette.AccentHover;
            btnBackup.Click += async (s, e) => await BackupAsync().ConfigureAwait(true);

            var btnRestore = new Guna2Button
            {
                Text = "📥 Phục hồi",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BorderRadius = 14,
                FillColor = UiPalette.NeutralSoft,
                ForeColor = UiPalette.TextPrimary,
                Cursor = Cursors.Hand,
                Location = new Point(266, 428),
                Size = new Size(130, 40)
            };
            btnRestore.HoverState.FillColor = UiPalette.AccentSoftHover;
            btnRestore.Click += async (s, e) => await RestoreAsync().ConfigureAwait(true);

            var btnClose = new Guna2Button
            {
                Text = "Đóng",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BorderRadius = 14,
                FillColor = UiPalette.NeutralSoft,
                ForeColor = UiPalette.TextSecondary,
                Cursor = Cursors.Hand,
                Location = new Point(590, 428),
                Size = new Size(90, 40)
            };
            btnClose.HoverState.FillColor = UiPalette.NeutralSoft;
            btnClose.Click += (s, e) => Close();

            Controls.Add(_txtFilter);
            Controls.Add(lblHistoryHeader);
            Controls.Add(pnlHistory);
            Controls.Add(lblBookmarkHeader);
            Controls.Add(pnlBookmark);
            Controls.Add(btnExport);
            Controls.Add(btnBackup);
            Controls.Add(btnRestore);
            Controls.Add(btnClose);

            Load += async (s, e) => await LoadDataAsync().ConfigureAwait(true);
        }

        private async Task LoadDataAsync()
        {
            await LoadHistoryAsync().ConfigureAwait(true);
            await LoadBookmarksAsync().ConfigureAwait(true);
        }

        private async Task LoadHistoryAsync()
        {
            _allHistory = await _historyRepository.GetRecentAsync().ConfigureAwait(true);
            RenderHistory();
        }

        private async Task LoadBookmarksAsync()
        {
            _allBookmarks = await _bookmarkRepository.GetAllAsync().ConfigureAwait(true);
            RenderBookmarks();
        }

        private void ApplyFilter()
        {
            RenderHistory();
            RenderBookmarks();
        }

        private bool Matches(string query)
        {
            var filter = _txtFilter.Text?.Trim();
            return string.IsNullOrEmpty(filter)
                || (query ?? string.Empty).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void RenderHistory()
        {
            _historyList.Controls.Clear();
            var items = _allHistory.Where(i => Matches(i.Query)).ToList();
            if (items.Count == 0)
            {
                _historyList.Controls.Add(CreateInfoLabel(
                    _allHistory.Count == 0 ? "Chưa có lượt tra nào." : "Không có mục khớp bộ lọc."));
                return;
            }

            foreach (var item in items)
            {
                var query = item.Query;
                var language = item.Language;
                _historyList.Controls.Add(CreateRow(query, language, onClick: () => RaiseSearchRequested(query, language)));
            }
        }

        private void RenderBookmarks()
        {
            _bookmarkList.Controls.Clear();
            var items = _allBookmarks.Where(i => Matches(i.Query)).ToList();
            if (items.Count == 0)
            {
                _bookmarkList.Controls.Add(CreateInfoLabel(
                    _allBookmarks.Count == 0 ? "Chưa có bookmark nào." : "Không có mục khớp bộ lọc."));
                return;
            }

            foreach (var item in items)
            {
                var query = item.Query;
                var language = item.Language;
                _bookmarkList.Controls.Add(CreateRow(
                    query,
                    language,
                    onClick: () => RaiseSearchRequested(query, language),
                    onRemove: async () =>
                    {
                        await _bookmarkRepository.RemoveAsync(query, language).ConfigureAwait(true);
                        await LoadBookmarksAsync().ConfigureAwait(true);
                    }));
            }
        }

        private void RaiseSearchRequested(string query, Language language)
        {
            SearchRequested?.Invoke(query, language);
            Close();
        }

        private async Task ExportBookmarksAsync()
        {
            var bookmarks = await _bookmarkRepository.GetAllAsync().ConfigureAwait(true);

            using (var dialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = "bookmarks.xlsx"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    var bytes = _bookmarkXlsxExporter.Export(bookmarks);
                    File.WriteAllBytes(dialog.FileName, bytes);
                    MessageBox.Show(this, "Đã xuất " + bookmarks.Count + " bookmark ra file.", "Xuất XLS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Không thể ghi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ---- Sao lưu / phục hồi toàn bộ bookmark + tiến độ ôn tập (SRS) ra JSON ----

        private sealed class BackupFile
        {
            public int Version { get; set; } = 1;
            public string App { get; set; } = "Dictionary Tool";
            public DateTime ExportedAtUtc { get; set; }
            public List<BookmarkEntry> Bookmarks { get; set; } = new List<BookmarkEntry>();
        }

        private static readonly JsonSerializerSettings BackupJsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = { new StringEnumConverter() } // Language lưu dạng chữ cho dễ đọc / bền
        };

        private async Task BackupAsync()
        {
            var bookmarks = await _bookmarkRepository.GetAllAsync().ConfigureAwait(true);
            if (bookmarks.Count == 0)
            {
                MessageBox.Show(this, "Chưa có bookmark nào để sao lưu.", "Sao lưu",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog
            {
                Filter = "Dictionary Tool backup (*.json)|*.json",
                FileName = "dictionary-tool-backup-" + DateTime.Now.ToString("yyyyMMdd") + ".json"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    var backup = new BackupFile { ExportedAtUtc = DateTime.UtcNow, Bookmarks = bookmarks };
                    var json = JsonConvert.SerializeObject(backup, BackupJsonSettings);
                    File.WriteAllText(dialog.FileName, json, new System.Text.UTF8Encoding(false));
                    MessageBox.Show(this, "Đã sao lưu " + bookmarks.Count + " bookmark (kèm tiến độ ôn tập).",
                        "Sao lưu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Không thể ghi file: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task RestoreAsync()
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = "Dictionary Tool backup (*.json)|*.json|Tất cả file (*.*)|*.*"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                BackupFile backup;
                try
                {
                    var json = File.ReadAllText(dialog.FileName);
                    backup = JsonConvert.DeserializeObject<BackupFile>(json, BackupJsonSettings);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "File sao lưu không hợp lệ: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (backup?.Bookmarks == null || backup.Bookmarks.Count == 0)
                {
                    MessageBox.Show(this, "File không chứa bookmark nào.", "Phục hồi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Yes = ghi đè tiến độ của từ trùng; No = chỉ thêm từ mới; Cancel = thôi.
                var choice = MessageBox.Show(this,
                    "Tìm thấy " + backup.Bookmarks.Count + " bookmark trong file.\n\n" +
                    "• Yes: ghi đè tiến độ ôn tập của từ đã có bằng dữ liệu trong file.\n" +
                    "• No: chỉ thêm từ mới, giữ nguyên tiến độ hiện tại.\n" +
                    "• Cancel: không làm gì.",
                    "Phục hồi bookmark", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (choice == DialogResult.Cancel)
                {
                    return;
                }

                try
                {
                    var affected = await _bookmarkRepository
                        .ImportAsync(backup.Bookmarks, overwrite: choice == DialogResult.Yes)
                        .ConfigureAwait(true);
                    await LoadBookmarksAsync().ConfigureAwait(true);
                    MessageBox.Show(this, "Đã phục hồi " + affected + " bookmark.", "Phục hồi",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Không thể phục hồi: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private Control CreateRow(string query, Language language, Action onClick, Func<Task> onRemove = null)
        {
            var row = new FlickerFreeFlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Width = 280,
                Margin = new Padding(0, 2, 0, 2)
            };

            var textButton = new Guna2Button
            {
                Text = query + "  ·  " + LanguageLabel(language),
                TextAlign = HorizontalAlignment.Left,
                AutoSize = true,
                Padding = new Padding(10, 6, 10, 6),
                Font = new Font("Segoe UI", 10F),
                FillColor = UiPalette.NeutralSoft,
                ForeColor = UiPalette.TextPrimary,
                BorderRadius = 10,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 6, 0)
            };
            textButton.HoverState.FillColor = UiPalette.AccentSoftHover;
            textButton.ShadowDecoration.Enabled = false;
            textButton.Click += (s, e) => onClick();
            row.Controls.Add(textButton);

            if (onRemove != null)
            {
                var removeButton = new Guna2Button
                {
                    Text = "✕",
                    AutoSize = true,
                    Padding = new Padding(8, 6, 8, 6),
                    Font = new Font("Segoe UI", 9F),
                    FillColor = UiPalette.ErrorSoft,
                    ForeColor = UiPalette.ErrorText,
                    BorderRadius = 10,
                    Cursor = Cursors.Hand
                };
                removeButton.HoverState.FillColor = UiPalette.ErrorSoft;
                removeButton.ShadowDecoration.Enabled = false;
                removeButton.Click += async (s, e) => await onRemove().ConfigureAwait(true);
                row.Controls.Add(removeButton);
            }

            return row;
        }

        private static string LanguageLabel(Language language)
        {
            switch (language)
            {
                case Language.Vietnamese: return "VI";
                case Language.Chinese: return "ZH";
                default: return "EN";
            }
        }

        private static Label CreateInfoLabel(string text)
        {
            return new Label
            {
                AutoSize = true,
                Text = text,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                ForeColor = UiPalette.TextSecondary
            };
        }
    }
}
