using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Repositories
{
    public interface IBookmarkRepository
    {
        Task AddAsync(string query, Language language);
        Task RemoveAsync(string query, Language language);
        Task<bool> IsBookmarkedAsync(string query, Language language);

        /// <summary>Toàn bộ bookmark, mới thêm trước.</summary>
        Task<List<BookmarkEntry>> GetAllAsync();

        /// <summary>Các bookmark đã đến hạn ôn tập (NextReview &lt;= now hoặc chưa lên lịch), đến hạn sớm trước.</summary>
        Task<List<BookmarkEntry>> GetDueForReviewAsync(DateTime nowUtc);

        /// <summary>Số bookmark đang đến hạn ôn tập.</summary>
        Task<int> GetDueCountAsync(DateTime nowUtc);

        /// <summary>Ghi nhận 1 lần ôn và tính lịch kế tiếp theo SRS.</summary>
        Task ApplyReviewAsync(string query, Language language, ReviewGrade grade, DateTime nowUtc);

        /// <summary>
        /// Nhập bookmark từ bản sao lưu (giữ nguyên tiến độ SRS). overwrite=true thì
        /// ghi đè các từ đã có, false thì chỉ thêm từ mới. Trả về số dòng ảnh hưởng.
        /// </summary>
        Task<int> ImportAsync(IEnumerable<BookmarkEntry> entries, bool overwrite);
    }
}
