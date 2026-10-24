using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using DictionaryTool.Core.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class StardictRepositoryTests
    {
        private string _dbPath;
        private string _enViIdxPath, _enViDictDzPath;
        private string _viEnIdxPath, _viEnDictDzPath;

        [TestInitialize]
        public void Setup()
        {
            var id = Guid.NewGuid().ToString("N");
            var tmp = Path.GetTempPath();

            _dbPath = Path.Combine(tmp, "DictionaryToolTests_stardict_" + id + ".db");
            _enViIdxPath = Path.Combine(tmp, "envi_" + id + ".idx");
            _enViDictDzPath = Path.Combine(tmp, "envi_" + id + ".dict.dz");
            _viEnIdxPath = Path.Combine(tmp, "vien_" + id + ".idx");
            _viEnDictDzPath = Path.Combine(tmp, "vien_" + id + ".dict.dz");

            WriteStardictFixture(_enViIdxPath, _enViDictDzPath, new[]
            {
                ("cat", "con mèo"),
                ("cat", "con vật thuộc họ mèo"), // 2 entry trùng headword -> phải gộp lại khi import
                ("dog", "con chó")
            });

            WriteStardictFixture(_viEnIdxPath, _viEnDictDzPath, new[]
            {
                ("con mèo", "cat\nfeline"),
                ("con chó", "dog")
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var path in new[] { _dbPath, _enViIdxPath, _enViDictDzPath, _viEnIdxPath, _viEnDictDzPath })
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [TestMethod]
        public async Task LookupEnViAsync_KnownWordWithDuplicateHeadwords_MergesDefinitions()
        {
            var repo = new StardictRepository(_dbPath, _enViIdxPath, _enViDictDzPath, _viEnIdxPath, _viEnDictDzPath);
            await repo.EnsureImportedAsync();

            var result = await repo.LookupEnViAsync("cat");

            Assert.IsFalse(result.NotFound);
            Assert.AreEqual(2, result.Definitions.Count);
            CollectionAssert.Contains(result.Definitions, "con mèo");
            CollectionAssert.Contains(result.Definitions, "con vật thuộc họ mèo");
        }

        [TestMethod]
        public async Task LookupEnViAsync_IsCaseInsensitive()
        {
            var repo = new StardictRepository(_dbPath, _enViIdxPath, _enViDictDzPath, _viEnIdxPath, _viEnDictDzPath);
            await repo.EnsureImportedAsync();

            var result = await repo.LookupEnViAsync("CAT");

            Assert.IsFalse(result.NotFound);
        }

        [TestMethod]
        public async Task LookupEnViAsync_UnknownWord_ReturnsNotFound()
        {
            var repo = new StardictRepository(_dbPath, _enViIdxPath, _enViDictDzPath, _viEnIdxPath, _viEnDictDzPath);
            await repo.EnsureImportedAsync();

            var result = await repo.LookupEnViAsync("elephant");

            Assert.IsTrue(result.NotFound);
        }

        [TestMethod]
        public async Task LookupViEnAsync_MultiLineDefinition_SplitsIntoSeparateEntries()
        {
            var repo = new StardictRepository(_dbPath, _enViIdxPath, _enViDictDzPath, _viEnIdxPath, _viEnDictDzPath);
            await repo.EnsureImportedAsync();

            var result = await repo.LookupViEnAsync("con mèo");

            Assert.IsFalse(result.NotFound);
            Assert.AreEqual(2, result.Definitions.Count);
            CollectionAssert.Contains(result.Definitions, "cat");
            CollectionAssert.Contains(result.Definitions, "feline");
        }

        [TestMethod]
        public async Task LookupViEnByNoDiacriticsAsync_KnownStrippedForm_ReturnsRealHeadword()
        {
            var repo = new StardictRepository(_dbPath, _enViIdxPath, _enViDictDzPath, _viEnIdxPath, _viEnDictDzPath);
            await repo.EnsureImportedAsync();

            var result = await repo.LookupViEnByNoDiacriticsAsync("con meo");

            Assert.IsFalse(result.NotFound);
            Assert.AreEqual("con mèo", result.Word);
        }

        [TestMethod]
        public async Task LookupViEnByNoDiacriticsAsync_UnknownForm_ReturnsNotFound()
        {
            var repo = new StardictRepository(_dbPath, _enViIdxPath, _enViDictDzPath, _viEnIdxPath, _viEnDictDzPath);
            await repo.EnsureImportedAsync();

            var result = await repo.LookupViEnByNoDiacriticsAsync("con voi");

            Assert.IsTrue(result.NotFound);
        }

        [TestMethod]
        public async Task EnsureImportedAsync_CalledTwice_DoesNotDuplicateRows()
        {
            var repo = new StardictRepository(_dbPath, _enViIdxPath, _enViDictDzPath, _viEnIdxPath, _viEnDictDzPath);

            await repo.EnsureImportedAsync();
            await repo.EnsureImportedAsync();

            var result = await repo.LookupEnViAsync("cat");
            Assert.AreEqual(2, result.Definitions.Count);
        }

        private static void WriteStardictFixture(string idxPath, string dictDzPath, (string Word, string Definition)[] entries)
        {
            var dictBytes = new List<byte>();
            using (var idxStream = new MemoryStream())
            {
                foreach (var entry in entries)
                {
                    var defBytes = Encoding.UTF8.GetBytes(entry.Definition);
                    uint offset = (uint)dictBytes.Count;
                    uint length = (uint)defBytes.Length;
                    dictBytes.AddRange(defBytes);

                    var wordBytes = Encoding.UTF8.GetBytes(entry.Word);
                    idxStream.Write(wordBytes, 0, wordBytes.Length);
                    idxStream.WriteByte(0);
                    WriteUInt32BigEndian(idxStream, offset);
                    WriteUInt32BigEndian(idxStream, length);
                }

                File.WriteAllBytes(idxPath, idxStream.ToArray());
            }

            using (var fileStream = File.Create(dictDzPath))
            using (var gzipStream = new GZipStream(fileStream, CompressionMode.Compress))
            {
                var arr = dictBytes.ToArray();
                gzipStream.Write(arr, 0, arr.Length);
            }
        }

        private static void WriteUInt32BigEndian(Stream stream, uint value)
        {
            stream.WriteByte((byte)(value >> 24));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)value);
        }
    }
}
