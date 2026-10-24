using System;
using System.Globalization;
using System.Text;

namespace DictionaryTool.Core
{
    /// <summary>
    /// Các phép biến đổi văn bản tiếng Việt dùng cho tra từ gõ không dấu và tự
    /// động bỏ loại từ đứng đầu (vd "con mèo" -> "mèo").
    /// </summary>
    public static class VietnameseNormalizer
    {
        /// <summary>
        /// Loại từ tiếng Việt phổ biến — khi tra 1 cụm "[loại từ] [từ chính]" mà
        /// không tìm thấy, thử bỏ loại từ rồi tra lại chỉ phần còn lại.
        /// </summary>
        public static readonly string[] Classifiers =
        {
            "con", "cái", "chiếc", "cục", "tờ", "quyển", "cuốn", "bộ", "đôi", "cặp",
            "căn", "ngôi", "tấm", "miếng", "viên", "hạt", "giọt", "sợi", "thanh",
            "cây", "bó", "chùm"
        };

        /// <summary>
        /// Bỏ dấu tiếng Việt bằng cách tách tổ hợp Unicode (NFD) rồi loại bỏ các
        /// dấu kết hợp (combining marks) — cách này tự động đúng cho mọi nguyên âm
        /// có dấu (à/á/ả/ã/ạ, ă+dấu, â+dấu, ê+dấu, ô+dấu, ơ+dấu, ư+dấu, ...) mà
        /// không cần liệt kê tay từng ký tự. Riêng "đ"/"Đ" không tách được qua NFD
        /// (là chữ cái riêng, không phải tổ hợp dấu) nên xử lý thủ công.
        /// </summary>
        public static string StripDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var decomposed = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(decomposed.Length);

            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd')
                .Replace('Đ', 'D');
        }

        /// <summary>
        /// Bỏ loại từ đứng đầu nếu có (vd "con mèo" -> "mèo"). Trả về null nếu từ
        /// không bắt đầu bằng loại từ nào trong danh sách.
        /// </summary>
        public static string StripLeadingClassifier(string word)
        {
            var trimmed = (word ?? string.Empty).Trim();

            foreach (var classifier in Classifiers)
            {
                var prefix = classifier + " ";
                if (trimmed.Length > prefix.Length && trimmed.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring(prefix.Length).Trim();
                }
            }

            return null;
        }
    }
}
