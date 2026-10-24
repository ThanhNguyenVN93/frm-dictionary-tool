using System.Threading;
using System.Threading.Tasks;

namespace DictionaryTool.Core.Clients
{
    /// <summary>
    /// Kết quả tra cứu từ dictionaryapi.dev cho 1 nguồn duy nhất, tách khỏi
    /// WordLookupResult để DictionaryService tự merge nhiều nguồn lại.
    /// </summary>
    public class DictionaryApiDevResult
    {
        public System.Collections.Generic.List<Models.Phonetic> Phonetics { get; set; } = new System.Collections.Generic.List<Models.Phonetic>();
        public System.Collections.Generic.List<Models.DictionaryMeaning> Meanings { get; set; } = new System.Collections.Generic.List<Models.DictionaryMeaning>();
    }

    public interface IDictionaryApiDevClient
    {
        /// <summary>
        /// Trả về null nếu từ không tồn tại (404 hoặc mảng rỗng) — đây KHÔNG phải lỗi.
        /// Ném exception nếu có lỗi hệ thống/mạng thật sự (caller chịu trách nhiệm catch).
        /// </summary>
        Task<DictionaryApiDevResult> GetAsync(string word, CancellationToken ct = default(CancellationToken));
    }
}
