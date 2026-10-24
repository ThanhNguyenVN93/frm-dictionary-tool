using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DictionaryTool.Core;
using DictionaryTool.Core.Cache;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Export;
using DictionaryTool.Core.Repositories;
using DictionaryTool.UI.Interop;
using Guna.UI2.WinForms;
using NAudio.Wave;

namespace DictionaryTool.UI
{
    public partial class MainForm : Form
    {
        private readonly ITrilingualLookupService _trilingualLookupService;
        private readonly IAudioCacheRepository _audioCacheRepository;
        private readonly IApiUsageTracker _apiUsageTracker;
        private readonly ICacheRepository _cacheRepository;
        private readonly ICcCedictRepository _cedictRepository;
        private readonly IStardictRepository _stardictRepository;
        private readonly ITatoebaRepository _tatoebaRepository;
        private readonly IHistoryRepository _historyRepository;
        private readonly IBookmarkRepository _bookmarkRepository;
        private readonly IBookmarkXlsxExporter _bookmarkXlsxExporter;

        private CancellationTokenSource _searchCts;
        private string _lastSearchedWord;
        private string _currentResultQuery;
        private Language _currentResultLanguage;

        private const int HotKeyId = 1;
        private uint _hotkeyModifiers = NativeMethods.MOD_CONTROL | NativeMethods.MOD_SHIFT;
        private uint _hotkeyVk = (uint)Keys.D;
        private bool _clipboardWatchEnabled;
        private string _lastClipboardText = string.Empty;
        private bool _isMiniMode;
        private bool _isLoading;
        private Language? _lockedLanguage;

        // Mini mode KHÔNG bỏ cột VI/ZH (lần đầu làm vậy user phản hồi thiếu) — chỉ
        // thu nhỏ cả 3 cột lại. Toạ độ Full lấy đúng từ InitializeComponent, dùng để
        // khôi phục khi thoát Mini mode; toạ độ 3 cột + nội dung bên trong (pnlPhonetics,
        // pnlDefinitions...) đều tính lại theo ColumnWidth/ContentWidth tương ứng.
        private const int ColumnStartX = 20;
        private const int ColumnStartY = 90;

        private const int FullColumnWidth = 308;
        private const int FullColumnHeight = 380;
        private const int FullColumnGap = 16;
        private const int FullContentWidth = 268;

        private const int MiniColumnWidth = 200;
        private const int MiniColumnHeight = 300;
        private const int MiniColumnGap = 12;
        private const int MiniContentWidth = 160;

        private static readonly Point FullSearchBoxLocation = new Point(24, 24);
        private static readonly Size FullSearchBoxSize = new Size(360, 46);
        private static readonly Point FullCircleLoadingLocation = new Point(483, 24);
        private static readonly Point FullRetryLocation = new Point(456, 24);
        private static readonly Size FullRetrySize = new Size(100, 46);

        private static readonly Point MiniSearchBoxLocation = new Point(20, 24);
        private static readonly Size MiniSearchBoxSize = new Size(200, 46);
        private static readonly Point MiniCircleLoadingLocation = new Point(226, 19);
        private static readonly Point MiniRetryLocation = new Point(226, 24);
        private static readonly Size MiniRetrySize = new Size(90, 46);

        private static readonly Point FullHotkeyConfigLocation = new Point(660, 24);
        private static readonly Point FullMiniModeLocation = new Point(714, 24);

        // btnInfo kết thúc ở x=976 + lề phải 20 = 996; khớp đúng 3 cột 308px.
        private const int FullToolbarMinWidth = 996;

        // Constructor không tham số: chỉ để WinForms Designer tạo instance xem
        // trước layout — Designer bắt buộc phải có ctor này, không thì crash khi
        // mở view Design. Không dùng constructor này khi chạy thật.
        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(
            ITrilingualLookupService trilingualLookupService,
            IAudioCacheRepository audioCacheRepository,
            IApiUsageTracker apiUsageTracker,
            ICacheRepository cacheRepository,
            ICcCedictRepository cedictRepository,
            IStardictRepository stardictRepository,
            ITatoebaRepository tatoebaRepository,
            IHistoryRepository historyRepository,
            IBookmarkRepository bookmarkRepository,
            IBookmarkXlsxExporter bookmarkXlsxExporter) : this()
        {
            _trilingualLookupService = trilingualLookupService ?? throw new ArgumentNullException(nameof(trilingualLookupService));
            _audioCacheRepository = audioCacheRepository ?? throw new ArgumentNullException(nameof(audioCacheRepository));
            _apiUsageTracker = apiUsageTracker ?? throw new ArgumentNullException(nameof(apiUsageTracker));
            _cacheRepository = cacheRepository ?? throw new ArgumentNullException(nameof(cacheRepository));
            _cedictRepository = cedictRepository ?? throw new ArgumentNullException(nameof(cedictRepository));
            _stardictRepository = stardictRepository ?? throw new ArgumentNullException(nameof(stardictRepository));
            _tatoebaRepository = tatoebaRepository ?? throw new ArgumentNullException(nameof(tatoebaRepository));
            _historyRepository = historyRepository ?? throw new ArgumentNullException(nameof(historyRepository));
            _bookmarkRepository = bookmarkRepository ?? throw new ArgumentNullException(nameof(bookmarkRepository));
            _bookmarkXlsxExporter = bookmarkXlsxExporter ?? throw new ArgumentNullException(nameof(bookmarkXlsxExporter));

            LoadSettings();
            ResetToIdleState();
            ApplyClipboardButtonState();
            ApplyUiLanguage();

            _ = LoadAutoCompleteWordsAsync();
            _ = InitializeOfflineDataAsync();
        }

