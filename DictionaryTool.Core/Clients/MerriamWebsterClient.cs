using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DictionaryTool.Core.Cache;
using DictionaryTool.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DictionaryTool.Core.Clients
{
    /// <summary>
    /// Client cho 2 reference API miễn phí của Merriam-Webster (dictionaryapi.com):
    /// Collegiate Dictionary và Learner's Dictionary. Mỗi API đăng ký riêng, có key
    /// riêng, mỗi key giới hạn 1000 query/ngày (xem spec 2.5) — vì vậy 2 key được
    /// cấu hình độc lập thay vì dùng chung 1 key.
    /// </summary>
    public class MerriamWebsterClient : IMerriamWebsterClient
    {
        private const string CollegiateBaseUrl = "https://www.dictionaryapi.com/api/v3/references/collegiate/json/";
        private const string LearnerBaseUrl = "https://www.dictionaryapi.com/api/v3/references/learners/json/";

        private readonly string _collegiateApiKey;
        private readonly string _learnerApiKey;
        private readonly HttpClient _httpClient;
        private readonly IApiUsageTracker _usageTracker;

        public MerriamWebsterClient(string collegiateApiKey, string learnerApiKey, HttpClient httpClient = null, IApiUsageTracker usageTracker = null)
        {
            _collegiateApiKey = collegiateApiKey;
            _learnerApiKey = learnerApiKey;
            _httpClient = httpClient ?? new HttpClient();
            _usageTracker = usageTracker;
        }

        public Task<List<MwMeaning>> GetCollegiateAsync(string word, CancellationToken ct = default(CancellationToken))
        {
            return GetAsync(CollegiateBaseUrl, _collegiateApiKey, "Collegiate", word, ct);
        }

        public Task<List<MwMeaning>> GetLearnerAsync(string word, CancellationToken ct = default(CancellationToken))
        {
            return GetAsync(LearnerBaseUrl, _learnerApiKey, "Learner's", word, ct);
        }

        private async Task<List<MwMeaning>> GetAsync(string baseUrl, string apiKey, string sourceLabel, string word, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // Key chưa được cấu hình -> coi như nguồn này không có dữ liệu, không gọi
                // API (tránh tốn quota với key rỗng/không hợp lệ) và không throw.
                return new List<MwMeaning>();
            }

            var url = baseUrl + WebUtility.UrlEncode(word) + "?key=" + WebUtility.UrlEncode(apiKey);

            if (_usageTracker != null)
            {
                // Chỉ đếm khi thực sự gửi request lên MW (key hợp lệ, không phải cache
                // hit — cache hit không bao giờ chạy tới đây).
                await _usageTracker.IncrementAsync(sourceLabel, ct).ConfigureAwait(false);
            }

            using (var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new List<MwMeaning>();
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var token = JToken.Parse(json);

                if (token.Type != JTokenType.Array)
                {
                    return new List<MwMeaning>();
                }

                var array = (JArray)token;
                if (array.Count == 0)
                {
                    return new List<MwMeaning>();
                }

                // Nếu phần tử đầu tiên là string -> đây là mảng gợi ý chính tả, nghĩa là
                // từ không tồn tại trong từ điển này (spec 2.5), không phải lỗi.
                if (array[0].Type == JTokenType.String)
                {
                    return new List<MwMeaning>();
                }

                var entries = array.ToObject<List<EntryDto>>();
                var result = new List<MwMeaning>();

                foreach (var entry in entries)
                {
                    if (entry.ShortDefinitions == null || entry.ShortDefinitions.Count == 0)
                    {
                        continue;
                    }

                    result.Add(new MwMeaning
                    {
                        PartOfSpeech = entry.FunctionalLabel,
                        ShortDefinitions = entry.ShortDefinitions,
                        Source = sourceLabel
                    });
                }

                return result;
            }
        }

        private class EntryDto
        {
            [JsonProperty("fl")]
            public string FunctionalLabel { get; set; }

            [JsonProperty("shortdef")]
            public List<string> ShortDefinitions { get; set; }
        }
    }
}
