using System.Collections.Generic;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Repositories
{
    public interface IHistoryRepository
    {
        /// <summary>Ghi nhận 1 lượt tra từ thành công. Tra lại từ đã có sẽ cập nhật lại thời gian, không tạo dòng mới.</summary>
        Task AddAsync(string query, Language language);

        /// <summary>Danh sách lượt tra gần nhất, mới nhất trước.</summary>
        Task<List<HistoryEntry>> GetRecentAsync(int limit = 50);

        Task ClearAsync();

        /// <summary>Lưu kết quả tra đầy đủ (EN+VI+ZH) vào TrilingualCache để hiện lại tức thời khi click lịch sử.</summary>
        Task SaveTrilingualResultAsync(string query, Language language, TrilingualResult result);

        /// <summary>Lấy kết quả đã cache; trả về null nếu không có.</summary>
        Task<TrilingualResult> GetTrilingualResultAsync(string query, Language language);
    }
}
