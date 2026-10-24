using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using DictionaryTool.Core.Cache;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class CacheRepositoryTests
    {
        private string _dbPath;
        private string _historyDbPath;

        [TestInitialize]
        public void Setup()
        {
            var id = Guid.NewGuid().ToString("N");
            _dbPath = Path.Combine(Path.GetTempPath(), "DictionaryToolTests_cache_" + id + ".db");
            _historyDbPath = Path.Combine(Path.GetTempPath(), "DictionaryToolTests_history_" + id + ".db");
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var path in new[] { _dbPath, _historyDbPath })
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [TestMethod]
        public async Task TryGetAsync_Miss_ReturnsNull()
        {
            var repo = new CacheRepository(_dbPath);

            var result = await repo.TryGetAsync("doesnotexist");

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task SaveThenTryGet_CacheHit_ReturnsSameData()
        {
            var repo = new CacheRepository(_dbPath);
            var word = "hello";
            var original = new WordLookupResult
            {
                Word = word,
                Phonetics = new List<Phonetic>
                {
                    new Phonetic { Text = "/hu0259u02c8lou028a/", AudioUrl = "https://example.com/hello.mp3", AccentLabel = "US" }
                },
                Meanings = new List<DictionaryMeaning>
                {
                    new DictionaryMeaning
                    {
                        PartOfSpeech = "interjection",
                        Definitions = new List<DictionaryDefinition>
                        {
                            new DictionaryDefinition { Definition = "used as a greeting", Example = "Hello, everyone." }
                        }
                    }
                }
            };

            await repo.SaveAsync(word, original);
            var cached = await repo.TryGetAsync(word);

            Assert.IsNotNull(cached);
            Assert.IsTrue(cached.FromCache);
            Assert.AreEqual(word, cached.Word);
            Assert.AreEqual(1, cached.Phonetics.Count);
            Assert.AreEqual("https://example.com/hello.mp3", cached.Phonetics[0].AudioUrl);
            Assert.AreEqual(1, cached.Meanings.Count);
            Assert.AreEqual("used as a greeting", cached.Meanings[0].Definitions[0].Definition);
        }

        [TestMethod]
        public async Task SaveAsync_SameWordTwice_Overwrites()
        {
            var repo = new CacheRepository(_dbPath);
            var word = "hello";

            await repo.SaveAsync(word, new WordLookupResult { Word = word, ErrorMessage = "first" });
            await repo.SaveAsync(word, new WordLookupResult { Word = word, ErrorMessage = "second" });

            var cached = await repo.TryGetAsync(word);

            Assert.AreEqual("second", cached.ErrorMessage);
        }

        [TestMethod]
        public async Task GetAllCachedViWordsAsync_ReturnsVietnameseQueriesFromSearchHistory()
        {
            // Dùng HistoryRepository để seed SearchHistory trong _historyDbPath
            var historyRepo = new HistoryRepository(_historyDbPath);
            await historyRepo.AddAsync("con mèo", Language.Vietnamese);
            await historyRepo.AddAsync("con chó", Language.Vietnamese);
            await historyRepo.AddAsync("cat", Language.English); // EN không được trả

            var repo = new CacheRepository(_dbPath, _historyDbPath);
            var result = await repo.GetAllCachedViWordsAsync();

            Assert.AreEqual(2, result.Count);
            CollectionAssert.Contains(result, "con mèo");
            CollectionAssert.Contains(result, "con chó");
        }

        [TestMethod]
        public async Task GetAllCachedZhWordsAsync_ReturnsChineseQueriesFromSearchHistory()
        {
            var historyRepo = new HistoryRepository(_historyDbPath);
            await historyRepo.AddAsync("猫", Language.Chinese);
            await historyRepo.AddAsync("khong", Language.Vietnamese); // VI không được trả

            var repo = new CacheRepository(_dbPath, _historyDbPath);
            var result = await repo.GetAllCachedZhWordsAsync();

            Assert.AreEqual(1, result.Count);
            CollectionAssert.Contains(result, "猫");
        }

        [TestMethod]
        public async Task GetAllCachedViWordsAsync_EmptyHistory_ReturnsEmptyList()
        {
            // historyDbPath trỏ đến file chưa tồn tại (SearchHistory chưa có) —
            // CacheRepository bắt SQLiteException, không throw ra ngoài.
            var repo = new CacheRepository(_dbPath, _historyDbPath);
            var result = await repo.GetAllCachedViWordsAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }
    }
}