        private async Task InitializeOfflineDataAsync()
        {
            lblImportStatus.Visible = true;
            var progress = new Progress<string>(message => lblImportStatus.Text = message);

            try
            {
                await _cedictRepository.EnsureImportedAsync(progress).ConfigureAwait(true);
                await _stardictRepository.EnsureImportedAsync(progress).ConfigureAwait(true);
                await _tatoebaRepository.EnsureImportedAsync(progress).ConfigureAwait(true);
            }
            catch (Exception)
            {
                // Lỗi nhập dữ liệu offline (vd thiếu file seed) không được làm crash app —
                // tra từ vẫn hoạt động, chỉ riêng VI/ZH/câu ví dụ sẽ rỗng.
            }
            finally
            {
                lblImportStatus.Visible = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LoadHotkeyFromConfig();
            RegisterCurrentHotKey();

            NativeMethods.AddClipboardFormatListener(Handle);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnregisterCurrentHotKey();
            NativeMethods.RemoveClipboardFormatListener(Handle);
            base.OnFormClosed(e);
        }

        private void LoadHotkeyFromConfig()
        {
            try
            {
                var modsStr = System.Configuration.ConfigurationManager.AppSettings["Hotkey:Modifiers"];
                var keyStr = System.Configuration.ConfigurationManager.AppSettings["Hotkey:Key"];

                if (uint.TryParse(modsStr, out var mods) && mods != 0)
                    _hotkeyModifiers = mods;
                if (uint.TryParse(keyStr, out var vk) && vk != 0)
                    _hotkeyVk = vk;
            }
            catch (Exception)
            {
                // App.config không đọc được — giữ giá trị mặc định Ctrl+Shift+D.
            }
        }

        private void RegisterCurrentHotKey()
        {
            if (!NativeMethods.RegisterHotKey(Handle, HotKeyId, _hotkeyModifiers, _hotkeyVk))
            {
                var combo = HotkeyConfigDialog.FormatCombo(_hotkeyModifiers, _hotkeyVk);
                MessageBox.Show(
                    UiStrings.ErrHotkeyBody(combo),
                    UiStrings.ErrHotkeyTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void UnregisterCurrentHotKey()
        {
            NativeMethods.UnregisterHotKey(Handle, HotKeyId);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_HOTKEY && m.WParam.ToInt32() == HotKeyId)
            {
                ShowAndFocusSearch();
            }
            else if (m.Msg == NativeMethods.WM_CLIPBOARDUPDATE && _clipboardWatchEnabled)
            {
                HandleClipboardChanged();
            }

            base.WndProc(ref m);
        }

        private void ShowAndFocusSearch()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }

            Show();
            NativeMethods.ShowWindow(Handle, NativeMethods.SW_RESTORE);
            NativeMethods.SetForegroundWindow(Handle);
            Activate();

            txtSearch.Focus();
            txtSearch.SelectAll();
        }

        private void HandleClipboardChanged()
        {
            string text;
            try
            {
                text = Clipboard.ContainsText() ? Clipboard.GetText() : null;
            }
            catch (Exception)
            {
                // Clipboard có thể bị khoá tạm thời bởi app khác — bỏ qua, không crash.
                return;
            }

            if (!ClipboardWatchFilter.ShouldTrigger(_lastClipboardText, text))
            {
                return;
            }

            _lastClipboardText = text.Trim();
            txtSearch.Text = _lastClipboardText;
            debounceTimer.Stop();
            _ = SearchAsync(_lastClipboardText);
        }

        private void btnClipboardToggle_Click(object sender, EventArgs e)
        {
            _clipboardWatchEnabled = !_clipboardWatchEnabled;

            btnClipboardToggle.Text = _clipboardWatchEnabled ? UiStrings.BtnClipboardOn : UiStrings.BtnClipboardOff;
            btnClipboardToggle.FillColor = _clipboardWatchEnabled ? UiPalette.Accent : UiPalette.NeutralSoft;
            btnClipboardToggle.ForeColor = _clipboardWatchEnabled ? Color.White : UiPalette.TextSecondary;
            SaveSetting("UI:ClipboardEnabled", _clipboardWatchEnabled.ToString().ToLower());
        }

        private void btnHotkeyConfig_Click(object sender, EventArgs e)
        {
            using (var dialog = new HotkeyConfigDialog(_hotkeyModifiers, _hotkeyVk))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                UnregisterCurrentHotKey();
                _hotkeyModifiers = dialog.NewModifiers;
                _hotkeyVk = dialog.NewKey;
                RegisterCurrentHotKey();

                try
                {
                    var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(
                        System.Configuration.ConfigurationUserLevel.None);
                    config.AppSettings.Settings["Hotkey:Modifiers"].Value = _hotkeyModifiers.ToString();
                    config.AppSettings.Settings["Hotkey:Key"].Value = _hotkeyVk.ToString();
                    config.Save(System.Configuration.ConfigurationSaveMode.Modified);
                    System.Configuration.ConfigurationManager.RefreshSection("appSettings");
                }
                catch (Exception)
                {
                    // Không ghi được config — hotkey vẫn hoạt động phiên này nhưng
                    // sẽ reset về cũ khi khởi động lại.
                }
            }
        }

