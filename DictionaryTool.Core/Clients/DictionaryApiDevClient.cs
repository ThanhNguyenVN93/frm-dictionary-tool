using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;
using Newtonsoft.Json;

namespace DictionaryTool.Core.Clients
{
    public class DictionaryApiDevClient : IDictionaryApiDevClient
    {
        private const string BaseUrl = "https://api.dictionaryapi.dev/api/v2/entries/en/";

        private readonly HttpClient _httpClient;

        public DictionaryApiDevClient(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<DictionaryApiDevResult> GetAsync(string word, CancellationToken ct = default(CancellationToken))
        {
            var url = BaseUrl + WebUtility.UrlEncode(word);

            using (var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false))
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // Từ không tồn tại — không phải lỗi hệ thống.
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var entries = JsonConvert.DeserializeObject<List<EntryDto>>(json);

                if (entries == null || entries.Count == 0)
                {
                    return null;
                }

                var result = new DictionaryApiDevResult();

                foreach (var entry in entries)
                {
                    if (entry.Phonetics != null)
                    {
                        // dictionaryapi.dev thường trả về 1 mục chỉ có "text" (IPA chung,
                        // không audio) và các mục khác chỉ có "audio" (theo từng giọng,
                        // không có "text" riêng) — cần gộp IPA dự phòng vào các mục có
                        // audio, nếu không sẽ mất hẳn ký hiệu IPA cho giọng đó.
                        var fallbackText = entry.Phonetics
                            .Select(p => p.Text)
                            .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                        if (string.IsNullOrWhiteSpace(fallbackText))
                        {
                            fallbackText = entry.Phonetic;
                        }

                        var audioEntries = entry.Phonetics.Where(p => !string.IsNullOrWhiteSpace(p.Audio)).ToList();

                        if (audioEntries.Count > 0)
                        {
                            foreach (var p in audioEntries)
                            {
                                result.Phonetics.Add(new Phonetic
                                {
                                    Text = string.IsNullOrWhiteSpace(p.Text) ? (fallbackText ?? string.Empty) : p.Text,
                                    AudioUrl = p.Audio,
                                    AccentLabel = InferAccentLabel(p.Audio)
                                });
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(fallbackText))
                        {
                            // Không có audio cho từ này, nhưng vẫn có IPA -> vẫn hiện text
                            // (chỉ ẩn icon loa, không ẩn cả dòng — spec 3.3).
                            result.Phonetics.Add(new Phonetic
                            {
                                Text = fallbackText,
                                AudioUrl = string.Empty,
                                AccentLabel = string.Empty
                            });
                        }
                    }

                    if (entry.Meanings != null)
                    {
                        foreach (var m in entry.Meanings)
                        {
                            var meaning = new DictionaryMeaning
                            {
                                PartOfSpeech = m.PartOfSpeech,
                                Synonyms = m.Synonyms ?? new List<string>(),
                                Antonyms = m.Antonyms ?? new List<string>()
                            };

                            if (m.Definitions != null)
                            {
                                foreach (var d in m.Definitions)
                                {
                                    meaning.Definitions.Add(new DictionaryDefinition
                                    {
                                        Definition = d.Definition,
                                        Example = d.Example
                                    });
                                }
                            }

                            result.Meanings.Add(meaning);
                        }
                    }
                }

                if (result.Phonetics.Count == 0 && result.Meanings.Count == 0)
                {
                    return null;
                }

                return result;
            }
        }

        private static string InferAccentLabel(string audioUrl)
        {
            if (audioUrl.IndexOf("-us.mp3", System.StringComparison.OrdinalIgnoreCase) >= 0
                || audioUrl.IndexOf("-us1.mp3", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "US";
            }

            if (audioUrl.IndexOf("-uk.mp3", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "UK";
            }

            if (audioUrl.IndexOf("-au.mp3", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "AU";
            }

            return string.Empty;
        }

        private class EntryDto
        {
            [JsonProperty("word")]
            public string Word { get; set; }

            [JsonProperty("phonetic")]
            public string Phonetic { get; set; }

            [JsonProperty("phonetics")]
            public List<PhoneticDto> Phonetics { get; set; }

            [JsonProperty("meanings")]
            public List<MeaningDto> Meanings { get; set; }
        }

        private class PhoneticDto
        {
            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("audio")]
            public string Audio { get; set; }
        }

        private class MeaningDto
        {
            [JsonProperty("partOfSpeech")]
            public string PartOfSpeech { get; set; }

            [JsonProperty("definitions")]
            public List<DefinitionDto> Definitions { get; set; }

            [JsonProperty("synonyms")]
            public List<string> Synonyms { get; set; }

            [JsonProperty("antonyms")]
            public List<string> Antonyms { get; set; }
        }

        private class DefinitionDto
        {
            [JsonProperty("definition")]
            public string Definition { get; set; }

            [JsonProperty("example")]
            public string Example { get; set; }
        }
    }
}
