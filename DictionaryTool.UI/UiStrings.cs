using DictionaryTool.Core.Models;

namespace DictionaryTool.UI
{
    internal static class UiStrings
    {
        public static bool IsVietnamese { get; set; } = true;

        // Toolbar
        public static string SearchPlaceholder  => IsVietnamese
            ? "🔍  Nhập từ EN / VI / ZH cần tra..."
            : "🔍  Enter an EN / VI / ZH word...";
        public static string BtnSearch          => IsVietnamese ? "Tra từ"     : "Search";
        public static string BtnRetry           => IsVietnamese ? "Thử lại"    : "Retry";
        public static string BtnMiniModeOff     => IsVietnamese ? "⤢ Thu gọn"  : "⤢ Mini";
        public static string BtnMiniModeOn      => IsVietnamese ? "⤢ Đầy đủ"  : "⤢ Full";
        public static string BtnClipboardOn     => IsVietnamese ? "📋 Bật"     : "📋 On";
        public static string BtnClipboardOff    => IsVietnamese ? "📋 Tắt"     : "📋 Off";

        // Column headers
        public static string ColEn              => IsVietnamese ? "🇬🇧 ENGLISH"      : "🇬🇧 ENGLISH";
        public static string ColVi              => IsVietnamese ? "🇻🇳 TIẾNG VIỆT"   : "🇻🇳 VIETNAMESE";
        public static string ColZh              => IsVietnamese ? "🇨🇳 中文"          : "🇨🇳 CHINESE";

        // Section headers
        public static string SectionDefinitions => IsVietnamese ? "ĐỊNH NGHĨA"          : "DEFINITIONS";
        public static string SectionMW          => "MERRIAM-WEBSTER";
        public static string SectionExamples    => IsVietnamese ? "CÂU VÍ DỤ · TATOEBA" : "EXAMPLES · TATOEBA";

        // Info menu
        public static string MenuAbout          => IsVietnamese ? "ℹ️  Giới thiệu" : "ℹ️  About";
        public static string MenuFeedback       => IsVietnamese ? "💬  Góp ý"      : "💬  Feedback";
        public static string MenuDonate         => IsVietnamese ? "🤝  Đóng góp"   : "🤝  Donate";

        // Error / info messages
        public static string ErrNetwork         => IsVietnamese
            ? "⚠️  Không thể kết nối tới các nguồn từ điển. Vui lòng kiểm tra kết nối mạng và thử lại."
            : "⚠️  Could not connect to dictionary sources. Please check your network connection and try again.";
        public static string ErrNotFound        => IsVietnamese
            ? "🔍  Không tìm thấy từ này. Kiểm tra lại chính tả và thử lại nhé."
            : "🔍  Word not found. Please check your spelling and try again.";
        public static string InfoNoData         => IsVietnamese ? "Không có dữ liệu."                        : "No data available.";
        public static string InfoNoDataDictApi  => IsVietnamese ? "Không có dữ liệu từ dictionaryapi.dev."   : "No data from dictionaryapi.dev.";
        public static string InfoNoExamples     => IsVietnamese
            ? "Chưa có câu ví dụ cho từ này."
            : "No example sentences for this word.";
        public static string InfoNoMWData       => IsVietnamese
            ? "Không có dữ liệu từ Merriam-Webster."
            : "No data from Merriam-Webster.";

        // Hotkey registration error
        public static string ErrHotkeyTitle    => IsVietnamese ? "Phím tắt không khả dụng"  : "Hotkey Unavailable";
        public static string ErrHotkeyBody(string combo) => IsVietnamese
            ? $"Không thể đăng ký phím tắt {combo}.\nTổ hợp phím này có thể đang được dùng bởi ứng dụng khác."
            : $"Could not register hotkey {combo}.\nThis key combination may be in use by another application.";

        // About dialog
        public static string AboutTitle        => IsVietnamese ? "Giới thiệu" : "About";
        public static string AboutBody         => IsVietnamese
            ? "Dictionary Tool — EN / VI / ZH\n\nTra từ điển Anh · Việt · Trung trong một cửa sổ.\n\nPhiên bản 1.0"
            : "Dictionary Tool — EN / VI / ZH\n\nLook up English, Vietnamese and Chinese in one window.\n\nVersion 1.0";

        // Tooltips
        public static string TipClipboard       => IsVietnamese
            ? "Tự động tra từ khi copy văn bản ở ứng dụng khác"
            : "Auto-search when copying text from other apps";
        public static string TipHotkey          => IsVietnamese
            ? "Cấu hình phím tắt gọi nhanh"
            : "Configure global hotkey";
        public static string TipMiniMode        => IsVietnamese
            ? "Thu nhỏ / phóng to cửa sổ"
            : "Toggle compact / full window";
        public static string TipBookmark        => IsVietnamese
            ? "Đánh dấu / bỏ đánh dấu từ đang xem"
            : "Bookmark / unbookmark current word";
        public static string TipHistory         => IsVietnamese
            ? "Xem lịch sử tra từ và từ đã đánh dấu"
            : "View search history and bookmarks";
        public static string TipInfo            => IsVietnamese
            ? "Giới thiệu · Góp ý · Đóng góp"
            : "About · Feedback · Donate";
        public static string TipHelp            => IsVietnamese
            ? "Hướng dẫn sử dụng"
            : "How to use";
        public static string TipSearch          => IsVietnamese
            ? "Tra nghĩa của từ đã nhập (Enter)"
            : "Look up the entered word (Enter)";
        public static string TipRetry           => IsVietnamese
            ? "Tra lại từ sau khi gặp lỗi"
            : "Retry the last search";
        public static string TipChipAuto        => IsVietnamese
            ? "Đang tự động nhận diện ngôn ngữ — click để chốt EN"
            : "Auto-detecting language — click to lock to EN";
        public static string TipChipLocked(string lang, string next) => IsVietnamese
            ? $"Đang chốt {lang} — click để chuyển sang {next}"
            : $"Locked to {lang} — click to switch to {next}";

