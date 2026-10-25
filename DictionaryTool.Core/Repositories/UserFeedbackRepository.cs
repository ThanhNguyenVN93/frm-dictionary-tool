using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace DictionaryTool.Core.Repositories
{
    public class UserFeedbackEntry
    {
        public long   Id          { get; set; }
        public string Query       { get; set; }
        public string WrongResult { get; set; }
        public string Suggestion  { get; set; }
        public string Note        { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserFeedbackRepository
    {
        private readonly string _connectionString;

        public UserFeedbackRepository(string dbPath = null)
        {
            var path = dbPath ?? CcCedictRepository.GetDefaultDbPath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            _connectionString = "Data Source=" + path + ";Version=3;";
            EnsureCreated();
        }

        private SQLiteConnection OpenConnection()
        {
            var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;";
                cmd.ExecuteNonQuery();
            }
            return conn;
        }

        private void EnsureCreated()
        {
            using (var conn = OpenConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS UserFeedback (
                        Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        Query       TEXT NOT NULL,
                        WrongResult TEXT NOT NULL DEFAULT '',
                        Suggestion  TEXT NOT NULL DEFAULT '',
                        Note        TEXT NOT NULL DEFAULT '',
                        CreatedAt   TEXT NOT NULL
                    );";
                cmd.ExecuteNonQuery();
            }
        }

        public Task SaveAsync(string query, string wrongResult, string suggestion, string note = null)
        {
            return Task.Run(() =>
            {
                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO UserFeedback (Query, WrongResult, Suggestion, Note, CreatedAt)
                        VALUES (@q, @wr, @sug, @note, @dt);";
                    cmd.Parameters.AddWithValue("@q",    query ?? "");
                    cmd.Parameters.AddWithValue("@wr",   wrongResult ?? "");
                    cmd.Parameters.AddWithValue("@sug",  suggestion ?? "");
                    cmd.Parameters.AddWithValue("@note", note ?? "");
                    cmd.Parameters.AddWithValue("@dt",   DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public Task<List<UserFeedbackEntry>> GetAllAsync()
        {
            return Task.Run(() =>
            {
                var list = new List<UserFeedbackEntry>();
                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Query, WrongResult, Suggestion, Note, CreatedAt FROM UserFeedback ORDER BY Id DESC;";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime.TryParse(reader.GetString(5), out var dt);
                            list.Add(new UserFeedbackEntry
                            {
                                Id          = reader.GetInt64(0),
                                Query       = reader.GetString(1),
                                WrongResult = reader.GetString(2),
                                Suggestion  = reader.GetString(3),
                                Note        = reader.GetString(4),
                                CreatedAt   = dt
                            });
                        }
                    }
                }
                return list;
            });
        }

        public Task DeleteAsync(long id)
        {
            return Task.Run(() =>
            {
                using (var conn = OpenConnection())
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM UserFeedback WHERE Id = @id;";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            });
        }
    }
}
