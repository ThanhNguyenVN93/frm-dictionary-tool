using System.Threading;
using System.Threading.Tasks;

namespace DictionaryTool.Core.Cache
{
    public interface IAudioCacheRepository
    {
        /// <summary>
        /// Trả về nội dung audio (mp3) cho <paramref name="audioUrl"/>. Nếu đã có
        /// trong cache cục bộ thì đọc từ đĩa, không gọi mạng; nếu chưa có thì tải
        /// về, lưu cache rồi trả về.
        /// </summary>
        Task<byte[]> GetOrDownloadAsync(string audioUrl, CancellationToken ct = default(CancellationToken));
    }
}
