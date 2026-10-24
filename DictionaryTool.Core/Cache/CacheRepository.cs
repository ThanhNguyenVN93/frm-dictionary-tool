using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;
using Newtonsoft.Json;

namespace DictionaryTool.Core.Cache
{
    /// <summary>
    /// Đọc/ghi WordCache trong SQLite. Đây là nơi duy nhất thao tác SQL trực tiếp
    /// (spec 2.6) — DictionaryService chỉ gọi qua ICacheRepository.
    /// </summary>
    public class CacheRepository : ICacheRepository
    {
        private readonly string _connectionString;
        private readonly string _historyConnectionString;

        public CacheRepository(string dbPath = null, string historyDbPath = null)
        {
            var path = dbPath ?? GetDefaultDbPath();

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connectionString = "Data Source=" + path + ";Version=3;";

            var hPath = historyDbPath ?? Repositories.CcCedictRepository.GetDefaultDbPath();
            _historyConnectionString = "Data Source=" + hPath + ";Version=3;";

            EnsureCreated();
        }

        public static string GetDefaultDbPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "DictionaryTool", "cache.db");
        }

        private void EnsureCreated()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS WordCache (
                            Word TEXT PRIMARY KEY,
                            ResultJson TEXT NOT NULL,
                            CreatedAt TEXT NOT NULL
                        );";
                    command.ExecuteNonQuery();
                }
            }
        }

        public Task<WordLookupResult> TryGetAsync(string normalizedWord)
        {
            return Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT ResultJson FROM WordCache WHERE Word = @word;";
                        command.Parameters.AddWithValue("@word", normalizedWord);

                        var json = command.ExecuteScalar() as string;
                        if (json == null)
                        {
                            return null;
                        }

                        var result = JsonConvert.DeserializeObject<WordLookupResult>(json);
                        result.FromCache = true;
                        return result;
                    }
                }
            });
        }

        public Task SaveAsync(string normalizedWord, WordLookupResult result)
        {
            return Task.Run(() =>
            {
                var json = JsonConvert.SerializeObject(result);

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO WordCache (Word, ResultJson, CreatedAt)
                            VALUES (@word, @json, @createdAt)
                            ON CONFLICT(Word) DO UPDATE SET ResultJson = @json, CreatedAt = @createdAt;";
                        command.Parameters.AddWithValue("@word", normalizedWord);
                        command.Parameters.AddWithValue("@json", json);
                        command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("o"));
                        command.ExecuteNonQuery();
                    }
                }
            });
        }

        public Task<List<string>> GetAllCachedWordsAsync()
        {
            return Task.Run(() =>
            {
                var words = new List<string>();

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Word FROM WordCache ORDER BY Word;";
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                words.Add(reader.GetString(0));
                            }
                        }
                    }
                }

                return words;
            });
        }

        public Task<List<string>> GetAllCachedViWordsAsync()
        {
            return GetAllCachedWordsByLanguageAsync("Vietnamese");
        }

        public Task<List<string>> GetAllCachedZhWordsAsync()
        {
            return GetAllCachedWordsByLanguageAsync("Chinese");
        }

        private Task<List<string>> GetAllCachedWordsByLanguageAsync(string language)
        {
            return Task.Run(() =>
            {
                var words = new List<string>();

                try
                {
                    using (var connection = new SQLiteConnection(_historyConnectionString))
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "SELECT Query FROM SearchHistory WHERE Language = @language ORDER BY Query;";
                            command.Parameters.AddWithValue("@language", language);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    words.Add(reader.GetString(0));
                                }
                            }
                        }
                    }
                }
                catch (SQLiteException)
                {
                    // SearchHistory chưa tồn tại (DB mới, chưa tra lần nào) — trả danh sách rỗng.
                }

                return words;
            });
        }
    }
}
