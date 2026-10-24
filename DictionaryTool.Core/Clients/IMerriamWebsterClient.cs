using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Clients
{
    public interface IMerriamWebsterClient
    {
        /// <summary>
        /// Trả về danh sách rỗng nếu từ không tồn tại (response là mảng gợi ý chính tả)
        /// hoặc nếu API key chưa được cấu hình. Ném exception nếu lỗi hệ thống/mạng thật sự.
        /// </summary>
        Task<MwCollegiateResult> GetCollegiateAsync(string word, CancellationToken ct = default(CancellationToken));

        Task<List<MwMeaning>> GetLearnerAsync(string word, CancellationToken ct = default(CancellationToken));
    }
}
