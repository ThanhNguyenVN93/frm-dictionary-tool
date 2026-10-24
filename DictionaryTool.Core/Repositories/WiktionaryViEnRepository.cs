using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;
using Newtonsoft.Json.Linq;

namespace DictionaryTool.Core.Repositories
{
    /// <summary>
    /// Tra từ tiếng Việt qua Wiktionary tiếng Anh (en.wiktionary.org), cache kết
    /// quả vào SQLite để không phải gọi lại API cho cùng 1 từ. Sắp xếp trong
    /// fallback chain sau tất cả nguồn offline, trước Google Translate.
    ///
    /// API: action=parse&amp;prop=wikitext — trả về wikitext nguyên gốc.
    /// Parser tìm section ==Vietnamese== rồi lấy các dòng định nghĩa "# ..." dưới
    /// các part-of-speech heading (===Noun===, ===Verb===, ...).
    /// </summary>
    public class WiktionaryViEnRepository
    {
        // Thời gian cache định nghĩa tìm thấy (30 ngày)
        private static readonly TimeSpan FoundExpiry = TimeSpan.FromDays(30);
        // Thời gian cache từ không có trong Wiktionary (7 ngày, tránh gọi lại liên tục)
        private static readonly TimeSpan NotFoundExpiry = TimeSpan.FromDays(7);

        private readonly string _connectionString;
        private readonly HttpClient _httpClient;

        // ==Vietnamese== đến hết section (dừng ở == cùng level tiếp theo)
        private static readonly Regex VietnameseSectionRegex = new Regex(
            @"==Vietnamese==(.*?)(?=\n==[^=]|\z)",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // Dòng định nghĩa "# ..." (không bắt "#: example" hay "## sub-def")
        private static readonly Regex DefinitionLineRegex = new Regex(
            @"^# (.+)$",
            RegexOptions.Multiline | RegexOptions.Compiled);

        public WiktionaryViEnRepository(string dbPath = null, HttpClient httpClient = null)
        {
            var path = dbPath ?? CcCedictRepository.GetDefaultDbPath();
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            _connectionString = "Data Source=" + path + ";Version=3;";
            _httpClient = httpClient;

            EnsureCreated();
        }

        private SQLiteConnection OpenConnection()
        {
            var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;";
                cmd.ExecuteNonQuery();
            }
            return connection;
        }

        private void EnsureCreated()
        {
            using (var connection = OpenConnection())
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS WiktionaryViEnCache (
                        Word        TEXT PRIMARY KEY COLLATE NOCASE,
                        DefsJson    TEXT NOT NULL,
                        FetchedAt   TEXT NOT NULL
                    );";
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Trả về entry từ cache hoặc gọi Wiktionary API.
        /// Trả về NotFound = true nếu: offline, API lỗi, hoặc không có section Vietnamese.
        /// </summary>
        public async Task<StardictEntry> LookupAsync(string word)
        {
            var trimmed = (word ?? string.Empty).Trim();
            if (trimmed.Length == 0)
                return new StardictEntry { Word = trimmed, NotFound = true };

            var cached = TryGetFromCache(trimmed);
            if (cached != null)
                return cached;

            if (_httpClient == null)
                return new StardictEntry { Word = trimmed, NotFound = true };

            var definitions = await FetchFromWiktionaryAsync(trimmed).ConfigureAwait(false);
            SaveToCache(trimmed, definitions);

            if (definitions == null || definitions.Count == 0)
                return new StardictEntry { Word = trimmed, NotFound = true };

            return new StardictEntry { Word = trimmed, Definitions = definitions, NotFound = false };
        }

        private StardictEntry TryGetFromCache(string word)
        {
            try
            {
                using (var connection = OpenConnection())
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT DefsJson, FetchedAt FROM WiktionaryViEnCache WHERE Word = @word;";
                    cmd.Parameters.AddWithValue("@word", word);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;

                        var defsJson = reader.GetString(0);
                        if (!DateTime.TryParse(reader.GetString(1), out var fetchedAt))
                            return null;

                        var defs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(defsJson);
                        var isFound = defs != null && defs.Count > 0;
                        var expiry = isFound ? FoundExpiry : NotFoundExpiry;

                        if (DateTime.UtcNow - fetchedAt > expiry)
                            return null; // hết hạn, cần fetch lại

                        if (!isFound)
                            return new StardictEntry { Word = word, NotFound = true };

                        return new StardictEntry { Word = word, Definitions = defs, NotFound = false };
                    }
                }
            }
            catch { return null; }
        }

        private void SaveToCache(string word, List<string> definitions)
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(definitions ?? new List<string>());
                using (var connection = OpenConnection())
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT OR REPLACE INTO WiktionaryViEnCache (Word, DefsJson, FetchedAt)
                        VALUES (@word, @json, @fetchedAt);";
                    cmd.Parameters.AddWithValue("@word", word);
                    cmd.Parameters.AddWithValue("@json", json);
                    cmd.Parameters.AddWithValue("@fetchedAt", DateTime.UtcNow.ToString("O"));
                    cmd.ExecuteNonQuery();
                }
            }
            catch { /* không cần fail nếu cache ghi lỗi */ }
        }

        private async Task<List<string>> FetchFromWiktionaryAsync(string word)
        {
            try
            {
                var encoded = Uri.EscapeDataString(word);
                var url = "https://en.wiktionary.org/w/api.php" +
                          "?action=parse&prop=wikitext&format=json&redirects=1&page=" + encoded;

                var json = await _httpClient.GetStringAsync(url).ConfigureAwait(false);
                return ParseWikitextResponse(json);
            }
            catch { return new List<string>(); }
        }

        private static List<string> ParseWikitextResponse(string apiJson)
        {
            var results = new List<string>();
            try
            {
                var obj = JObject.Parse(apiJson);
                if (obj["error"] != null) return results;

                var wikitext = obj["parse"]?["wikitext"]?["*"]?.Value<string>();
                if (string.IsNullOrEmpty(wikitext)) return results;

                var viSection = VietnameseSectionRegex.Match(wikitext);
                if (!viSection.Success) return results;

                foreach (Match defMatch in DefinitionLineRegex.Matches(viSection.Groups[1].Value))
                {
                    var def = CleanWikiMarkup(defMatch.Groups[1].Value.Trim());
                    if (!string.IsNullOrWhiteSpace(def))
                        results.Add(def);
                }
            }
            catch { }
            return results;
        }

        private static string CleanWikiMarkup(string text)
        {
            // [[word|display]] → display
            text = Regex.Replace(text, @"\[\[(?:[^\]|]+\|)?([^\]]+)\]\]", "$1");
            // {{gloss|text}} → (text)
            text = Regex.Replace(text, @"\{\{gloss\|([^}]+)\}\}", "($1)");
            // {{lb|vi|...}} và các template khác → xóa
            text = Regex.Replace(text, @"\{\{[^}]+\}\}", "");
            // '''bold''' → bold
            text = Regex.Replace(text, @"'''([^']+)'''", "$1");
            // ''italic'' → italic
            text = Regex.Replace(text, @"''([^']+)''", "$1");
            // <ref>...</ref>
            text = Regex.Replace(text, @"<ref[^>]*>.*?</ref>", "", RegexOptions.Singleline);
            // HTML tags
            text = Regex.Replace(text, @"<[^>]+>", "");
            // Multiple spaces
            text = Regex.Replace(text, @"\s{2,}", " ");
            return text.Trim();
        }
    }
}
