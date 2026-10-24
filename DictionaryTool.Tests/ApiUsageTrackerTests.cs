using System;
using System.IO;
using System.Threading.Tasks;
using DictionaryTool.Core.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class ApiUsageTrackerTests
    {
        private string _dbPath;

        [TestInitialize]
        public void Setup()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), "DictionaryToolUsageTests_" + Guid.NewGuid().ToString("N") + ".db");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
            }
        }

        [TestMethod]
        public async Task GetTodayCountAsync_NoCallsYet_ReturnsZero()
        {
            var tracker = new ApiUsageTracker(_dbPath);

            var count = await tracker.GetTodayCountAsync("Collegiate");

            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task IncrementAsync_MultipleCalls_CountsSequentially()
        {
            var tracker = new ApiUsageTracker(_dbPath);

            var first = await tracker.IncrementAsync("Collegiate");
            var second = await tracker.IncrementAsync("Collegiate");
            var third = await tracker.IncrementAsync("Collegiate");

            Assert.AreEqual(1, first);
            Assert.AreEqual(2, second);
            Assert.AreEqual(3, third);
            Assert.AreEqual(3, await tracker.GetTodayCountAsync("Collegiate"));
        }

        [TestMethod]
        public async Task IncrementAsync_DifferentApiNames_CountedIndependently()
        {
            var tracker = new ApiUsageTracker(_dbPath);

            await tracker.IncrementAsync("Collegiate");
            await tracker.IncrementAsync("Collegiate");
            await tracker.IncrementAsync("Learner's");

            Assert.AreEqual(2, await tracker.GetTodayCountAsync("Collegiate"));
            Assert.AreEqual(1, await tracker.GetTodayCountAsync("Learner's"));
        }
    }
}
