using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core
{
    /// <summary>
    /// Nhận diện ngôn ngữ của từ khoá nhập vào (EN/VI/ZH), dựa trên vùng ký tự
    /// Unicode. Riêng tiếng Việt gõ không dấu (vd "khong") không có dấu hiệu
    /// Unicode để nhận ra — <see cref="DetectAsync"/> xử lý case này bằng cách tra
    /// ngược index từ điển qua <paramref name="vietnameseIndexProbe"/> (xem
    /// TrilingualLookupService.ResolveVietnameseEntryAsync).
    /// </summary>
    public static class LanguageDetector
    {
        private static readonly Regex ChineseRegex = new Regex(@"[一-鿿]", RegexOptions.Compiled);

        private static readonly Regex VietnameseDiacriticsRegex = new Regex(
            @"[àáâãèéêìíòóôõùúýăđơưạảấầẩẫậắằẳẵặẹẻẽềểễệỉịọỏốồổỗộớờởỡợụủứừửữựỳỵỷỹ]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Chỉ thử probe VI khi từ có ký hiệu điển hình của tiếng Việt gõ không dấu:
        // "kh" (khong, khe…), hoặc bắt đầu bằng "ng"/"nh"/"ngh" (nguoi, nhau…).
        // Các từ ASCII thuần không có dấu hiệu này (vd "run", "love") giữ nguyên EN
        // để tránh nhầm với những từ tồn tại trong cả hai ngôn ngữ.
        private static readonly Regex AsciiVietnameseMarkerRegex = new Regex(
            @"^(ng|nh|ngh)|kh",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Language Detect(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return Language.Unknown;
            }

            if (ChineseRegex.IsMatch(input))
            {
                return Language.Chinese;
            }

            if (VietnameseDiacriticsRegex.IsMatch(input))
            {
                return Language.Vietnamese;
            }

            return Language.English;
        }

        /// <summary>
        /// Như <see cref="Detect"/>, nhưng khi kết quả nhanh (không dấu hiệu Unicode
        /// nào) là English, thử thêm 1 bước tra ngược index tiếng Việt (gõ không
        /// dấu/bỏ loại từ...) qua <paramref name="vietnameseIndexProbe"/> trước khi
        /// kết luận English. Nếu probe null thì hành vi giống hệt <see cref="Detect"/>.
        /// </summary>
        public static async Task<Language> DetectAsync(string input, Func<string, Task<bool>> vietnameseIndexProbe)
        {
            var quick = Detect(input);

            if (quick != Language.English || vietnameseIndexProbe == null)
                return quick;

            // Chỉ probe khi có dấu hiệu từ tiếng Việt gõ không dấu — tránh nhầm
            // những từ đồng dạng ASCII như "run" (cũng là từ tiếng Việt trong StarDict).
            // Ngoài marker "kh/ng/nh/ngh", còn probe khi input bắt đầu bằng loại từ
            // tiếng Việt ("con mèo", "cái bàn"…) — probe mới là nguồn quyết định, nên
            // nếu phần còn lại không khớp index thì vẫn trả English.
            var trimmed = input.Trim();
            if (!AsciiVietnameseMarkerRegex.IsMatch(trimmed)
                && VietnameseNormalizer.StripLeadingClassifier(trimmed) == null)
                return Language.English;

            var isKnownVietnamese = await vietnameseIndexProbe(trimmed).ConfigureAwait(false);
            return isKnownVietnamese ? Language.Vietnamese : Language.English;
        }
    }
}
