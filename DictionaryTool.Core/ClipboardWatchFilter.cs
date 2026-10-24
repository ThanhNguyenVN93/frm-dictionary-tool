namespace DictionaryTool.Core
{
    /// <summary>
    /// Quyết định 1 đoạn text vừa copy vào clipboard có nên tự động tra từ hay
    /// không. Tách riêng khỏi UI để test được mà không cần Clipboard/Form thật
    /// (Clipboard watcher thực tế chỉ là lớp mỏng gọi class này, xem MainForm).
    /// </summary>
    public static class ClipboardWatchFilter
    {
        /// <summary>Đoạn text dài hơn mức này coi như không phải 1 từ/cụm từ cần tra (vd copy nguyên đoạn văn).</summary>
        public const int MaxLength = 60;

        public static bool ShouldTrigger(string previousText, string newText)
        {
            if (string.IsNullOrWhiteSpace(newText))
            {
                return false;
            }

            var trimmed = newText.Trim();

            if (trimmed.Length > MaxLength)
            {
                return false;
            }

            // Nhiều dòng -> chắc chắn không phải 1 từ/cụm từ tra điển.
            if (trimmed.IndexOf('\n') >= 0 || trimmed.IndexOf('\r') >= 0)
            {
                return false;
            }

            if (string.Equals(trimmed, (previousText ?? string.Empty).Trim()))
            {
                return false;
            }

            return true;
        }
    }
}
