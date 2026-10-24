using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DictionaryTool.Core.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class TatoebaRepositoryTests
    {
        private string _dbPath;
        private string _enViSeedPath;
        private string _enZhSeedPath;

        [TestInitialize]
        public void Setup()
        {
            var id = Guid.NewGuid().ToString("N");
            var tmp = Path.GetTempPath();

            _dbPath = Path.Combine(tmp, "DictionaryToolTests_tatoeba_" + id + ".db");
            _enViSeedPath = Path.Combine(tmp, "envi_" + id + ".tsv.gz");
            _enZhSeedPath = Path.Combine(tmp, "enzh_" + id + ".tsv.gz");

            WriteGzipLines(_enViSeedPath, new[]
            {
                "1\tI have a cat.\t101\tTôi có một con mèo.",
                "2\tThe dog is sleeping.\t102\tCon chó đang ngủ."
            });

            WriteGzipLines(_enZhSeedPath, new[]
            {
                "1\tI have a cat.\t201\t我有一只猫。",
                "3\tCats are cute.\t202\t猫很可爱。"
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var path in new[] { _dbPath, _enViSeedPath, _enZhSeedPath })
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [TestMethod]
        public async Task GetExamplesForEnglishAsync_SentenceSharedByBothPairs_JoinsOnEnId()
        {
            var repo = new TatoebaRepository(_dbPath, _enViSeedPath, _enZhSeedPath);
            await repo.EnsureImportedAsync();

            var examples = await repo.GetExamplesForEnglishAsync("cat");

            var joined = examples.FirstOrDefault(e => e.English == "I have a cat.");
            Assert.IsNotNull(joined);
            Assert.AreEqual("Tôi có một con mèo.", joined.Vietnamese);
            Assert.AreEqual("我有一只猫。", joined.Chinese);
        }

        [TestMethod]
        public async Task GetExamplesForEnglishAsync_EnZhOnlySentence_HasNullVietnamese()
        {
            var repo = new TatoebaRepository(_dbPath, _enViSeedPath, _enZhSeedPath);
            await repo.EnsureImportedAsync();

            var examples = await repo.GetExamplesForEnglishAsync("cute");

            var zhOnly = examples.FirstOrDefault(e => e.English == "Cats are cute.");
            Assert.IsNotNull(zhOnly);
            Assert.IsNull(zhOnly.Vietnamese);
            Assert.AreEqual("猫很可爱。", zhOnly.Chinese);
        }

        [TestMethod]
        public async Task GetExamplesForEnglishAsync_RespectsLimit()
        {
            var repo = new TatoebaRepository(_dbPath, _enViSeedPath, _enZhSeedPath);
            await repo.EnsureImportedAsync();

            var examples = await repo.GetExamplesForEnglishAsync("a", limit: 1);

            Assert.IsTrue(examples.Count <= 1);
        }

        [TestMethod]
        public async Task GetExamplesForEnglishAsync_NoMatch_ReturnsEmptyList()
        {
            var repo = new TatoebaRepository(_dbPath, _enViSeedPath, _enZhSeedPath);
            await repo.EnsureImportedAsync();

            var examples = await repo.GetExamplesForEnglishAsync("spaceship");

            Assert.AreEqual(0, examples.Count);
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
