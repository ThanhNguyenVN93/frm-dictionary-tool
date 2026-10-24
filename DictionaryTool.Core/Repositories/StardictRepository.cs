using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Repositories
{
    /// <summary>
    /// Import + tra cứu 2 bộ Stardict bundle sẵn trong Data/stardict/en-vi và
    /// Data/stardict/vi-en (nguồn OVDP, xem StardictReader để biết format file).
    /// </summary>
    public class StardictRepository : IStardictRepository
    {
        private readonly string _connectionString;
        private readonly string _enViIdxPath;
        private readonly string _enViDictDzPath;
        private readonly string _viEnIdxPath;
        private readonly string _viEnDictDzPath;

        public StardictRepository(
            string dbPath = null,
            string enViIdxPath = null,
            string enViDictDzPath = null,
            string viEnIdxPath = null,
            string viEnDictDzPath = null)
        {
            var path = dbPath ?? CcCedictRepository.GetDefaultDbPath();

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connectionString = "Data Source=" + path + ";Version=3;";

            var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "stardict");
            _enViIdxPath = enViIdxPath ?? Path.Combine(baseDir, "en-vi", "star_anhviet.idx");
            _enViDictDzPath = enViDictDzPath ?? Path.Combine(baseDir, "en-vi", "star_anhviet.dict.dz");
            _viEnIdxPath = viEnIdxPath ?? Path.Combine(baseDir, "vi-en", "star_vietanh.idx");
            _viEnDictDzPath = viEnDictDzPath ?? Path.Combine(baseDir, "vi-en", "star_vietanh.dict.dz");

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
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS StardictEnVi (
                            Word TEXT PRIMARY KEY COLLATE NOCASE,
                            DefinitionsJson TEXT NOT NULL
                        );
                        CREATE TABLE IF NOT EXISTS StardictViEn (
                            Word TEXT PRIMARY KEY COLLATE NOCASE,
                            DefinitionsJson TEXT NOT NULL,
                            WordNoDiacritics TEXT NOT NULL DEFAULT ''
                        );";
                    command.ExecuteNonQuery();
                }

                // Nâng cấp DB cũ (tạo trước khi có cột WordNoDiacritics ở Phase 4) —
                // không có migration framework (xem SUMMARY.md), nên thêm cột kiểu
                // idempotent qua try/catch thay vì kiểm tra PRAGMA table_info. QUAN
                // TRỌNG: phải chạy TRƯỚC khi tạo index trên cột này — "CREATE TABLE
                // IF NOT EXISTS" là no-op với bảng cũ đã tồn tại (không tự thêm cột
                // mới), nên nếu tạo index ngay sau CREATE TABLE như lúc đầu sẽ lỗi
                // "no such column" trên DB có từ trước Phase 4 (phát hiện khi test
                // với bản sao DB thật, không phải DB rỗng trong unit test).
                try
                {
                    using (var alterCommand = connection.CreateCommand())
                    {
                        alterCommand.CommandText = "ALTER TABLE StardictViEn ADD COLUMN WordNoDiacritics TEXT NOT NULL DEFAULT '';";
                        alterCommand.ExecuteNonQuery();
                    }
                }
                catch (SQLiteException)
                {
                    // Cột đã tồn tại — bỏ qua.
                }

                using (var indexCommand = connection.CreateCommand())
                {
                    indexCommand.CommandText = "CREATE INDEX IF NOT EXISTS idx_stardictvien_nodiacritics ON StardictViEn(WordNoDiacritics);";
                    indexCommand.ExecuteNonQuery();
                }

                BackfillWordNoDiacritics(connection);
            }
        }

        /// <summary>
        /// DB đã import trước Phase 4 sẽ có WordNoDiacritics rỗng cho mọi dòng
        /// (giá trị DEFAULT '') — tính lại 1 lần cho các dòng đó. Không ảnh hưởng gì
        /// nếu đã backfill xong (COUNT = 0 thì return ngay).
        /// </summary>
        private static void BackfillWordNoDiacritics(SQLiteConnection connection)
        {
            using (var checkCommand = connection.CreateCommand())
            {
                checkCommand.CommandText = "SELECT COUNT(*) FROM StardictViEn WHERE WordNoDiacritics = '';";
                var missing = Convert.ToInt64(checkCommand.ExecuteScalar());
                if (missing == 0)
                {
                    return;
                }
            }

            var updates = new List<KeyValuePair<string, string>>();
            using (var selectCommand = connection.CreateCommand())
            {
                selectCommand.CommandText = "SELECT Word FROM StardictViEn WHERE WordNoDiacritics = '';";
                using (var reader = selectCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var word = reader.GetString(0);
                        updates.Add(new KeyValuePair<string, string>(word, VietnameseNormalizer.StripDiacritics(word).ToLowerInvariant()));
                    }
                }
            }

            using (var transaction = connection.BeginTransaction())
            using (var updateCommand = connection.CreateCommand())
            {
                updateCommand.CommandText = "UPDATE StardictViEn SET WordNoDiacritics = @stripped WHERE Word = @word;";
                var pStripped = updateCommand.Parameters.Add("@stripped", System.Data.DbType.String);
                var pWord = updateCommand.Parameters.Add("@word", System.Data.DbType.String);

                foreach (var kvp in updates)
                {
                    pWord.Value = kvp.Key;
                    pStripped.Value = kvp.Value;
                    updateCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }
        }

        public Task EnsureImportedAsync(IProgress<string> progress = null)
        {
            return Task.Run(() =>
            {
                using (var connection = OpenConnection())
                {
                    ImportIfEmpty(connection, "StardictEnVi", _enViIdxPath, _enViDictDzPath, progress, "Anh-Việt");
                    ImportIfEmpty(connection, "StardictViEn", _viEnIdxPath, _viEnDictDzPath, progress, "Việt-Anh");
                }
            });
        }

        private static void ImportIfEmpty(
            SQLiteConnection connection,
            string table,
            string idxPath,
            string dictDzPath,
            IProgress<string> progress,
            string label)
        {
            using (var checkCommand = connection.CreateCommand())
            {
                checkCommand.CommandText = "SELECT COUNT(*) FROM " + table + ";";
                var count = Convert.ToInt64(checkCommand.ExecuteScalar());
                if (count > 0)
                {
                    return;
                }
            }

            if (!File.Exists(idxPath) || !File.Exists(dictDzPath))
            {
                return;
            }

            progress?.Report("Đang nhập từ điển " + label + "...");

            // Nhiều entry trong .idx có thể trùng headword (nhiều nghĩa) -> gom lại
            // theo từ (không phân biệt hoa thường) trước khi insert.
            var grouped = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in StardictReader.ReadEntries(idxPath, dictDzPath))
            {
                if (!grouped.TryGetValue(entry.Key, out var list))
                {
                    list = new List<string>();
                    grouped[entry.Key] = list;
                }

                list.AddRange(ParseDefinitionSegments(entry.Value, entry.Key));
            }

            var isViEn = string.Equals(table, "StardictViEn", StringComparison.Ordinal);

            using (var transaction = connection.BeginTransaction())
            using (var insertCommand = connection.CreateCommand())
            {
                insertCommand.CommandText = isViEn
                    ? "INSERT OR REPLACE INTO " + table + " (Word, DefinitionsJson, WordNoDiacritics) VALUES (@word, @json, @noDiacritics);"
                    : "INSERT OR REPLACE INTO " + table + " (Word, DefinitionsJson) VALUES (@word, @json);";
                var pWord = insertCommand.Parameters.Add("@word", System.Data.DbType.String);
                var pJson = insertCommand.Parameters.Add("@json", System.Data.DbType.String);
                var pNoDiacritics = isViEn ? insertCommand.Parameters.Add("@noDiacritics", System.Data.DbType.String) : null;

                foreach (var kvp in grouped)
                {
                    pWord.Value = kvp.Key;
                    pJson.Value = Newtonsoft.Json.JsonConvert.SerializeObject(kvp.Value);
                    if (pNoDiacritics != null)
                    {
                        pNoDiacritics.Value = VietnameseNormalizer.StripDiacritics(kvp.Key).ToLowerInvariant();
                    }
                    insertCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            progress?.Report("Hoàn tất nhập từ điển " + label + ".");
        }

        /// <summary>
        /// Dữ liệu OVDP dùng markup nội bộ trong 1 khối text: "@headword" lặp lại
        /// đầu mục, "* loại từ" đánh dấu part-of-speech, "- nghĩa" là từng nghĩa cụ
        /// thể, các phần cách nhau bởi "|" (có thể trên cùng 1 dòng hoặc nhiều dòng).
        /// Chỉ giữ lại các đoạn "- nghĩa" làm Definitions (bỏ phần lặp headword và
        /// nhãn loại từ) để vừa hiển thị gọn vừa làm cầu nối từ tiếng Anh đáng tin
        /// cậy hơn (xem TrilingualLookupService.ExtractBridgeWord).
        /// </summary>
        private static IEnumerable<string> ParseDefinitionSegments(string rawText, string headword)
        {
            var results = new List<string>();

            foreach (var line in rawText.Split('\n'))
            {
                foreach (var rawSegment in line.Split('|'))
                {
                    var segment = rawSegment.Trim();
                    if (segment.Length == 0)
                    {
                        continue;
                    }

                    if (segment[0] == '@' || segment[0] == '*')
                    {
                        // Lặp lại headword hoặc nhãn loại từ (noun/verb/...) — bỏ qua.
                        continue;
                    }

                    if (segment[0] == '-')
                    {
                        segment = segment.Substring(1).Trim();
                    }

                    if (segment.Length > 0 && !string.Equals(segment, headword, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(segment);
                    }
                }
            }

            // Entry không theo markup "@/*/-" (vd không có ký tự "|") -> giữ nguyên
            // nguyên văn từng dòng để không mất dữ liệu.
            if (results.Count == 0)
            {
                foreach (var line in rawText.Split('\n'))
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length > 0)
                    {
                        results.Add(trimmed);
                    }
                }
            }

            return results;
        }

        public Task<StardictEntry> LookupEnViAsync(string englishWord)
        {
            return LookupAsync("StardictEnVi", englishWord);
        }

        public Task<StardictEntry> LookupViEnAsync(string vietnameseWord)
        {
            return LookupAsync("StardictViEn", vietnameseWord);
        }

        public Task<StardictEntry> LookupViEnByNoDiacriticsAsync(string wordNoDiacritics)
        {
            var normalized = (wordNoDiacritics ?? string.Empty).Trim().ToLowerInvariant();

            return Task.Run(() =>
            {
                using (var connection = OpenConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT Word, DefinitionsJson FROM StardictViEn
                            WHERE WordNoDiacritics = @word
                            ORDER BY Word ASC
                            LIMIT 1;";
                        command.Parameters.AddWithValue("@word", normalized);

                        using (var reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return new StardictEntry { Word = normalized, NotFound = true };
                            }

                            var definitions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(reader.GetString(1));
                            return new StardictEntry { Word = reader.GetString(0), Definitions = definitions, NotFound = false };
                        }
                    }
                }
            });
        }

        private Task<StardictEntry> LookupAsync(string table, string word)
        {
            var normalized = (word ?? string.Empty).Trim();

            return Task.Run(() =>
            {
                using (var connection = OpenConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT DefinitionsJson FROM " + table + " WHERE Word = @word;";
                        command.Parameters.AddWithValue("@word", normalized);

                        var json = command.ExecuteScalar() as string;
                        if (json == null)
                        {
                            return new StardictEntry { Word = normalized, NotFound = true };
                        }

                        var definitions = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);
                        return new StardictEntry { Word = normalized, Definitions = definitions, NotFound = false };
                    }
                }
            });
        }
    }
}