        private void btnMiniMode_Click(object sender, EventArgs e)
        {
            _isMiniMode = !_isMiniMode;
            ApplyWindowMode();
        }

        private void ApplyWindowMode()
        {
            var columnWidth = _isMiniMode ? MiniColumnWidth : FullColumnWidth;
            var columnHeight = _isMiniMode ? MiniColumnHeight : FullColumnHeight;
            var columnGap = _isMiniMode ? MiniColumnGap : FullColumnGap;
            var contentWidth = _isMiniMode ? MiniContentWidth : FullContentWidth;

            var enLocation = new Point(ColumnStartX, ColumnStartY);
            var viLocation = new Point(enLocation.X + columnWidth + columnGap, ColumnStartY);
            var zhLocation = new Point(viLocation.X + columnWidth + columnGap, ColumnStartY);
            var columnSize = new Size(columnWidth, columnHeight);

            pnlColumnEn.Location = enLocation;
            pnlColumnEn.Size = columnSize;
            pnlColumnEn.AutoScrollPosition = Point.Empty;
            pnlColumnVi.Location = viLocation;
            pnlColumnVi.Size = columnSize;
            pnlColumnVi.AutoScrollPosition = Point.Empty;
            pnlColumnZh.Location = zhLocation;
            pnlColumnZh.Size = columnSize;
            pnlColumnZh.AutoScrollPosition = Point.Empty;

            // Nội dung bên trong mỗi cột (FlowLayoutPanel/Separator) phải co theo
            // ContentWidth tương ứng, không thì bị tràn ra ngoài cột ở Mini mode.
            pnlPhonetics.Width = contentWidth;
            sepAfterPhonetics.Width = contentWidth;
            pnlDefinitions.Width = contentWidth;
            sepAfterDefinitions.Width = contentWidth;
            pnlLearnerDefs.Width = contentWidth;
            lblWord.MaximumSize = new Size(contentWidth, 0);
            pnlViDefinitions.Width = contentWidth;
            lblViWord.MaximumSize = new Size(contentWidth, 0);
            pnlZhDefinitions.Width = contentWidth;
            lblZhWord.MaximumSize = new Size(contentWidth, 0);

            var combinedWidth = zhLocation.X + columnWidth - ColumnStartX;
            var examplesLocation = new Point(ColumnStartX, ColumnStartY + columnHeight + 12);
            var examplesHeight = _isMiniMode ? 110 : 130;
            pnlExamples.Location = examplesLocation;
            pnlExamples.Size = new Size(combinedWidth, examplesHeight);

            pnlError.Location = enLocation;
            pnlError.Size = new Size(combinedWidth, columnHeight + 12 + examplesHeight);

            if (!pnlError.Visible)
            {
                pnlColumnVi.Visible = true;
                pnlColumnZh.Visible = true;
                pnlExamples.Visible = true;
            }

            lblUsage.Location = new Point(20, examplesLocation.Y + examplesHeight + 14);

            // Chiều rộng cửa sổ phải đủ cho cả vùng kết quả LẪN thanh công cụ (ở Full
            // mode có 7 nút, cần rộng hơn vùng kết quả) — lấy giá trị lớn hơn, không
            // chỉ tính theo combinedWidth như trước (sẽ cắt mất nút bookmark/history).
            var contentBasedWidth = ColumnStartX + combinedWidth + 20;
            var toolbarMinWidth = _isMiniMode ? 0 : FullToolbarMinWidth;
            ClientSize = new Size(Math.Max(contentBasedWidth, toolbarMinWidth), lblUsage.Location.Y + 30);

            // Thanh công cụ: ở Mini mode ẩn bớt chip/nút search/clipboard/bookmark/history
            // (vẫn gõ + debounce để tra được), dồn ô search hẹp lại, và LUÔN đặt nút
            // Mini/Đầy đủ theo mép phải cửa sổ hiện tại — tránh lặp lại lỗi nút bị trôi
            // ra ngoài vùng nhìn thấy khi cửa sổ thu nhỏ.
            txtSearch.Location = _isMiniMode ? MiniSearchBoxLocation : FullSearchBoxLocation;
            txtSearch.Size = _isMiniMode ? MiniSearchBoxSize : FullSearchBoxSize;

            pnlLanguageChip.Visible = !_isMiniMode;
            btnSearch.Visible = !_isMiniMode && !_isLoading;
            btnClipboardToggle.Visible = !_isMiniMode;
            btnBookmarkToggle.Visible = !_isMiniMode;
            btnHistory.Visible = !_isMiniMode;
            btnHotkeyConfig.Visible = !_isMiniMode;
            btnInfo.Visible = !_isMiniMode;

            btnMiniMode.Text = _isMiniMode ? UiStrings.BtnMiniModeOn : UiStrings.BtnMiniModeOff;
            // btnHotkeyConfig ẩn ở Mini mode nên không cần theo mép phải; giữ đúng vị trí
            // cố định của Full mode để không đè lên btnClipboardToggle/btnBookmarkToggle
            // khi bật lại Full mode (xem InitializeComponent).
            btnHotkeyConfig.Location = FullHotkeyConfigLocation;
            btnMiniMode.Location = _isMiniMode
                ? new Point(ClientSize.Width - 20 - btnMiniMode.Width, 24)
                : FullMiniModeLocation;

            circleLoading.Location = _isMiniMode ? MiniCircleLoadingLocation : FullCircleLoadingLocation;
            btnRetry.Location = _isMiniMode ? MiniRetryLocation : FullRetryLocation;
            btnRetry.Size = _isMiniMode ? MiniRetrySize : FullRetrySize;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Debounce: mỗi keystroke reset lại timer, chỉ search sau khi người dùng
            // ngừng gõ ~450ms (spec 3.2).
            debounceTimer.Stop();
            debounceTimer.Start();
        }

