using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;
using Newtonsoft.Json;

namespace DictionaryTool.Core.Repositories
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly string _connectionString;

        public HistoryRepository(string dbPath = null)
        {
            var path = dbPath ?? CcCedictRepository.GetDefaultDbPath();

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connectionString = "Data Source=" + path + ";Version=3;";
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
                        CREATE TABLE IF NOT EXISTS SearchHistory (
                            Query TEXT NOT NULL COLLATE NOCASE,
                            Language TEXT NOT NULL,
                            SearchedAtUtc TEXT NOT NULL,
                            PRIMARY KEY (Query, Language)
                        );
                        CREATE TABLE IF NOT EXISTS TrilingualCache (
                            Query TEXT NOT NULL COLLATE NOCASE,
                            Language TEXT NOT NULL,
                            ResultJson TEXT NOT NULL,
                            CachedAtUtc TEXT NOT NULL,
                            PRIMARY KEY (Query, Language)
                        );";
                    command.ExecuteNonQuery();
                }
            }
        }

        public Task AddAsync(string query, Language language)
        {
            var normalized = (query ?? string.Empty).Trim();
            if (normalized.Length == 0)
            {
                return Task.CompletedTask;
            }

            return Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO SearchHistory (Query, Language, SearchedAtUtc)
                            VALUES (@query, @language, @searchedAt)
                            ON CONFLICT(Query, Language) DO UPDATE SET SearchedAtUtc = @searchedAt;";
                        command.Parameters.AddWithValue("@query", normalized);
                        command.Parameters.AddWithValue("@language", language.ToString());
                        command.Parameters.AddWithValue("@searchedAt", DateTime.UtcNow.ToString("o"));
                        command.ExecuteNonQuery();
                    }
                }
            });
        }

        public Task<List<HistoryEntry>> GetRecentAsync(int limit = 50)
        {
            return Task.Run(() =>
            {
                var results = new List<HistoryEntry>();

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Query, Language, SearchedAtUtc FROM SearchHistory ORDER BY SearchedAtUtc DESC LIMIT @limit;";
                        command.Parameters.AddWithValue("@limit", limit);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new HistoryEntry
                                {
                                    Query = reader.GetString(0),
                                    Language = ParseLanguage(reader.GetString(1)),
                                    SearchedAtUtc = DateTime.Parse(reader.GetString(2)).ToUniversalTime()
                                });
                            }
                        }
                    }
                }

                return results;
            });
        }

        public Task ClearAsync()
        {
            return Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM SearchHistory;";
                        command.ExecuteNonQuery();
                    }
                }
            });
        }

        public Task SaveTrilingualResultAsync(string query, Language language, TrilingualResult result)
        {
            var normalizedQuery = (query ?? string.Empty).Trim();
            if (normalizedQuery.Length == 0 || result == null)
                return Task.CompletedTask;

            return Task.Run(() =>
            {
                var json = JsonConvert.SerializeObject(result);
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO TrilingualCache (Query, Language, ResultJson, CachedAtUtc)
                            VALUES (@query, @language, @json, @cachedAt)
                            ON CONFLICT(Query, Language) DO UPDATE SET ResultJson = @json, CachedAtUtc = @cachedAt;";
                        command.Parameters.AddWithValue("@query", normalizedQuery);
                        command.Parameters.AddWithValue("@language", language.ToString());
                        command.Parameters.AddWithValue("@json", json);
                        command.Parameters.AddWithValue("@cachedAt", DateTime.UtcNow.ToString("o"));
                        command.ExecuteNonQuery();
                    }
                }
            });
        }

        public Task<TrilingualResult> GetTrilingualResultAsync(string query, Language language)
        {
            var normalizedQuery = (query ?? string.Empty).Trim();

            return Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT ResultJson FROM TrilingualCache WHERE Query = @query AND Language = @language;";
                        command.Parameters.AddWithValue("@query", normalizedQuery);
                        command.Parameters.AddWithValue("@language", language.ToString());

                        var json = command.ExecuteScalar() as string;
                        if (json == null) return null;

                        return JsonConvert.DeserializeObject<TrilingualResult>(json);
                    }
                }
            });
        }

        private static Language ParseLanguage(string value)
        {
            return Enum.TryParse<Language>(value, out var language) ? language : Language.Unknown;
        }
    }
}
