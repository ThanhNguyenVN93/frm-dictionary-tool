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

        public Task<MwCollegiateResult> GetCollegiateAsync(string word, CancellationToken ct = default(CancellationToken))
        {
            return GetCollegiateInternalAsync(CollegiateBaseUrl, _collegiateApiKey, word, ct);
        }

        public Task<List<MwMeaning>> GetLearnerAsync(string word, CancellationToken ct = default(CancellationToken))
        {
            return GetLearnerInternalAsync(LearnerBaseUrl, _learnerApiKey, word, ct);
        }

        private async Task<MwCollegiateResult> GetCollegiateInternalAsync(string baseUrl, string apiKey, string word, CancellationToken ct)
        {
            var result = new MwCollegiateResult();

            if (string.IsNullOrWhiteSpace(apiKey))
                return result;

            var url = baseUrl + WebUtility.UrlEncode(word) + "?key=" + WebUtility.UrlEncode(apiKey);

            if (_usageTracker != null)
                await _usageTracker.IncrementAsync("Collegiate", ct).ConfigureAwait(false);

            using (var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return result;

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var token = JToken.Parse(json);

                if (token.Type != JTokenType.Array)
                    return result;

                var array = (JArray)token;
                if (array.Count == 0 || array[0].Type == JTokenType.String)
                    return result;

                var entries = array.ToObject<List<CollegiateEntryDto>>();
                bool audioAdded = false;

                foreach (var entry in entries)
                {
                    if (entry.ShortDefinitions != null && entry.ShortDefinitions.Count > 0)
                    {
                        result.Meanings.Add(new MwMeaning
                        {
                            PartOfSpeech = entry.FunctionalLabel,
                            ShortDefinitions = entry.ShortDefinitions,
                            Source = "Collegiate"
                        });
                    }

                    if (!audioAdded && entry.Hwi?.Prs != null)
                    {
                        foreach (var pr in entry.Hwi.Prs)
                        {
                            var audioFile = pr.Sound?.Audio;
                            if (string.IsNullOrWhiteSpace(audioFile)) continue;

                            var audioUrl = BuildMwAudioUrl(audioFile);
                            result.Phonetics.Add(new Phonetic
                            {
                                Text = pr.Mw ?? string.Empty,
                                AudioUrl = audioUrl,
                                AccentLabel = "US"
                            });
                            audioAdded = true;
                            break;
                        }
                    }
                }

                return result;
            }
        }

        private async Task<List<MwMeaning>> GetLearnerInternalAsync(string baseUrl, string apiKey, string word, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return new List<MwMeaning>();

            var url = baseUrl + WebUtility.UrlEncode(word) + "?key=" + WebUtility.UrlEncode(apiKey);

            if (_usageTracker != null)
                await _usageTracker.IncrementAsync("Learner's", ct).ConfigureAwait(false);

            using (var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return new List<MwMeaning>();

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var token = JToken.Parse(json);

                if (token.Type != JTokenType.Array)
                    return new List<MwMeaning>();

                var array = (JArray)token;
                if (array.Count == 0 || array[0].Type == JTokenType.String)
                    return new List<MwMeaning>();

                var entries = array.ToObject<List<LearnerEntryDto>>();
                var result = new List<MwMeaning>();

                foreach (var entry in entries)
                {
                    if (entry.ShortDefinitions == null || entry.ShortDefinitions.Count == 0)
                        continue;

                    result.Add(new MwMeaning
                    {
                        PartOfSpeech = entry.FunctionalLabel,
                        ShortDefinitions = entry.ShortDefinitions,
                        Source = "Learner's"
                    });
                }

                return result;
            }
        }

        private static string BuildMwAudioUrl(string audioFilename)
        {
            string subdir;
            if (audioFilename.StartsWith("bix", System.StringComparison.OrdinalIgnoreCase))
                subdir = "bix";
            else if (char.IsDigit(audioFilename[0]) || audioFilename[0] == '_')
                subdir = "number";
            else
                subdir = audioFilename[0].ToString().ToLowerInvariant();

            return $"https://media.merriam-webster.com/audio/prons/en/us/mp3/{subdir}/{audioFilename}.mp3";
        }

        private class CollegiateEntryDto
        {
            [JsonProperty("fl")]
            public string FunctionalLabel { get; set; }

            [JsonProperty("shortdef")]
            public List<string> ShortDefinitions { get; set; }

            [JsonProperty("hwi")]
            public HwiDto Hwi { get; set; }
        }

        private class LearnerEntryDto
        {
            [JsonProperty("fl")]
            public string FunctionalLabel { get; set; }

            [JsonProperty("shortdef")]
            public List<string> ShortDefinitions { get; set; }
        }

        private class HwiDto
        {
            [JsonProperty("prs")]
            public List<PrDto> Prs { get; set; }
        }

        private class PrDto
        {
            [JsonProperty("mw")]
            public string Mw { get; set; }

            [JsonProperty("sound")]
            public SoundDto Sound { get; set; }
        }

        private class SoundDto
        {
            [JsonProperty("audio")]
            public string Audio { get; set; }
        }
    }
}