        private async void debounceTimer_Tick(object sender, EventArgs e)
        {
            debounceTimer.Stop();
            await SearchAsync(txtSearch.Text);
        }

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            debounceTimer.Stop();
            await SearchAsync(txtSearch.Text);
        }

        private async void btnRetry_Click(object sender, EventArgs e)
        {
            await SearchAsync(_lastSearchedWord);
        }

        private async Task SearchFromHistoryAsync(string query, Language language)
        {
            var cached = await _historyRepository.GetTrilingualResultAsync(query, language).ConfigureAwait(true);
            if (cached != null)
            {
                HideLoading();
                UpdateLanguageChip(cached.DetectedLanguage);
                RenderResult(cached);
                return;
            }

            await SearchAsync(query).ConfigureAwait(true);
        }

        private async Task SearchAsync(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return;
            }

            _lastSearchedWord = word;

            // Huỷ tìm kiếm trước đó (nếu còn đang chạy) để tránh race condition khi
            // người dùng gõ nhanh — kết quả cũ trả về sau sẽ bị bỏ qua (spec 3.3).
            _searchCts?.Cancel();
            _searchCts?.Dispose();
            var cts = new CancellationTokenSource();
            _searchCts = cts;

            ShowLoading();
            if (_lockedLanguage != null)
                UpdateLanguageChipLockState();
            else
                UpdateLanguageChip(LanguageDetector.Detect(word));

            TrilingualResult result;
            try
            {
                result = await _trilingualLookupService.LookupAsync(word, _lockedLanguage, cts.Token).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (cts.IsCancellationRequested || !ReferenceEquals(cts, _searchCts))
            {
                // Đã có tìm kiếm mới hơn bắt đầu trong lúc chờ -> bỏ kết quả này.
                return;
            }

            HideLoading();
            UpdateLanguageChip(result.DetectedLanguage);
            _ = RefreshUsageLabelAsync();

            RenderResult(result);
        }

        private void UpdateLanguageChip(Language language)
        {
            if (_lockedLanguage != null) return; // lock mode overrides auto-detect display
            switch (language)
            {
                case Language.Vietnamese: lblLanguageChip.Text = "VI"; break;
                case Language.Chinese: lblLanguageChip.Text = "ZH"; break;
                default: lblLanguageChip.Text = "EN"; break;
            }
            pnlLanguageChip.FillColor = UiPalette.AccentSoft;
            lblLanguageChip.ForeColor = UiPalette.Accent;
        }

        private void UpdateLanguageChipLockState()
        {
            if (_lockedLanguage == null)
            {
                pnlLanguageChip.FillColor = UiPalette.AccentSoft;
                lblLanguageChip.ForeColor = UiPalette.Accent;
                lblLanguageChip.Text = "EN";
                toolTipToolbar.SetToolTip(pnlLanguageChip, UiStrings.TipChipAuto);
                toolTipToolbar.SetToolTip(lblLanguageChip, UiStrings.TipChipAuto);
                return;
            }
            string langLabel;
            string nextLang;
            switch (_lockedLanguage)
            {
                case Language.English:
                    lblLanguageChip.Text = "EN 🔒";
                    langLabel = UiStrings.LockEn;
                    nextLang = "VI";
                    break;
                case Language.Vietnamese:
                    lblLanguageChip.Text = "VI 🔒";
                    langLabel = UiStrings.LockVi;
                    nextLang = "ZH";
                    break;
                default:
                    lblLanguageChip.Text = "ZH 🔒";
                    langLabel = UiStrings.LockZh;
                    nextLang = UiStrings.LockAuto;
                    break;
            }
            pnlLanguageChip.FillColor = UiPalette.Accent;
            lblLanguageChip.ForeColor = Color.White;
            var tip = UiStrings.TipChipLocked(langLabel, nextLang);
            toolTipToolbar.SetToolTip(pnlLanguageChip, tip);
            toolTipToolbar.SetToolTip(lblLanguageChip, tip);
        }

        private void pnlLanguageChip_Click(object sender, EventArgs e)
        {
            if (_lockedLanguage == null) _lockedLanguage = Language.English;
            else if (_lockedLanguage == Language.English) _lockedLanguage = Language.Vietnamese;
            else if (_lockedLanguage == Language.Vietnamese) _lockedLanguage = Language.Chinese;
            else _lockedLanguage = null;
            UpdateLanguageChipLockState();
            string lockCode = _lockedLanguage == null ? ""
                : _lockedLanguage == Language.English ? "EN"
                : _lockedLanguage == Language.Vietnamese ? "VI" : "ZH";
            SaveSetting("UI:LockedLanguage", lockCode);
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            cmsInfo.Show(btnInfo, new Point(0, btnInfo.Height));
        }

        private void tsmiLangToggle_Click(object sender, EventArgs e)
        {
            UiStrings.IsVietnamese = !UiStrings.IsVietnamese;
            SaveSetting("UI:Language", UiStrings.IsVietnamese ? "VI" : "EN");
            ApplyUiLanguage();
        }

