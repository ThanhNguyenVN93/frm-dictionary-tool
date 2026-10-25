using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using DictionaryTool.Core;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Repositories;

namespace DictionaryTool.UI
{
    /// <summary>
    /// Màn hình ôn tập giãn cách (#2): lần lượt hiện từng từ đã lưu đến hạn, người
    /// dùng tự nhớ → bấm "Hiện đáp án" → tự đánh giá Quên/Khó/Nhớ/Dễ. Lịch kế tiếp
    /// do SrsScheduler tính qua BookmarkRepository.ApplyReviewAsync.
    /// </summary>
    public class ReviewForm : Form
    {
        public event Action<string, Language> SearchRequested;

        private readonly IBookmarkRepository _bookmarks;
        private readonly ITrilingualLookupService _lookup;

        private List<BookmarkEntry> _due = new List<BookmarkEntry>();
        private int _index;
        private int _total;

        private Label _lblProgress;
        private Label _lblWord;
        private Label _lblAnswer;
        private Button _btnShow;
        private Panel _pnlGrades;
        private Button _btnOpen;
        private Button _btnClose;

        public ReviewForm(IBookmarkRepository bookmarks, ITrilingualLookupService lookup)
        {
            _bookmarks = bookmarks ?? throw new ArgumentNullException(nameof(bookmarks));
            _lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));

