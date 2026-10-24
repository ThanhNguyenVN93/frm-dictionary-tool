using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using DictionaryTool.Core.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class CcCedictRepositoryTests
    {
        private string _dbPath;
        private string _seedGzPath;

        [TestInitialize]
        public void Setup()
        {
            var id = Guid.NewGuid().ToString("N");
            _dbPath = Path.Combine(Path.GetTempPath(), "DictionaryToolTests_cedict_" + id + ".db");
            _seedGzPath = Path.Combine(Path.GetTempPath(), "DictionaryToolTests_cedict_" + id + ".txt.gz");

            var lines = new[]
            {
                "# comment line should be skipped",
                "你好 你好 [ni3 hao3] /hello/hi/",
                "貓 猫 [mao1] /cat/feline/"
            };
            WriteGzipLines(_seedGzPath, lines);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_dbPath)) File.Delete(_dbPath);
            if (File.Exists(_seedGzPath)) File.Delete(_seedGzPath);
        }

        [TestMethod]
        public async Task EnsureImportedAsync_CalledTwice_OnlyImportsOnce()
        {
            var repo = new CcCedictRepository(_dbPath, _seedGzPath);

            await repo.EnsureImportedAsync();
            await repo.EnsureImportedAsync();

            var result = await repo.LookupByChineseAsync("你好");
            Assert.IsFalse(result.NotFound);
        }

        [TestMethod]
        public async Task LookupByChineseAsync_KnownSimplifiedOrTraditional_ReturnsEntry()
        {
            var repo = new CcCedictRepository(_dbPath, _seedGzPath);
            await repo.EnsureImportedAsync();

            var bySimplified = await repo.LookupByChineseAsync("你好");
            var byTraditional = await repo.LookupByChineseAsync("貓");

            Assert.IsFalse(bySimplified.NotFound);
            Assert.AreEqual("ni3 hao3", bySimplified.Pinyin);
            CollectionAssert.Contains(bySimplified.Definitions, "hello");

            Assert.IsFalse(byTraditional.NotFound);
            Assert.AreEqual("猫", byTraditional.Simplified);
        }

        [TestMethod]
        public async Task LookupByChineseAsync_UnknownWord_ReturnsNotFound()
        {
            var repo = new CcCedictRepository(_dbPath, _seedGzPath);
            await repo.EnsureImportedAsync();

            var result = await repo.LookupByChineseAsync("不存在");

            Assert.IsTrue(result.NotFound);
        }

        [TestMethod]
        public async Task LookupByEnglishAsync_KnownGlossToken_ReturnsMatchingChinese()
        {
            var repo = new CcCedictRepository(_dbPath, _seedGzPath);
            await repo.EnsureImportedAsync();

            var result = await repo.LookupByEnglishAsync("cat");

            Assert.IsFalse(result.NotFound);
            Assert.AreEqual("猫", result.Simplified);
        }

        [TestMethod]
        public async Task LookupByEnglishAsync_UnknownToken_ReturnsNotFound()
        {
            var repo = new CcCedictRepository(_dbPath, _seedGzPath);
            await repo.EnsureImportedAsync();

            var result = await repo.LookupByEnglishAsync("spaceship");

            Assert.IsTrue(result.NotFound);
        }

        private static void WriteGzipLines(string path, string[] lines)
        {
            using (var fileStream = File.Create(path))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
            using (var writer = new StreamWriter(gzipStream, new UTF8Encoding(false)))
            {
                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                }
            }
        }
    }
}
