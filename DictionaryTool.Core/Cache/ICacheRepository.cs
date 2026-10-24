using System.Collections.Generic;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Cache
{
    public interface ICacheRepository
    {
        /// <summary>Trả về null nếu cache miss. Nếu hit, WordLookupResult.FromCache sẽ được set true.</summary>
        Task<WordLookupResult> TryGetAsync(string normalizedWord);

        Task SaveAsync(string normalizedWord, WordLookupResult result);

        /// <summary>Danh sách tất cả từ đã cache — dùng cho gợi ý autocomplete ở UI.</summary>
        Task<List<string>> GetAllCachedWordsAsync();

        /// <summary>Danh sách từ tiếng Việt đã tra (từ SearchHistory) — dùng cho autocomplete.</summary>
        Task<List<string>> GetAllCachedViWordsAsync();

        /// <summary>Danh sách từ tiếng Trung đã tra (từ SearchHistory) — dùng cho autocomplete.</summary>
        Task<List<string>> GetAllCachedZhWordsAsync();
    }
}
