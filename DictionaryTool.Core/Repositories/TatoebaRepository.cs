using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Repositories
{
    /// <summary>
    /// Import + tra câu ví dụ từ 2 file Tatoeba "sentence pairs" bundle sẵn
    /// (Data/tatoeba/tatoeba_en_vi.tsv.gz và tatoeba_en_zh.tsv.gz, lấy từ công cụ
    /// export của chính Tatoeba, mỗi dòng: EnId\tEnText\tOtherId\tOtherText).
    /// Giới hạn: chỉ có câu nối qua tiếng Anh — không có cặp VI-ZH trực tiếp,
    /// khớp với thiết kế chain EN làm cầu nối của toàn bộ TrilingualLookupService.
    /// </summary>
    public class TatoebaRepository : ITatoebaRepository
    {
        private readonly string _connectionString;
        private readonly string _enViSeedPath;
        private readonly string _enZhSeedPath;

        public TatoebaRepository(string dbPath = null, string enViSeedPath = null, string enZhSeedPath = null)
        {
            var path = dbPath ?? CcCedictRepository.GetDefaultDbPath();

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connectionString = "Data Source=" + path + ";Version=3;";

            var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "tatoeba");
            _enViSeedPath = enViSeedPath ?? Path.Combine(baseDir, "tatoeba_en_vi.tsv.gz");
            _enZhSeedPath = enZhSeedPath ?? Path.Combine(baseDir, "tatoeba_en_zh.tsv.gz");

            EnsureCreated();
        }

        private void EnsureCreated()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS TatoebaEnVi (
                            EnId INTEGER NOT NULL,
                            EnText TEXT NOT NULL,
                            EnTextLower TEXT NOT NULL,
                            ViText TEXT NOT NULL
                        );
                        CREATE INDEX IF NOT EXISTS idx_tatoeba_envi_lower ON TatoebaEnVi(EnTextLower);
                        CREATE INDEX IF NOT EXISTS idx_tatoeba_envi_enid ON TatoebaEnVi(EnId);

                        CREATE TABLE IF NOT EXISTS TatoebaEnZh (
                            EnId INTEGER NOT NULL,
                            EnText TEXT NOT NULL,
                            EnTextLower TEXT NOT NULL,
                            ZhText TEXT NOT NULL
                        );
                        CREATE INDEX IF NOT EXISTS idx_tatoeba_enzh_lower ON TatoebaEnZh(EnTextLower);
                        CREATE INDEX IF NOT EXISTS idx_tatoeba_enzh_enid ON TatoebaEnZh(EnId);";
                    command.ExecuteNonQuery();
                }
            }
        }

        public Task EnsureImportedAsync(IProgress<string> progress = null)
        {
            return Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    ImportIfEmpty(connection, "TatoebaEnVi", "ViText", _enViSeedPath, progress, "EN-VI");
                    ImportIfEmpty(connection, "TatoebaEnZh", "ZhText", _enZhSeedPath, progress, "EN-ZH");
                }
            });
        }

        private static void ImportIfEmpty(
            SQLiteConnection connection,
            string table,
            string otherTextColumn,
            string seedPath,
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

            if (!File.Exists(seedPath))
            {
                return;
            }

            progress?.Report("Đang nhập câu ví dụ " + label + "...");

            using (var transaction = connection.BeginTransaction())
            using (var insertCommand = connection.CreateCommand())
            {
                insertCommand.CommandText =
                    "INSERT INTO " + table + " (EnId, EnText, EnTextLower, " + otherTextColumn + ") " +
                    "VALUES (@enId, @enText, @enTextLower, @otherText);";
                var pEnId = insertCommand.Parameters.Add("@enId", System.Data.DbType.Int64);
                var pEnText = insertCommand.Parameters.Add("@enText", System.Data.DbType.String);
                var pEnTextLower = insertCommand.Parameters.Add("@enTextLower", System.Data.DbType.String);
                var pOtherText = insertCommand.Parameters.Add("@otherText", System.Data.DbType.String);

                foreach (var line in ReadGzipLines(seedPath))
                {
                    var columns = line.Split('\t');
                    if (columns.Length < 4)
                    {
                        continue;
                    }

                    if (!long.TryParse(columns[0], out var enId))
                    {
                        continue;
                    }

                    var enText = columns[1];
                    var otherText = columns[3];

                    pEnId.Value = enId;
                    pEnText.Value = enText;
                    pEnTextLower.Value = enText.ToLowerInvariant();
                    pOtherText.Value = otherText;
                    insertCommand.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            progress?.Report("Hoàn tất nhập câu ví dụ " + label + ".");
        }

        public Task<List<ExampleSentence>> GetExamplesForEnglishAsync(string englishWord, int limit = 20)
        {
            var pattern = "%" + (englishWord ?? string.Empty).Trim().ToLowerInvariant() + "%";

            return Task.Run(() =>
            {
                var results = new List<ExampleSentence>();
                var seenEnIds = new HashSet<long>();

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT v.EnId, v.EnText, v.ViText, z.ZhText
                            FROM TatoebaEnVi v
                            LEFT JOIN TatoebaEnZh z ON z.EnId = v.EnId
                            WHERE v.EnTextLower LIKE @pattern
                            LIMIT @limit;";
                        command.Parameters.AddWithValue("@pattern", pattern);
                        command.Parameters.AddWithValue("@limit", limit);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                seenEnIds.Add(reader.GetInt64(0));
                                results.Add(new ExampleSentence
                                {
                                    English = reader.GetString(1),
                                    Vietnamese = reader.GetString(2),
                                    Chinese = reader.IsDBNull(3) ? null : reader.GetString(3)
                                });
                            }
                        }
                    }

                    if (results.Count < limit)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                SELECT EnId, EnText, ZhText
                                FROM TatoebaEnZh
                                WHERE EnTextLower LIKE @pattern
                                LIMIT @limit;";
                            command.Parameters.AddWithValue("@pattern", pattern);
                            command.Parameters.AddWithValue("@limit", limit);

                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read() && results.Count < limit)
                                {
                                    var enId = reader.GetInt64(0);
                                    if (seenEnIds.Contains(enId))
                                    {
                                        continue;
                                    }

                                    results.Add(new ExampleSentence
                                    {
                                        English = reader.GetString(1),
                                        Vietnamese = null,
                                        Chinese = reader.GetString(2)
                                    });
                                }
                            }
                        }
                    }
                }

                return results;
            });
        }

        private static IEnumerable<string> ReadGzipLines(string path)
        {
            using (var fileStream = File.OpenRead(path))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzipStream, System.Text.Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
