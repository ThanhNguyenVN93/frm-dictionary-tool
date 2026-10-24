using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DictionaryTool.Core.Clients
{
    /// <summary>
    /// Gọi Google Translate không chính thức (client=gtx, không cần API key) để
    /// dịch ngược VI→EN khi các nguồn offline (StarDict) không tìm thấy từ.
    /// Endpoint: translate.googleapis.com/translate_a/single
    /// dt=t  → bản dịch chính (arr[0][0][0])
    /// dt=bd → từ điển song ngữ: mảng [từ_loại, [nghĩa1, nghĩa2, ...]] ở arr[5..n]
    /// </summary>
    public class GoogleTranslateClient
    {
        private readonly HttpClient _httpClient;

        public GoogleTranslateClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        /// <summary>
        /// Trả về danh sách nghĩa tiếng Anh (tối đa 5) của từ tiếng Việt,
        /// hoặc list rỗng nếu không kết nối / lỗi parse.
        /// </summary>
        public async Task<List<string>> TranslateViToEnAsync(string text)
        {
            var results = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return results;

            try
            {
                var encoded = Uri.EscapeDataString(text.Trim());
                var url = "https://translate.googleapis.com/translate_a/single" +
                          "?client=gtx&sl=vi&tl=en&dt=t&dt=bd&q=" + encoded;

                var json = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
                ParseInto(json, results);
            }
            catch { /* offline hoặc rate-limited — bỏ qua */ }

            return results;
        }

        private static void ParseInto(string json, List<string> results)
        {
            JArray arr;
            try { arr = JArray.Parse(json); }
            catch { return; }

            // dt=t: arr[0][n][0] — có thể split "word1 word2" thành nhiều token
            var tBlock = arr[0] as JArray;
            if (tBlock != null)
            {
                foreach (var segment in tBlock)
                {
                    var word = segment?[0]?.Value<string>();
                    if (!string.IsNullOrWhiteSpace(word))
                        AddWords(word, results);
                }
            }

            // dt=bd: mảng ở đâu đó trong arr — tìm phần tử là JArray mà phần tử đầu
            // tiên của nó là JArray có dạng [string_từloại, JArray_nghĩa].
            foreach (var item in arr)
            {
                if (!(item is JArray outer) || outer.Count == 0) continue;
                if (!(outer[0] is JArray firstEntry)) continue;
                if (firstEntry.Count < 2) continue;
                if (firstEntry[0]?.Type != JTokenType.String) continue;
                if (!(firstEntry[1] is JArray)) continue;

                // Đây là mảng bd: [[pos, [word1, word2, ...], ...], ...]
                foreach (var posGroup in outer)
                {
                    if (!(posGroup is JArray pg) || pg.Count < 2) continue;
                    if (!(pg[1] is JArray words)) continue;
                    foreach (var w in words)
                    {
                        var word = w.Value<string>();
                        if (!string.IsNullOrWhiteSpace(word))
                            AddWords(word, results);
                        if (results.Count >= 5) return;
                    }
                }
                if (results.Count > 0) return;
            }
        }

        private static void AddWords(string phrase, List<string> results)
        {
            // Google có thể trả về "to study" hay "học sinh" — lấy từng token
            foreach (var token in phrase.Split(new[] { ' ', ',', ';', '/' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var w = token.Trim().ToLowerInvariant();
                if (w.Length >= 2 && !results.Contains(w))
                    results.Add(w);
                if (results.Count >= 5) return;
            }
        }
    }
}
