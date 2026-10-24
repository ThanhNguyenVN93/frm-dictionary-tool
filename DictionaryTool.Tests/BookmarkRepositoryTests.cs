using System;
using System.IO;
using System.Threading.Tasks;
using DictionaryTool.Core.Models;
using DictionaryTool.Core.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class BookmarkRepositoryTests
    {
        private string _dbPath;

        [TestInitialize]
        public void Setup()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "DictionaryToolTests_bookmark_" + Guid.NewGuid().ToString("N") + ".db");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_dbPath)) File.Delete(_dbPath);
        }

        [TestMethod]
        public async Task IsBookmarkedAsync_NotYetAdded_ReturnsFalse()
        {
            var repo = new BookmarkRepository(_dbPath);

            Assert.IsFalse(await repo.IsBookmarkedAsync("hello", Language.English));
        }

        [TestMethod]
        public async Task AddAsync_ThenIsBookmarkedAsync_ReturnsTrue()
        {
            var repo = new BookmarkRepository(_dbPath);

            await repo.AddAsync("hello", Language.English);

            Assert.IsTrue(await repo.IsBookmarkedAsync("hello", Language.English));
        }

        [TestMethod]
        public async Task AddAsync_SameWordTwice_DoesNotDuplicate()
        {
            var repo = new BookmarkRepository(_dbPath);

            await repo.AddAsync("hello", Language.English);
            await repo.AddAsync("hello", Language.English);

            var all = await repo.GetAllAsync();
            Assert.AreEqual(1, all.Count);
        }

        [TestMethod]
        public async Task RemoveAsync_RemovesBookmark()
        {
            var repo = new BookmarkRepository(_dbPath);
            await repo.AddAsync("hello", Language.English);

            await repo.RemoveAsync("hello", Language.English);

            Assert.IsFalse(await repo.IsBookmarkedAsync("hello", Language.English));
        }

        [TestMethod]
        public async Task SameWordDifferentLanguage_TrackedIndependently()
        {
            var repo = new BookmarkRepository(_dbPath);

            await repo.AddAsync("to", Language.English);

            Assert.IsTrue(await repo.IsBookmarkedAsync("to", Language.English));
            Assert.IsFalse(await repo.IsBookmarkedAsync("to", Language.Vietnamese));
        }

        [TestMethod]
        public async Task GetAllAsync_MostRecentlyAddedFirst()
        {
            var repo = new BookmarkRepository(_dbPath);

            await repo.AddAsync("first", Language.English);
            await Task.Delay(10);
            await repo.AddAsync("second", Language.English);

            var all = await repo.GetAllAsync();

            Assert.AreEqual("second", all[0].Query);
            Assert.AreEqual("first", all[1].Query);
        }
    }
}
