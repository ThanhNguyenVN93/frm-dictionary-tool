using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Srs;

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

                // Cột SRS thêm sau — dùng ALTER TABLE trong try/catch để không lỗi với
                // DB cũ đã có bảng Bookmark (bỏ qua nếu cột đã tồn tại).
                AddColumnIfMissing(connection, "NextReviewUtc", "TEXT");
                AddColumnIfMissing(connection, "IntervalDays", "INTEGER NOT NULL DEFAULT 0");
                AddColumnIfMissing(connection, "Ease", "REAL NOT NULL DEFAULT 2.5");
                AddColumnIfMissing(connection, "Reps", "INTEGER NOT NULL DEFAULT 0");
                AddColumnIfMissing(connection, "LastReviewedUtc", "TEXT");
            }
        }

        private static void AddColumnIfMissing(SQLiteConnection connection, string column, string definition)
        {
            try
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"ALTER TABLE Bookmark ADD COLUMN {column} {definition};";
                    command.ExecuteNonQuery();
                }
            }
            catch (SQLiteException)
            {
                // Cột đã tồn tại — bỏ qua.
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
                        // Bookmark mới đến hạn ôn ngay (NextReviewUtc = thời điểm tạo).
                        command.CommandText = @"
                            INSERT INTO Bookmark (Query, Language, CreatedAtUtc, NextReviewUtc, IntervalDays, Ease, Reps)
                            VALUES (@query, @language, @createdAt, @createdAt, 0, 2.5, 0)
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
                        command.CommandText = SelectColumns + " ORDER BY CreatedAtUtc DESC;";

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(MapRow(reader));
                            }
                        }
                    }
                }

                return results;
            });
        }

        public Task<List<BookmarkEntry>> GetDueForReviewAsync(DateTime nowUtc)
        {
            var now = nowUtc.ToUniversalTime().ToString("o");

            return Task.Run(() =>
            {
                var results = new List<BookmarkEntry>();

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        // NextReviewUtc NULL (dữ liệu cũ) coi như đến hạn ngay.
                        command.CommandText = SelectColumns +
                            " WHERE NextReviewUtc IS NULL OR NextReviewUtc <= @now" +
                            " ORDER BY (NextReviewUtc IS NULL) DESC, NextReviewUtc ASC;";
                        command.Parameters.AddWithValue("@now", now);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                results.Add(MapRow(reader));
                            }
                        }
                    }
                }

                return results;
            });
        }

        public Task<int> GetDueCountAsync(DateTime nowUtc)
        {
            var now = nowUtc.ToUniversalTime().ToString("o");

            return Task.Run(() =>
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM Bookmark WHERE NextReviewUtc IS NULL OR NextReviewUtc <= @now;";
                        command.Parameters.AddWithValue("@now", now);
                        return Convert.ToInt32(command.ExecuteScalar());
                    }
                }
            });
        }

        public Task ApplyReviewAsync(string query, Language language, ReviewGrade grade, DateTime nowUtc)
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

                    int intervalDays = 0, reps = 0;
                    double ease = SrsScheduler.DefaultEase;

                    using (var read = connection.CreateCommand())
                    {
                        read.CommandText = "SELECT IntervalDays, Ease, Reps FROM Bookmark WHERE Query = @query AND Language = @language;";
                        read.Parameters.AddWithValue("@query", normalized);
                        read.Parameters.AddWithValue("@language", language.ToString());
                        using (var reader = read.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                intervalDays = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                                ease = reader.IsDBNull(1) ? SrsScheduler.DefaultEase : Convert.ToDouble(reader.GetValue(1));
                                reps = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetValue(2));
                            }
                            else
                            {
                                return; // không còn bookmark này
                            }
                        }
                    }

                    var next = SrsScheduler.Next(intervalDays, ease, reps, grade, nowUtc.ToUniversalTime());

                    using (var write = connection.CreateCommand())
                    {
                        write.CommandText = @"
                            UPDATE Bookmark
                            SET IntervalDays = @interval, Ease = @ease, Reps = @reps,
                                NextReviewUtc = @next, LastReviewedUtc = @last
                            WHERE Query = @query AND Language = @language;";
                        write.Parameters.AddWithValue("@interval", next.IntervalDays);
                        write.Parameters.AddWithValue("@ease", next.Ease);
                        write.Parameters.AddWithValue("@reps", next.Reps);
                        write.Parameters.AddWithValue("@next", next.NextReviewUtc.ToString("o"));
                        write.Parameters.AddWithValue("@last", nowUtc.ToUniversalTime().ToString("o"));
                        write.Parameters.AddWithValue("@query", normalized);
                        write.Parameters.AddWithValue("@language", language.ToString());
                        write.ExecuteNonQuery();
                    }
                }
            });
        }

        public Task<int> ImportAsync(IEnumerable<BookmarkEntry> entries, bool overwrite)
        {
            var list = (entries ?? Enumerable.Empty<BookmarkEntry>())
                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Query))
                .ToList();
            if (list.Count == 0)
            {
                return Task.FromResult(0);
            }

            return Task.Run(() =>
            {
                // overwrite: cập nhật cả tiến độ SRS của từ đã có; ngược lại chỉ thêm mới.
                var conflict = overwrite
                    ? @"ON CONFLICT(Query, Language) DO UPDATE SET
                            CreatedAtUtc = excluded.CreatedAtUtc,
                            NextReviewUtc = excluded.NextReviewUtc,
                            IntervalDays = excluded.IntervalDays,
                            Ease = excluded.Ease,
                            Reps = excluded.Reps,
                            LastReviewedUtc = excluded.LastReviewedUtc"
                    : "ON CONFLICT(Query, Language) DO NOTHING";

                var affected = 0;
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (var entry in list)
                        {
                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;
                                command.CommandText =
                                    "INSERT INTO Bookmark (Query, Language, CreatedAtUtc, NextReviewUtc, IntervalDays, Ease, Reps, LastReviewedUtc) " +
                                    "VALUES (@query, @language, @created, @next, @interval, @ease, @reps, @last) " + conflict + ";";
                                command.Parameters.AddWithValue("@query", entry.Query.Trim());
                                command.Parameters.AddWithValue("@language", entry.Language.ToString());
                                command.Parameters.AddWithValue("@created",
                                    (entry.CreatedAtUtc == default(DateTime) ? DateTime.UtcNow : entry.CreatedAtUtc).ToUniversalTime().ToString("o"));
                                command.Parameters.AddWithValue("@next",
                                    (object)entry.NextReviewUtc?.ToUniversalTime().ToString("o") ?? DBNull.Value);
                                command.Parameters.AddWithValue("@interval", entry.IntervalDays);
                                command.Parameters.AddWithValue("@ease", entry.Ease <= 0 ? SrsScheduler.DefaultEase : entry.Ease);
                                command.Parameters.AddWithValue("@reps", entry.Reps);
                                command.Parameters.AddWithValue("@last",
                                    (object)entry.LastReviewedUtc?.ToUniversalTime().ToString("o") ?? DBNull.Value);
                                affected += command.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
                return affected;
            });
        }

        private const string SelectColumns =
            "SELECT Query, Language, CreatedAtUtc, NextReviewUtc, IntervalDays, Ease, Reps, LastReviewedUtc FROM Bookmark";

        private static BookmarkEntry MapRow(SQLiteDataReader reader)
        {
            return new BookmarkEntry
            {
                Query = reader.GetString(0),
                Language = ParseLanguage(reader.GetString(1)),
                CreatedAtUtc = DateTime.Parse(reader.GetString(2)).ToUniversalTime(),
                NextReviewUtc = reader.IsDBNull(3) ? (DateTime?)null : DateTime.Parse(reader.GetString(3)).ToUniversalTime(),
                IntervalDays = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetValue(4)),
                Ease = reader.IsDBNull(5) ? SrsScheduler.DefaultEase : Convert.ToDouble(reader.GetValue(5)),
                Reps = reader.IsDBNull(6) ? 0 : Convert.ToInt32(reader.GetValue(6)),
                LastReviewedUtc = reader.IsDBNull(7) ? (DateTime?)null : DateTime.Parse(reader.GetString(7)).ToUniversalTime()
            };
        }

        private static Language ParseLanguage(string value)
        {
            return Enum.TryParse<Language>(value, out var language) ? language : Language.Unknown;
        }
    }
}
