using System;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DictionaryTool.Core.Cache
{
    public class ApiUsageTracker : IApiUsageTracker
    {
        private readonly string _connectionString;

        public ApiUsageTracker(string dbPath = null)
        {
            var path = dbPath ?? CacheRepository.GetDefaultDbPath();

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
                        CREATE TABLE IF NOT EXISTS ApiUsage (
                            ApiName TEXT PRIMARY KEY,
                            UsageDate TEXT NOT NULL,
                            CallCount INTEGER NOT NULL
                        );";
                    command.ExecuteNonQuery();
                }
            }
        }

        public Task<int> IncrementAsync(string apiName, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                var today = DateTime.Now.ToString("yyyy-MM-dd");

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        int newCount = 1;

                        using (var select = connection.CreateCommand())
                        {
                            select.Transaction = transaction;
                            select.CommandText = "SELECT UsageDate, CallCount FROM ApiUsage WHERE ApiName = @name;";
                            select.Parameters.AddWithValue("@name", apiName);
                            using (var reader = select.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var storedDate = reader.GetString(0);
                                    var storedCount = reader.GetInt32(1);
                                    newCount = storedDate == today ? storedCount + 1 : 1;
                                }
                            }
                        }

                        using (var upsert = connection.CreateCommand())
                        {
                            upsert.Transaction = transaction;
                            upsert.CommandText = @"
                                INSERT INTO ApiUsage (ApiName, UsageDate, CallCount)
                                VALUES (@name, @date, @count)
                                ON CONFLICT(ApiName) DO UPDATE SET UsageDate = @date, CallCount = @count;";
                            upsert.Parameters.AddWithValue("@name", apiName);
                            upsert.Parameters.AddWithValue("@date", today);
                            upsert.Parameters.AddWithValue("@count", newCount);
                            upsert.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return newCount;
                    }
                }
            }, ct);
        }

        public Task<int> GetTodayCountAsync(string apiName, CancellationToken ct = default(CancellationToken))
        {
            return Task.Run(() =>
            {
                var today = DateTime.Now.ToString("yyyy-MM-dd");

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT UsageDate, CallCount FROM ApiUsage WHERE ApiName = @name;";
                        command.Parameters.AddWithValue("@name", apiName);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var storedDate = reader.GetString(0);
                                var storedCount = reader.GetInt32(1);
                                return storedDate == today ? storedCount : 0;
                            }
                            return 0;
                        }
                    }
                }
            }, ct);
        }
    }
}