            BuildUi();
            Load += async (s, e) => await LoadDueAsync().ConfigureAwait(true);
        }

        private void BuildUi()
        {
            Text = UiStrings.ReviewTitle;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(560, 420);
            BackColor = UiPalette.PageBackground;
            ForeColor = UiPalette.TextPrimary;
            Font = new Font(UiFont.Family, 10F);

            _lblProgress = new Label
            {
                AutoSize = true,
                Location = new Point(20, 16),
                ForeColor = UiPalette.TextSecondary,
                Font = new Font(UiFont.Family, 9.5F)
            };
            Controls.Add(_lblProgress);

            _lblWord = new Label
            {
                AutoSize = false,
                Location = new Point(20, 54),
                Size = new Size(520, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = UiPalette.TextPrimary,
                Font = new Font(UiFont.Family, 24F, FontStyle.Bold)
            };
            Controls.Add(_lblWord);

            _lblAnswer = new Label
            {
                AutoSize = false,
                Location = new Point(20, 128),
                Size = new Size(520, 180),
                ForeColor = UiPalette.TextPrimary,
                Font = new Font(UiFont.Family, 11F),
                Visible = false
            };
            Controls.Add(_lblAnswer);

            _btnShow = MakeButton(UiStrings.ReviewShowAnswer, UiPalette.Accent, Color.White);
            _btnShow.Size = new Size(200, 44);
            _btnShow.Location = new Point((ClientSize.Width - 200) / 2, 330);
            _btnShow.Click += async (s, e) => await ShowAnswerAsync().ConfigureAwait(true);
            Controls.Add(_btnShow);

            _pnlGrades = new Panel
            {
                Location = new Point(20, 324),
                Size = new Size(520, 56),
                Visible = false
            };
            Controls.Add(_pnlGrades);

            AddGradeButton(0, ReviewGrade.Again, UiStrings.ReviewGradeAgain, UiPalette.ErrorText);
            AddGradeButton(1, ReviewGrade.Hard, UiStrings.ReviewGradeHard, UiPalette.NeutralSoft);
            AddGradeButton(2, ReviewGrade.Good, UiStrings.ReviewGradeGood, UiPalette.Accent);
            AddGradeButton(3, ReviewGrade.Easy, UiStrings.ReviewGradeEasy, Color.FromArgb(80, 170, 120));

            _btnOpen = MakeButton(UiStrings.ReviewOpenInApp, UiPalette.NeutralSoft, UiPalette.TextPrimary);
            _btnOpen.Size = new Size(160, 36);
            _btnOpen.Location = new Point(20, ClientSize.Height - 48);
            _btnOpen.Click += (s, e) =>
            {
                if (_index < _due.Count)
                {
                    var entry = _due[_index];
                    SearchRequested?.Invoke(entry.Query, entry.Language);
                }
                Close();
            };
            Controls.Add(_btnOpen);

            _btnClose = MakeButton(UiStrings.ReviewDone, UiPalette.NeutralSoft, UiPalette.TextPrimary);
            _btnClose.Size = new Size(120, 36);
            _btnClose.Location = new Point(ClientSize.Width - 140, ClientSize.Height - 48);
            _btnClose.Click += (s, e) => Close();
            Controls.Add(_btnClose);
        }

        private void AddGradeButton(int slot, ReviewGrade grade, string text, Color fill)
        {
            var width = (_pnlGrades.Width - 3 * 12) / 4;
            var btn = MakeButton(text, fill, Color.White);
            btn.Size = new Size(width, 48);
            btn.Location = new Point(slot * (width + 12), 0);
            btn.Click += async (s, e) => await GradeAsync(grade).ConfigureAwait(true);
            _pnlGrades.Controls.Add(btn);
        }

        private static Button MakeButton(string text, Color fill, Color fore)
        {
            return new Button
            {
                Text = text,
                FlatStyle = FlatStyle.Flat,
                BackColor = fill,
                ForeColor = fore,
                Font = new Font(UiFont.Family, 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                FlatAppearance = { BorderSize = 0 }
            };
        }

        private async Task LoadDueAsync()
        {
            try
            {
                _due = await _bookmarks.GetDueForReviewAsync(DateTime.UtcNow).ConfigureAwait(true);
            }
            catch (Exception)
            {
                _due = new List<BookmarkEntry>();
            }
            _total = _due.Count;
            _index = 0;
            ShowCurrent();
        }

        private void ShowCurrent()
        {
            _lblAnswer.Visible = false;
            _pnlGrades.Visible = false;

            if (_total == 0)
            {
                _lblWord.Text = string.Empty;
                _lblAnswer.Text = UiStrings.ReviewNoneDue;
                _lblAnswer.Visible = true;
                _btnShow.Visible = false;
                _btnOpen.Visible = false;
                _lblProgress.Text = string.Empty;
                return;
            }

            if (_index >= _total)
            {
                _lblWord.Text = string.Empty;
                _lblAnswer.Text = UiStrings.ReviewFinished;
                _lblAnswer.Visible = true;
                _btnShow.Visible = false;
                _btnOpen.Visible = false;
                _lblProgress.Text = UiStrings.ReviewProgress(_total, _total);
                return;
            }

            var entry = _due[_index];
            _lblProgress.Text = UiStrings.ReviewProgress(_index, _total);
            _lblWord.Text = entry.Query;
            _btnShow.Visible = true;
            _btnOpen.Visible = true;
        }

        private async Task ShowAnswerAsync()
        {
            if (_index >= _total) return;

            _btnShow.Visible = false;
            _lblAnswer.Text = UiStrings.ReviewLoading;
            _lblAnswer.Visible = true;

            var entry = _due[_index];
            string answer;
            try
            {
                var result = await _lookup.LookupAsync(entry.Query, entry.Language).ConfigureAwait(true);
                answer = BuildAnswer(result);
            }
            catch (Exception)
            {
                answer = "—";
            }

            _lblAnswer.Text = answer;
            _pnlGrades.Visible = true;
        }

        private static string BuildAnswer(TrilingualResult result)
        {
            var parts = new List<string>();

            if (result.VietnameseResult != null && !result.VietnameseResult.NotFound && result.VietnameseResult.Definitions.Count > 0)
            {
                var n = Math.Min(3, result.VietnameseResult.Definitions.Count);
                parts.Add("🇻🇳 " + string.Join("; ", result.VietnameseResult.Definitions.GetRange(0, n)));
            }

            var en = result.EnglishResult;
            if (en != null && !en.NotFound)
            {
                if (en.Meanings.Count > 0 && en.Meanings[0].Definitions.Count > 0)
                    parts.Add("🇬🇧 " + en.Meanings[0].Definitions[0].Definition);
                else if (en.LearnerMeanings.Count > 0 && en.LearnerMeanings[0].ShortDefinitions.Count > 0)
                    parts.Add("🇬🇧 " + en.LearnerMeanings[0].ShortDefinitions[0]);
            }

            if (result.ChineseResult != null && !result.ChineseResult.NotFound)
            {
                var zh = result.ChineseResult;
                parts.Add("🇨🇳 " + zh.Simplified + "  [" + zh.Pinyin + "]");
            }

            return parts.Count > 0 ? string.Join("\n\n", parts) : "—";
        }

        private async Task GradeAsync(ReviewGrade grade)
        {
            if (_index >= _total) return;

            var entry = _due[_index];
            try
            {
                await _bookmarks.ApplyReviewAsync(entry.Query, entry.Language, grade, DateTime.UtcNow).ConfigureAwait(true);
            }
            catch (Exception)
            {
                // Không để lỗi ghi lịch làm kẹt phiên ôn.
            }

            _index++;
            ShowCurrent();
        }
    }
}
