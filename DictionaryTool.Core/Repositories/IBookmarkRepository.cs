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
    }
}
