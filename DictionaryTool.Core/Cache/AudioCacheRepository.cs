using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DictionaryTool.Core.Cache
{
    /// <summary>
    /// Cache audio phát âm dạng file trên đĩa (không dùng SQLite — audio là dữ
    /// liệu nhị phân lớn, lưu file trực tiếp đơn giản và hiệu quả hơn nhét BLOB
    /// vào DB). Key cache = MD5 của URL, vì URL đã là định danh duy nhất cho mỗi
    /// bản ghi âm và không đổi giữa các lần tra cùng 1 từ.
    /// </summary>
    public class AudioCacheRepository : IAudioCacheRepository
    {
        private readonly string _cacheDirectory;
        private readonly HttpClient _httpClient;

        public AudioCacheRepository(HttpClient httpClient = null, string cacheDirectory = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _cacheDirectory = cacheDirectory ?? GetDefaultCacheDirectory();
            Directory.CreateDirectory(_cacheDirectory);
        }

        public static string GetDefaultCacheDirectory()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "DictionaryTool", "audio");
        }

        public async Task<byte[]> GetOrDownloadAsync(string audioUrl, CancellationToken ct = default(CancellationToken))
        {
            var filePath = GetCacheFilePath(audioUrl);

            if (File.Exists(filePath))
            {
                return await Task.Run(() => File.ReadAllBytes(filePath), ct).ConfigureAwait(false);
            }

            using (var response = await _httpClient.GetAsync(audioUrl, ct).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                await Task.Run(() => File.WriteAllBytes(filePath, data), ct).ConfigureAwait(false);
                return data;
            }
        }

        private string GetCacheFilePath(string audioUrl)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(audioUrl));
                var hex = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
                return Path.Combine(_cacheDirectory, hex + ".mp3");
            }
        }
    }
}
