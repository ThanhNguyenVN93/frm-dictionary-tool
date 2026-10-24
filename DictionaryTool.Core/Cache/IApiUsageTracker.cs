using System.Threading;
using System.Threading.Tasks;

namespace DictionaryTool.Core.Cache
{
    /// <summary>
    /// Đếm số lượt gọi API thật trong ngày cho từng nguồn (vd "Collegiate",
    /// "Learner's"), tự reset khi sang ngày mới. Chỉ đếm lượt gọi API thật qua
    /// app này — không phản ánh quota MW thực tế nếu key được dùng ở nơi khác,
    /// vì MW không cung cấp cách tra quota còn lại qua API.
    /// </summary>
    public interface IApiUsageTracker
    {
        Task<int> IncrementAsync(string apiName, CancellationToken ct = default(CancellationToken));

        Task<int> GetTodayCountAsync(string apiName, CancellationToken ct = default(CancellationToken));
    }
}