        private void ApplyUiLanguage()
        {
            btnSearch.Text = UiStrings.BtnSearch;
            btnRetry.Text = UiStrings.BtnRetry;
            btnMiniMode.Text = _isMiniMode ? UiStrings.BtnMiniModeOn : UiStrings.BtnMiniModeOff;
            btnClipboardToggle.Text = _clipboardWatchEnabled ? UiStrings.BtnClipboardOn : UiStrings.BtnClipboardOff;

            lblViColumnHeader.Text = UiStrings.ColVi;
            lblZhColumnHeader.Text = UiStrings.ColZh;
            lblDefinitionsHeader.Text = UiStrings.SectionDefinitions;
            lblExamplesHeader.Text = UiStrings.SectionExamples;

            tsmiAbout.Text = UiStrings.MenuAbout;
            tsmiGopY.Text = UiStrings.MenuFeedback;
            tsmiDongGop.Text = UiStrings.MenuDonate;
            tsmiLangToggle.Text = UiStrings.MenuLangToggle;

            toolTipToolbar.SetToolTip(btnSearch, UiStrings.TipSearch);
            toolTipToolbar.SetToolTip(btnRetry, UiStrings.TipRetry);
            toolTipToolbar.SetToolTip(btnClipboardToggle, UiStrings.TipClipboard);
            toolTipToolbar.SetToolTip(btnHotkeyConfig, UiStrings.TipHotkey);
            toolTipToolbar.SetToolTip(btnMiniMode, UiStrings.TipMiniMode);
            toolTipToolbar.SetToolTip(btnBookmarkToggle, UiStrings.TipBookmark);
            toolTipToolbar.SetToolTip(btnHistory, UiStrings.TipHistory);
            toolTipToolbar.SetToolTip(btnInfo, UiStrings.TipInfo);

            UpdateLanguageChipLockState();
            _ = RefreshUsageLabelAsync();
        }

        private void tsmiAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                UiStrings.AboutBody,
                UiStrings.AboutTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void tsmiGopY_Click(object sender, EventArgs e)
        {
            try { System.Diagnostics.Process.Start("https://github.com/ThanhNguyenVN93/frm-dictionary-tool/issues"); }
            catch { }
        }

        private void tsmiDongGop_Click(object sender, EventArgs e)
        {
            using (var form = new DonateForm())
                form.ShowDialog(this);
        }

        private void ResetToIdleState()
        {
            _isLoading = false;
            circleLoading.Visible = false;
            btnRetry.Visible = false;
            btnSearch.Visible = !_isMiniMode;
            pnlError.Visible = false;
            pnlColumnEn.Visible = true;
            pnlColumnVi.Visible = true;
            pnlColumnZh.Visible = true;
            pnlExamples.Visible = true;
            btnBookmarkToggle.Enabled = false;
            SetBookmarkButtonState(false);
        }

        private void ShowLoading()
        {
            _isLoading = true;
            btnSearch.Visible = false;
            btnRetry.Visible = false;
            circleLoading.Visible = true;
            pnlColumnEn.Visible = false;
            pnlColumnVi.Visible = false;
            pnlColumnZh.Visible = false;
            pnlExamples.Visible = false;
            pnlError.Visible = false;
            btnBookmarkToggle.Enabled = false;
        }

        private void HideLoading()
        {
            _isLoading = false;
            circleLoading.Visible = false;
            btnSearch.Visible = !_isMiniMode;
        }

        private async Task LoadAutoCompleteWordsAsync()
        {
            try
            {
                var enWords = await _cacheRepository.GetAllCachedWordsAsync().ConfigureAwait(true);
                var viWords = await _cacheRepository.GetAllCachedViWordsAsync().ConfigureAwait(true);
                var zhWords = await _cacheRepository.GetAllCachedZhWordsAsync().ConfigureAwait(true);

                var allWords = new System.Collections.Generic.List<string>(enWords.Count + viWords.Count + zhWords.Count);
                allWords.AddRange(enWords);
                allWords.AddRange(viWords);
                allWords.AddRange(zhWords);

                foreach (var word in allWords)
                {
                    if (!txtSearch.AutoCompleteCustomSource.Contains(word))
                    {
                        txtSearch.AutoCompleteCustomSource.Add(word);
                    }
                }
            }
            catch (Exception)
            {
                // Không để lỗi nạp gợi ý làm hỏng luồng chính.
            }
        }

        private async Task RefreshUsageLabelAsync()
        {
            try
            {
                var collegiateCount = await _apiUsageTracker.GetTodayCountAsync("Collegiate").ConfigureAwait(true);
                var learnerCount = await _apiUsageTracker.GetTodayCountAsync("Learner's").ConfigureAwait(true);
                lblUsage.Text = UiStrings.UsageLabel(collegiateCount, learnerCount);
            }
            catch (Exception)
            {
                // Không để lỗi đếm quota làm hỏng luồng chính.
            }
        }

        private void ShowError(string message, bool canRetry, bool isSystemError)
        {
            pnlColumnEn.Visible = false;
            pnlColumnVi.Visible = false;
            pnlColumnZh.Visible = false;
            pnlExamples.Visible = false;
            pnlError.Visible = true;
            lblErrorMessage.Text = message;
            lblErrorMessage.ForeColor = isSystemError ? UiPalette.ErrorText : UiPalette.NeutralText;
            if (canRetry)
            {
                btnSearch.Visible = false;
                btnRetry.Visible = true;
            }
            else
            {
                btnRetry.Visible = false;
                btnSearch.Visible = !_isMiniMode;
            }

            _currentResultQuery = null;
            btnBookmarkToggle.Enabled = false;
            SetBookmarkButtonState(false);
        }

