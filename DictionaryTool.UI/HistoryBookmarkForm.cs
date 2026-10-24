using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using DictionaryTool.Core.Export;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Repositories;
using Guna.UI2.WinForms;

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

            var lblHistoryHeader = new Label
            {
                Text = "🕘 LỊCH SỬ TRA TỪ",
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = UiPalette.TextSecondary,
                Location = new Point(20, 16)
            };

            var pnlHistory = new FlickerFreePanel
            {
                Location = new Point(20, 46),
                Size = new Size(320, 370),
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
                Location = new Point(360, 16)
            };

            var pnlBookmark = new FlickerFreePanel
            {
                Location = new Point(360, 46),
                Size = new Size(320, 370),
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
                Text = "📤 Xuất XLS",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BorderRadius = 14,
                FillColor = UiPalette.Accent,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Location = new Point(20, 428),
                Size = new Size(130, 40)
            };
            btnExport.HoverState.FillColor = UiPalette.AccentHover;
            btnExport.Click += async (s, e) => await ExportBookmarksAsync().ConfigureAwait(true);

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

            Controls.Add(lblHistoryHeader);
            Controls.Add(pnlHistory);
            Controls.Add(lblBookmarkHeader);
            Controls.Add(pnlBookmark);
            Controls.Add(btnExport);
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
            _historyList.Controls.Clear();

            var items = await _historyRepository.GetRecentAsync().ConfigureAwait(true);
            if (items.Count == 0)
            {
                _historyList.Controls.Add(CreateInfoLabel("Chưa có lượt tra nào."));
                return;
            }

            foreach (var item in items)
            {
                var query = item.Query;
                var language = item.Language;
                _historyList.Controls.Add(CreateRow(query, language, onClick: () => RaiseSearchRequested(query, language)));
            }
        }

        private async Task LoadBookmarksAsync()
        {
            _bookmarkList.Controls.Clear();

            var items = await _bookmarkRepository.GetAllAsync().ConfigureAwait(true);
            if (items.Count == 0)
            {
                _bookmarkList.Controls.Add(CreateInfoLabel("Chưa có bookmark nào."));
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
