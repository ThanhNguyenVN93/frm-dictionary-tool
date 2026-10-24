using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Repositories
{
    /// <summary>
    /// Đọc file CC-CEDICT (bundle sẵn dạng .gz trong Data/cedict.txt.gz), import vào
    /// SQLite 1 lần duy nhất (idempotent, xem EnsureImportedAsync), sau đó tra cứu
    /// qua SQLite. Format mỗi dòng dữ liệu: "TRAD SIMP [pinyin] /gloss1/gloss2/.../".
    /// </summary>
    public class CcCedictRepository : ICcCedictRepository
    {
        private static readonly Regex LineRegex = new Regex(
            @"^(?<trad>\S+)\s+(?<simp>\S+)\s+\[(?<pinyin>[^\]]*)\]\s+/(?<glosses>.+)/\s*$",
            RegexOptions.Compiled);

        private static readonly Regex TokenRegex = new Regex(@"[a-zA-Z']+", RegexOptions.Compiled);

        private readonly string _connectionString;
        private readonly string _seedGzPath;

        public CcCedictRepository(string dbPath = null, string seedGzPath = null)
        {
            var path = dbPath ?? GetDefaultDbPath();

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connectionString = "Data Source=" + path + ";Version=3;";
            _seedGzPath = seedGzPath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "cedict.txt.gz");

            EnsureCreated();
        }

        public static string GetDefaultDbPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "DictionaryTool", "trilingual.db");
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
                        CREATE TABLE IF NOT EXISTS CcCedict (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Traditional TEXT NOT NULL,
                            Simplified TEXT NOT NULL,
                            Pinyin TEXT NOT NULL,
                            GlossJson TEXT NOT NULL
                        );
                        CREATE INDEX IF NOT EXISTS idx_cccedict_traditional ON CcCedict(Traditional);
                        CREATE INDEX IF NOT EXISTS idx_cccedict_simplified ON CcCedict(Simplified);

                        CREATE TABLE IF NOT EXISTS CcCedictEnglishIndex (
                            EnglishToken TEXT NOT NULL,
                            CcCedictId INTEGER NOT NULL
                        );
                        CREATE INDEX IF NOT EXISTS idx_cccedict_en_token ON CcCedictEnglishIndex(EnglishToken);";
                    command.ExecuteNonQuery();
                }
            }
        }

        public Task EnsureImportedAsync(IProgress<string> progress = null)
        {
            return Task.Run(() =>
            {
                using (var connection = OpenConnection())
                {
                    using (var checkCommand = connection.CreateCommand())
                    {
                        checkCommand.CommandText = "SELECT COUNT(*) FROM CcCedict;";
                        var count = Convert.ToInt64(checkCommand.ExecuteScalar());
                        if (count > 0)
                        {
                            return;
                        }
                    }

                    if (!File.Exists(_seedGzPath))
                    {
                        // Không có file seed -> bỏ qua import, repository chỉ đơn giản
                        // không trả về kết quả nào (không coi là lỗi hệ thống).
                        return;
                    }

                    progress?.Report("Đang nhập dữ liệu CC-CEDICT...");

                    using (var transaction = connection.BeginTransaction())
                    {
                        using (var insertEntry = connection.CreateCommand())
                        using (var insertToken = connection.CreateCommand())
                        {
                            insertEntry.CommandText = @"
                                INSERT INTO CcCedict (Traditional, Simplified, Pinyin, GlossJson)
                                VALUES (@trad, @simp, @pinyin, @glossJson);
                                SELECT last_insert_rowid();";
                            var pTrad = insertEntry.Parameters.Add("@trad", System.Data.DbType.String);
                            var pSimp = insertEntry.Parameters.Add("@simp", System.Data.DbType.String);
                            var pPinyin = insertEntry.Parameters.Add("@pinyin", System.Data.DbType.String);
                            var pGlossJson = insertEntry.Parameters.Add("@glossJson", System.Data.DbType.String);

                            insertToken.CommandText = "INSERT INTO CcCedictEnglishIndex (EnglishToken, CcCedictId) VALUES (@token, @id);";
                            var pToken = insertToken.Parameters.Add("@token", System.Data.DbType.String);
                            var pId = insertToken.Parameters.Add("@id", System.Data.DbType.Int64);

                            foreach (var line in ReadGzipLines(_seedGzPath))
                            {
                                if (line.Length == 0 || line[0] == '#')
                                {
                                    continue;
                                }

                                var match = LineRegex.Match(line);
                                if (!match.Success)
                                {
                                    continue;
                                }

                                var glosses = match.Groups["glosses"].Value.Split('/');

                                pTrad.Value = match.Groups["trad"].Value;
                                pSimp.Value = match.Groups["simp"].Value;
                                pPinyin.Value = match.Groups["pinyin"].Value;
                                pGlossJson.Value = Newtonsoft.Json.JsonConvert.SerializeObject(glosses);

                                var id = Convert.ToInt64(insertEntry.ExecuteScalar());

                                pId.Value = id;
                                foreach (var token in TokenizeEnglish(glosses))
                                {
                                    pToken.Value = token;
                                    insertToken.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }

                    progress?.Report("Hoàn tất nhập dữ liệu CC-CEDICT.");
                }
            });
        }

        public Task<ChineseResult> LookupByChineseAsync(string word)
        {
            var normalized = (word ?? string.Empty).Trim();

            return Task.Run(() =>
            {
                using (var connection = OpenConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT Traditional, Simplified, Pinyin, GlossJson FROM CcCedict
                            WHERE Traditional = @word OR Simplified = @word;";
                        command.Parameters.AddWithValue("@word", normalized);

                        return BuildResult(normalized, command);
                    }
                }
            });
        }

        public Task<ChineseResult> LookupByEnglishAsync(string englishWord)
        {
            var token = (englishWord ?? string.Empty).Trim().ToLowerInvariant();

            return Task.Run(() =>
            {
                using (var connection = OpenConnection())
                {
                    using (var command = connection.CreateCommand())
                    {
                        // Lấy nhiều ứng viên rồi xếp hạng trong code (xem ScoreGlossMatch) thay vì
                        // LIMIT 1 trực tiếp trong SQL — token index chỉ khớp theo từ đơn lẻ trong
                        // gloss, nên 1 từ ít gặp nhưng ngắn (vd "咪" / "sound for calling a cat")
                        // có thể trùng token "cat" dù không phải nghĩa chính xác của "cat".
                        // Không LIMIT: số entry trùng 1 token tiếng Anh thường chỉ vài chục,
                        // cần quét hết để ScoreGlossMatch chọn đúng nghĩa phổ biến nhất
                        // (xem PickBestEnglishMatch) thay vì dừng sớm và bỏ lỡ ứng viên tốt hơn.
                        command.CommandText = @"
                            SELECT c.Traditional, c.Simplified, c.Pinyin, c.GlossJson
                            FROM CcCedictEnglishIndex i
                            JOIN CcCedict c ON c.Id = i.CcCedictId
                            WHERE i.EnglishToken = @token;";
                        command.Parameters.AddWithValue("@token", token);

                        return PickBestEnglishMatch(token, command);
                    }
                }
            });
        }

        private static ChineseResult PickBestEnglishMatch(string token, SQLiteCommand command)
        {
            ChineseResult best = null;
            var bestScore = -1;
            var bestSimplifiedLength = int.MaxValue;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var traditional = reader.GetString(0);
                    var simplified = reader.GetString(1);
                    var pinyin = reader.GetString(2);
                    var glosses = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(reader.GetString(3));

                    var score = ScoreGlossMatch(glosses, token);
                    if (score > bestScore || (score == bestScore && simplified.Length < bestSimplifiedLength))
                    {
                        best = new ChineseResult
                        {
                            Traditional = traditional,
                            Simplified = simplified,
                            Pinyin = pinyin,
                            Definitions = new List<string>(glosses),
                            NotFound = false
                        };
                        bestScore = score;
                        bestSimplifiedLength = simplified.Length;
                    }
                }
            }

            return best ?? new ChineseResult { Traditional = token, Simplified = token, NotFound = true };
        }

        /// <summary>
        /// Chấm điểm mức độ khớp của 1 entry CC-CEDICT với từ tiếng Anh cần tra ngược:
        /// 3 = gloss khớp hoàn toàn với từ cần tra VÀ không có chú thích trong ngoặc
        ///     (nghĩa chính, phổ biến — vd 學生 "pupil" không ngoặc);
        /// 2 = gloss khớp hoàn toàn nhưng có chú thích trong ngoặc làm hẹp nghĩa
        ///     (vd 眸 "pupil (of the eye)" — đúng token nhưng nghĩa chuyên biệt);
        /// 1 = chỉ khớp 1 token rời rạc bên trong gloss dài hơn (vd "sound for calling a cat");
        /// 0 = không khớp.
        /// </summary>
        private static int ScoreGlossMatch(string[] glosses, string token)
        {
            var best = 0;
            foreach (var gloss in glosses)
            {
                var cleaned = Regex.Replace(gloss, @"\(.*?\)", string.Empty).Trim();
                var firstClause = cleaned.Split(new[] { ',', ';' }, 2)[0].Trim();

                if (string.Equals(firstClause, token, StringComparison.OrdinalIgnoreCase))
                {
                    if (!gloss.Contains('('))
                        return 3; // primary meaning, no qualifier — max score
                    if (best < 2)
                        best = 2;
                }
                else if (best < 1)
                {
                    best = 1;
                }
            }
            return best;
        }

        private static ChineseResult BuildResult(string query, SQLiteCommand command)
        {
            using (var reader = command.ExecuteReader())
            {
                var result = new ChineseResult { Traditional = query, Simplified = query };
                var pinyins = new List<string>();
                var definitions = new List<string>();
                var found = false;

                while (reader.Read())
                {
                    found = true;
                    result.Traditional = reader.GetString(0);
                    result.Simplified = reader.GetString(1);

                    var pinyin = reader.GetString(2);
                    if (!pinyins.Contains(pinyin))
                    {
                        pinyins.Add(pinyin);
                    }

                    var glosses = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(reader.GetString(3));
                    definitions.AddRange(glosses.Where(g => !string.IsNullOrWhiteSpace(g)));
                }

                result.Pinyin = string.Join("; ", pinyins);
                result.Definitions = definitions;
                result.NotFound = !found;
                return result;
            }
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

        private static IEnumerable<string> TokenizeEnglish(string[] glosses)
        {
            var tokens = new HashSet<string>();
            foreach (var gloss in glosses)
            {
                foreach (Match m in TokenRegex.Matches(gloss))
                {
                    var token = m.Value.ToLowerInvariant();
                    if (token.Length >= 2)
                    {
                        tokens.Add(token);
                    }
                }
            }
            return tokens;
        }
    }
}