        private void RenderResult(TrilingualResult result)
        {
            bool hasEnglishData = result.EnglishResult != null && !result.EnglishResult.HasError && !result.EnglishResult.NotFound;
            bool hasVietnameseData = result.VietnameseResult != null && !result.VietnameseResult.NotFound;
            bool hasChineseData = result.ChineseResult != null && !result.ChineseResult.NotFound;
            bool systemError = result.EnglishResult != null && result.EnglishResult.HasError;

            if (!hasEnglishData && !hasVietnameseData && !hasChineseData)
            {
                if (systemError)
                {
                    ShowError(UiStrings.ErrNetwork, canRetry: true, isSystemError: true);
                }
                else
                {
                    ShowError(UiStrings.ErrNotFound, canRetry: false, isSystemError: false);
                }
                return;
            }

            btnRetry.Visible = false;
            pnlError.Visible = false;
            pnlColumnEn.Visible = true;
            pnlColumnVi.Visible = true;
            pnlColumnZh.Visible = true;
            pnlExamples.Visible = true;

            RenderEnglishColumn(result.EnglishResult);
            RenderVietnameseColumn(result.VietnameseResult);
            RenderChineseColumn(result.ChineseResult);
            RenderExamples(result.ExampleSentences);

            var knownWord = result.EnglishResult?.Word ?? result.Query;
            if (!string.IsNullOrEmpty(knownWord) && !txtSearch.AutoCompleteCustomSource.Contains(knownWord))
            {
                txtSearch.AutoCompleteCustomSource.Add(knownWord);
            }

            _currentResultQuery = result.Query;
            _currentResultLanguage = result.DetectedLanguage;
            btnBookmarkToggle.Enabled = true;
            _ = _historyRepository.AddAsync(result.Query, result.DetectedLanguage);
            _ = _historyRepository.SaveTrilingualResultAsync(result.Query, result.DetectedLanguage, result);
            _ = RefreshBookmarkButtonAsync();
        }

        private async Task RefreshBookmarkButtonAsync()
        {
            if (_currentResultQuery == null)
            {
                return;
            }

            var query = _currentResultQuery;
            var language = _currentResultLanguage;

            bool isBookmarked;
            try
            {
                isBookmarked = await _bookmarkRepository.IsBookmarkedAsync(query, language).ConfigureAwait(true);
            }
            catch (Exception)
            {
                return;
            }

            // Tránh set nhầm trạng thái nếu người dùng đã tra từ khác trong lúc chờ.
            if (string.Equals(_currentResultQuery, query, StringComparison.Ordinal) && _currentResultLanguage == language)
            {
                SetBookmarkButtonState(isBookmarked);
            }
        }

        private void SetBookmarkButtonState(bool isBookmarked)
        {
            btnBookmarkToggle.Text = isBookmarked ? "★" : "☆";
            btnBookmarkToggle.FillColor = isBookmarked ? UiPalette.Accent : UiPalette.NeutralSoft;
            btnBookmarkToggle.ForeColor = isBookmarked ? Color.White : UiPalette.TextSecondary;
        }

        private async void btnBookmarkToggle_Click(object sender, EventArgs e)
        {
            if (_currentResultQuery == null)
            {
                return;
            }

            var query = _currentResultQuery;
            var language = _currentResultLanguage;

            var isBookmarked = await _bookmarkRepository.IsBookmarkedAsync(query, language).ConfigureAwait(true);
            if (isBookmarked)
            {
                await _bookmarkRepository.RemoveAsync(query, language).ConfigureAwait(true);
            }
            else
            {
                await _bookmarkRepository.AddAsync(query, language).ConfigureAwait(true);
            }

            await RefreshBookmarkButtonAsync().ConfigureAwait(true);
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            using (var dialog = new HistoryBookmarkForm(_historyRepository, _bookmarkRepository, _bookmarkXlsxExporter))
            {
                dialog.SearchRequested += (word, lang) =>
                {
                    txtSearch.Text = word;
                    debounceTimer.Stop();
                    _ = SearchFromHistoryAsync(word, lang);
                };

                dialog.ShowDialog(this);
            }
        }

        private void RenderEnglishColumn(WordLookupResult result)
        {
            if (result == null || (result.HasError == false && result.NotFound && result.Phonetics.Count == 0 && result.Meanings.Count == 0 && result.LearnerMeanings.Count == 0))
            {
                lblWord.Text = "—";
                pnlPhonetics.Controls.Clear();
                pnlDefinitions.Controls.Clear();
                pnlDefinitions.Controls.Add(CreateInfoLabel(UiStrings.InfoNoData));
                pnlLearnerDefs.Controls.Clear();
                return;
            }

            lblWord.Text = result.Word;
            RenderPhonetics(result);
            RenderDictionaryMeanings(result);
            RenderLearnerMeanings(result);
        }

