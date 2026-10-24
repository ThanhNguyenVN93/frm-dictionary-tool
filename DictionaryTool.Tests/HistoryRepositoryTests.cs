using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class HistoryRepositoryTests
    {
        private string _dbPath;

        [TestInitialize]
        public void Setup()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "DictionaryToolTests_history_" + Guid.NewGuid().ToString("N") + ".db");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_dbPath)) File.Delete(_dbPath);
        }

        [TestMethod]
        public async Task AddAsync_ThenGetRecentAsync_ReturnsEntry()
        {
            var repo = new HistoryRepository(_dbPath);

            await repo.AddAsync("hello", Language.English);
            var recent = await repo.GetRecentAsync();

            Assert.AreEqual(1, recent.Count);
            Assert.AreEqual("hello", recent[0].Query);
            Assert.AreEqual(Language.English, recent[0].Language);
        }

        [TestMethod]
        public async Task AddAsync_SameWordTwice_UpdatesTimestampInsteadOfDuplicating()
        {
            var repo = new HistoryRepository(_dbPath);

            await repo.AddAsync("hello", Language.English);
            await repo.AddAsync("hello", Language.English);

            var recent = await repo.GetRecentAsync();

            Assert.AreEqual(1, recent.Count);
        }

        [TestMethod]
        public async Task GetRecentAsync_MostRecentFirst()
        {
            var repo = new HistoryRepository(_dbPath);

            await repo.AddAsync("first", Language.English);
            await Task.Delay(10);
            await repo.AddAsync("second", Language.English);

            var recent = await repo.GetRecentAsync();

            Assert.AreEqual("second", recent[0].Query);
            Assert.AreEqual("first", recent[1].Query);
        }

        [TestMethod]
        public async Task GetRecentAsync_RespectsLimit()
        {
            var repo = new HistoryRepository(_dbPath);
            await repo.AddAsync("a", Language.English);
            await repo.AddAsync("b", Language.English);
            await repo.AddAsync("c", Language.English);

            var recent = await repo.GetRecentAsync(limit: 2);

            Assert.AreEqual(2, recent.Count);
        }

        [TestMethod]
        public async Task ClearAsync_RemovesAllEntries()
        {
            var repo = new HistoryRepository(_dbPath);
            await repo.AddAsync("hello", Language.English);

            await repo.ClearAsync();
            var recent = await repo.GetRecentAsync();

            Assert.AreEqual(0, recent.Count);
        }

        [TestMethod]
        public async Task SaveTrilingualResultAsync_ThenGetTrilingualResultAsync_ReturnsSameData()
        {
            var repo = new HistoryRepository(_dbPath);
            var result = new TrilingualResult
            {
                Query = "cat",
                DetectedLanguage = Language.English,
                EnglishResult = new WordLookupResult
                {
                    Word = "cat",
                    Meanings = new List<DictionaryMeaning>
                    {
                        new DictionaryMeaning
                        {
                            PartOfSpeech = "noun",
                            Definitions = new List<DictionaryDefinition>
                            {
                                new DictionaryDefinition { Definition = "a small domesticated carnivorous mammal" }
                            }
                        }
                    }
                },
                VietnameseResult = new VietnameseResult { Word = "cat", Definitions = new List<string> { "con mèo" } },
                ChineseResult = new ChineseResult { Simplified = "猫", Traditional = "貓" }
            };

            await repo.SaveTrilingualResultAsync("cat", Language.English, result);
            var loaded = await repo.GetTrilingualResultAsync("cat", Language.English);

            Assert.IsNotNull(loaded);
            Assert.AreEqual("cat", loaded.Query);
            Assert.AreEqual(Language.English, loaded.DetectedLanguage);
            Assert.AreEqual("cat", loaded.EnglishResult.Word);
            Assert.AreEqual("con mèo", loaded.VietnameseResult.Definitions[0]);
            Assert.AreEqual("猫", loaded.ChineseResult.Simplified);
        }

        [TestMethod]
        public async Task GetTrilingualResultAsync_CacheMiss_ReturnsNull()
        {
            var repo = new HistoryRepository(_dbPath);

            var loaded = await repo.GetTrilingualResultAsync("notcached", Language.English);

            Assert.IsNull(loaded);
        }

        [TestMethod]
        public async Task SaveTrilingualResultAsync_SameQueryTwice_OverwritesPrevious()
        {
            var repo = new HistoryRepository(_dbPath);

            await repo.SaveTrilingualResultAsync("cat", Language.English, new TrilingualResult { Query = "cat", DetectedLanguage = Language.English });
            await repo.SaveTrilingualResultAsync("cat", Language.English, new TrilingualResult { Query = "cat", DetectedLanguage = Language.English, ChineseResult = new ChineseResult { Simplified = "猫" } });

            var loaded = await repo.GetTrilingualResultAsync("cat", Language.English);

            Assert.IsNotNull(loaded.ChineseResult);
            Assert.AreEqual("猫", loaded.ChineseResult.Simplified);
        }
    }
}
