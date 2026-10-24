using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Repositories
{
    public class BookmarkRepository : IBookmarkRepository
    {
        private readonly string _connectionString;

        public BookmarkRepository(string dbPath = null)
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
                        CREATE TABLE IF NOT EXISTS Bookmark (
                            Query TEXT NOT NULL COLLATE NOCASE,
                            Language TEXT NOT NULL,
                            CreatedAtUtc TEXT NOT NULL,
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
                            INSERT INTO Bookmark (Query, Language, CreatedAtUtc)
                            VALUES (@query, @language, @createdAt)
                            ON CONFLICT(Query, Language) DO NOTHING;";
                        command.Parameters.AddWithValue("@query", normalized);
                        command.Parameters.AddWithValue("@language", language.ToString());
                        command.Parameters.AddWithValue("@createdAt", DateTime.UtcNow.ToString("o"));
                        command.ExecuteNonQuery();
                    }
                }
            });
        }

        public Task RemoveAsync(string query, Language language)
        {
            var normalized = (query ?? string.Empty).Trim();

            return Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "DELETE FROM Bookmark WHERE Query = @query AND Language = @language;";
                        command.Parameters.AddWithValue("@query", normalized);
                        command.Parameters.AddWithValue("@language", language.ToString());
                        command.ExecuteNonQuery();
                    }
                }
            });
        }

        public Task<bool> IsBookmarkedAsync(string query, Language language)
        {
            var normalized = (query ?? string.Empty).Trim();

            return Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM Bookmark WHERE Query = @query AND Language = @language;";
                        command.Parameters.AddWithValue("@query", normalized);
                        command.Parameters.AddWithValue("@language", language.ToString());
                        return Convert.ToInt64(command.ExecuteScalar()) > 0;
                    }
                }
            });
        }

        public Task<List<BookmarkEntry>> GetAllAsync()
        {
            return Task.Run(() =>
            {
                var results = new List<BookmarkEntry>();

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Query, Language, CreatedAtUtc FROM Bookmark ORDER BY CreatedAtUtc DESC;";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(new BookmarkEntry
                                {
                                    Query = reader.GetString(0),
                                    Language = ParseLanguage(reader.GetString(1)),
                                    CreatedAtUtc = DateTime.Parse(reader.GetString(2)).ToUniversalTime()
                                });
                            }
                        }
                    }
                }

                return results;
            });
        }

        private static Language ParseLanguage(string value)
        {
            return Enum.TryParse<Language>(value, out var language) ? language : Language.Unknown;
        }
    }
}