        private void RenderVietnameseColumn(VietnameseResult result)
        {
            pnlViDefinitions.Controls.Clear();

            if (result == null || result.NotFound)
            {
                lblViWord.Text = "—";
                pnlViDefinitions.Controls.Add(CreateInfoLabel(UiStrings.InfoNoData));
                return;
            }

            lblViWord.Text = result.Word;
            foreach (var definition in result.Definitions)
            {
                pnlViDefinitions.Controls.Add(CreateBulletLabel(definition));
            }
        }

        private void RenderChineseColumn(ChineseResult result)
        {
            pnlZhDefinitions.Controls.Clear();

            if (result == null || result.NotFound)
            {
                lblZhWord.Text = "—";
                lblZhPinyin.Text = string.Empty;
                pnlZhDefinitions.Controls.Add(CreateInfoLabel(UiStrings.InfoNoData));
                return;
            }

            lblZhWord.Text = result.Simplified == result.Traditional
                ? result.Simplified
                : $"{result.Simplified} ({result.Traditional})";
            lblZhPinyin.Text = result.Pinyin;

            foreach (var definition in result.Definitions)
            {
                pnlZhDefinitions.Controls.Add(CreateBulletLabel(definition));
            }
        }

        private void RenderExamples(List<ExampleSentence> examples)
        {
            // Controls index 0 là lblExamplesHeader, luôn giữ lại.
            while (pnlExamplesContent.Controls.Count > 1)
            {
                pnlExamplesContent.Controls.RemoveAt(1);
            }

            if (examples == null || examples.Count == 0)
            {
                pnlExamplesContent.Controls.Add(CreateInfoLabel(UiStrings.InfoNoExamples));
                return;
            }

            foreach (var example in examples)
            {
                var block = new FlickerFreeFlowLayoutPanel
                {
                    FlowDirection = FlowDirection.TopDown,
                    WrapContents = false,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    Width = 890,
                    Margin = new Padding(0, 0, 0, 12)
                };

                if (!string.IsNullOrWhiteSpace(example.English))
                {
                    block.Controls.Add(CreateExampleLanguageLabel("EN", example.English));
                }
                if (!string.IsNullOrWhiteSpace(example.Vietnamese))
                {
                    block.Controls.Add(CreateExampleLanguageLabel("VI", example.Vietnamese));
                }
                if (!string.IsNullOrWhiteSpace(example.Chinese))
                {
                    block.Controls.Add(CreateExampleLanguageLabel("ZH", example.Chinese));
                }

                pnlExamplesContent.Controls.Add(block);
            }
        }

        private static Label CreateExampleLanguageLabel(string languageCode, string text)
        {
            return new Label
            {
                AutoSize = true,
                MaximumSize = new Size(870, 0),
                Text = $"[{languageCode}]  {text}",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = languageCode == "EN" ? UiPalette.TextPrimary : UiPalette.TextSecondary,
                Margin = new Padding(0, 1, 0, 1)
            };
        }

        private void RenderPhonetics(WordLookupResult result)
        {
            pnlPhonetics.Controls.Clear();

            foreach (var phonetic in result.Phonetics)
            {
                bool hasAudio = !string.IsNullOrWhiteSpace(phonetic.AudioUrl);
                bool hasText = !string.IsNullOrWhiteSpace(phonetic.Text);
                if (!hasAudio && !hasText)
                {
                    continue;
                }

                var labelParts = new List<string>();
                if (!string.IsNullOrEmpty(phonetic.AccentLabel))
                {
                    labelParts.Add(phonetic.AccentLabel);
                }
                if (hasText)
                {
                    labelParts.Add(phonetic.Text);
                }
                var labelText = string.Join("  ", labelParts);

                // Guna2Button.Text bị cắt cụt khi chuỗi trộn icon loa (emoji) với ký
                // hiệu IPA (Unicode IPA Extensions) trong cùng 1 property Text — đã
                // xác minh Text/Width của control vẫn đúng, chỉ riêng phần vẽ (paint)
                // bị lỗi. Dùng Label (GDI chuẩn, render IPA đúng) cho phần chữ,
                // Guna2Button chỉ chứa icon loa (1 ký tự, không lỗi).
                var innerRow = new FlickerFreeFlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    WrapContents = false
                };

                if (hasAudio)
                {
                    var iconButton = new Guna2Button
                    {
                        Text = "🔊",
                        AutoSize = true,
                        Padding = new Padding(0),
                        Font = new Font("Segoe UI", 10F),
                        FillColor = UiPalette.AccentSoft,
                        ForeColor = UiPalette.Accent,
                        BorderRadius = 14,
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0, 0, 6, 0)
                    };
                    iconButton.HoverState.FillColor = UiPalette.AccentSoft;
                    iconButton.ShadowDecoration.Enabled = false;

                    var audioUrl = phonetic.AudioUrl;
                    iconButton.Click += async (s, e) => await PlayAudioAsync(audioUrl).ConfigureAwait(true);

                    innerRow.Controls.Add(iconButton);
                }

                if (!string.IsNullOrEmpty(labelText))
                {
                    var textLabel = new Label
                    {
                        AutoSize = true,
                        Text = labelText,
                        Font = new Font("Segoe UI", 10F),
                        ForeColor = UiPalette.Accent,
                        Margin = new Padding(0, 4, 0, 0)
                    };
                    innerRow.Controls.Add(textLabel);
                }

                var pill = new Guna2Panel
                {
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BorderRadius = 14,
                    FillColor = UiPalette.AccentSoft,
                    Padding = new Padding(14, 8, 14, 8),
                    Margin = new Padding(0, 0, 10, 10)
                };
                pill.ShadowDecoration.Enabled = false;
                pill.Controls.Add(innerRow);