        // Language lock chip labels
        public static string LockEn             => IsVietnamese ? "Tiếng Anh"  : "English";
        public static string LockVi             => IsVietnamese ? "Tiếng Việt" : "Vietnamese";
        public static string LockZh             => IsVietnamese ? "Tiếng Trung": "Chinese";
        public static string LockAuto           => IsVietnamese ? "Tự động"    : "Auto";

        // Usage label
        public static string UsageLabel(int collegiate, int learner) => IsVietnamese
            ? $"MW hôm nay — Collegiate: {collegiate}/1000 · Learner's: {learner}/1000"
            : $"MW today — Collegiate: {collegiate}/1000 · Learner's: {learner}/1000";

        public static string MenuLangToggle     => IsVietnamese
            ? "🌐  Switch to English"
            : "🌐  Chuyển sang Tiếng Việt";

        public static string MenuAutoSearch     => IsVietnamese
            ? "⏱  Tự động tra"
            : "⏱  Auto-search";

        // --- #2 Ôn tập (SRS) ---
        public static string MenuReview          => IsVietnamese ? "📚  Ôn tập" : "📚  Review";
        public static string MenuReviewDue(int n) => IsVietnamese ? $"📚  Ôn tập ({n})" : $"📚  Review ({n})";

        // --- #3 Cỡ chữ ---
        public static string MenuFontSize        => IsVietnamese ? "🔤  Cỡ chữ" : "🔤  Font size";
        public static string FontSmall           => IsVietnamese ? "Nhỏ"   : "Small";
        public static string FontMedium          => IsVietnamese ? "Vừa"   : "Medium";
        public static string FontLarge           => IsVietnamese ? "Lớn"   : "Large";
        public static string FontXLarge          => IsVietnamese ? "Rất lớn" : "Extra large";

        // --- #4 Giao diện ---
        // Nhãn đổi chiều theo trạng thái hiện tại (giống MenuLangToggle): đang dark →
        // mời bật sáng; đang light → mời bật tối.
        public static string MenuTheme(bool isLight) => isLight
            ? (IsVietnamese ? "🌙  Giao diện tối" : "🌙  Dark theme")
            : (IsVietnamese ? "☀️  Giao diện sáng" : "☀️  Light theme");

        // --- #5 Đồng/trái nghĩa ---
        public static string LabelSynonyms       => IsVietnamese ? "Đồng nghĩa" : "Synonyms";
        public static string LabelAntonyms       => IsVietnamese ? "Trái nghĩa" : "Antonyms";

        // --- #7 Tách chữ Hán ---
        public static string SectionCharBreakdown => IsVietnamese ? "TỪNG CHỮ" : "PER CHARACTER";

        // --- #6 Badge nguồn ---
        public static string SourceLabel(SourceKind kind)
        {
            switch (kind)
            {
                case SourceKind.OfflineStarDict:        return IsVietnamese ? "📴 StarDict" : "📴 StarDict";
                case SourceKind.OfflineCcCedict:        return IsVietnamese ? "📴 CC-CEDICT" : "📴 CC-CEDICT";
                case SourceKind.OnlineDictApi:          return IsVietnamese ? "🌐 Online" : "🌐 Online";
                case SourceKind.OnlineWiktionary:       return IsVietnamese ? "🌐 Wiktionary" : "🌐 Wiktionary";
                case SourceKind.OnlineGoogleTranslate:  return IsVietnamese ? "🌐 Google Dịch" : "🌐 Google Translate";
                case SourceKind.Cache:                  return IsVietnamese ? "💾 Bộ nhớ đệm" : "💾 Cached";
                default:                                return string.Empty;
            }
        }

        // --- ReviewForm ---
        public static string ReviewTitle         => IsVietnamese ? "Ôn tập từ đã lưu" : "Review bookmarks";
        public static string ReviewNoneDue       => IsVietnamese
            ? "🎉 Không có từ nào đến hạn ôn. Quay lại sau nhé!"
            : "🎉 No words due for review. Come back later!";
        public static string ReviewShowAnswer    => IsVietnamese ? "Hiện đáp án" : "Show answer";
        public static string ReviewProgress(int done, int total) => IsVietnamese
            ? $"Còn lại: {total - done}/{total}"
            : $"Remaining: {total - done}/{total}";
        public static string ReviewGradeAgain    => IsVietnamese ? "Quên" : "Again";
        public static string ReviewGradeHard     => IsVietnamese ? "Khó"  : "Hard";
        public static string ReviewGradeGood     => IsVietnamese ? "Nhớ"  : "Good";
        public static string ReviewGradeEasy     => IsVietnamese ? "Dễ"   : "Easy";
        public static string ReviewOpenInApp     => IsVietnamese ? "Mở tra đầy đủ" : "Open full lookup";
        public static string ReviewDone          => IsVietnamese ? "Đóng" : "Close";
        public static string ReviewLoading       => IsVietnamese ? "Đang tải…" : "Loading…";
        public static string ReviewFinished      => IsVietnamese
            ? "✅ Hoàn thành phiên ôn tập!"
            : "✅ Review session complete!";
    }
}