                pnlPhonetics.Controls.Add(pill);
            }
        }

        private void RenderDictionaryMeanings(WordLookupResult result)
        {
            pnlDefinitions.Controls.Clear();

            if (result.Meanings.Count == 0)
            {
                pnlDefinitions.Controls.Add(CreateInfoLabel(UiStrings.InfoNoDataDictApi));
                return;
            }

            foreach (var meaning in result.Meanings)
            {
                pnlDefinitions.Controls.Add(CreatePartOfSpeechBadge(meaning.PartOfSpeech));

                foreach (var definition in meaning.Definitions)
                {
                    pnlDefinitions.Controls.Add(CreateBulletLabel(definition.Definition));

                    if (!string.IsNullOrWhiteSpace(definition.Example))
                    {
                        pnlDefinitions.Controls.Add(CreateExampleLabel(definition.Example));
                    }
                }
            }
        }

        private void RenderLearnerMeanings(WordLookupResult result)
        {
            pnlLearnerDefs.Controls.Clear();

            if (result.LearnerMeanings.Count == 0)
            {
                pnlLearnerDefs.Controls.Add(CreateInfoLabel(UiStrings.InfoNoMWData));
                return;
            }

            foreach (var meaning in result.LearnerMeanings)
            {
                var header = string.IsNullOrEmpty(meaning.Source)
                    ? meaning.PartOfSpeech
                    : $"{meaning.PartOfSpeech} · {meaning.Source}";
                pnlLearnerDefs.Controls.Add(CreatePartOfSpeechBadge(header));

                foreach (var shortDef in meaning.ShortDefinitions)
                {
                    pnlLearnerDefs.Controls.Add(CreateBulletLabel(shortDef));
                }
            }
        }

        private static Guna2Button CreatePartOfSpeechBadge(string text)
        {
            var badge = new Guna2Button
            {
                Text = text.ToUpperInvariant(),
                AutoSize = true,
                Padding = new Padding(8, 4, 8, 4),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                FillColor = UiPalette.NeutralSoft,
                ForeColor = UiPalette.TextSecondary,
                BorderRadius = 10,
                Cursor = Cursors.Default,
                TabStop = false,
                Margin = new Padding(0, 10, 0, 6)
            };
            badge.HoverState.FillColor = UiPalette.NeutralSoft;
            return badge;
        }

        private static Label CreateBulletLabel(string text)
        {
            return new Label
            {
                AutoSize = true,
                MaximumSize = new Size(260, 0),
                Text = "•  " + text,
                Font = new Font("Segoe UI", 10F),
                ForeColor = UiPalette.TextPrimary,
                Margin = new Padding(6, 2, 0, 2)
            };
        }

        private static Label CreateExampleLabel(string text)
        {
            return new Label
            {
                AutoSize = true,
                MaximumSize = new Size(240, 0),
                Text = "      " + text,
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = UiPalette.TextSecondary,
                Margin = new Padding(6, 0, 0, 6)
            };
        }

        private static Label CreateInfoLabel(string text)
        {
            return new Label
            {
                AutoSize = true,
                Text = text,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                ForeColor = UiPalette.TextSecondary,
                Margin = new Padding(0, 2, 0, 2)
            };
        }

        private async Task PlayAudioAsync(string audioUrl)
        {
            try
            {
                var data = await _audioCacheRepository.GetOrDownloadAsync(audioUrl).ConfigureAwait(true);

                using (var ms = new MemoryStream(data))
                using (var reader = new Mp3FileReader(ms))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(reader);
                    outputDevice.Play();

                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        await Task.Delay(100).ConfigureAwait(true);
                    }
                }
            }
            catch (Exception)
            {
                // Lỗi mạng/404/định dạng audio không được làm crash UI (spec 3.2).
            }
        }

        private void LoadSettings()
        {
            var lang = System.Configuration.ConfigurationManager.AppSettings["UI:Language"];
            UiStrings.IsVietnamese = lang != "EN";

            if (bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["UI:ClipboardEnabled"], out var clipEnabled))
                _clipboardWatchEnabled = clipEnabled;

            var lockedLang = System.Configuration.ConfigurationManager.AppSettings["UI:LockedLanguage"];
            switch (lockedLang)
            {
                case "EN": _lockedLanguage = Language.English; break;
                case "VI": _lockedLanguage = Language.Vietnamese; break;
                case "ZH": _lockedLanguage = Language.Chinese; break;
                default:   _lockedLanguage = null; break;
            }
        }

        private void ApplyClipboardButtonState()
        {
            btnClipboardToggle.FillColor = _clipboardWatchEnabled ? UiPalette.Accent : UiPalette.NeutralSoft;
            btnClipboardToggle.ForeColor = _clipboardWatchEnabled ? Color.White : UiPalette.TextSecondary;
        }

        private void SaveSetting(string key, string value)
        {
            try
            {
                var config = System.Configuration.ConfigurationManager.OpenExeConfiguration(
                    System.Configuration.ConfigurationUserLevel.None);
                if (config.AppSettings.Settings[key] != null)
                    config.AppSettings.Settings[key].Value = value;
                else
                    config.AppSettings.Settings.Add(key, value);
                config.Save(System.Configuration.ConfigurationSaveMode.Modified);
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception) { }
        }
    }
}
