using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DictionaryTool.Core.Cache;
using DictionaryTool.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class AudioCacheRepositoryTests
    {
        private string _cacheDir;

        [TestInitialize]
        public void Setup()
        {
            _cacheDir = Path.Combine(Path.GetTempPath(), "DictionaryToolAudioTests_" + Guid.NewGuid().ToString("N"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_cacheDir))
            {
                Directory.Delete(_cacheDir, true);
            }
        }

        [TestMethod]
        public async Task GetOrDownloadAsync_SecondCallForSameUrl_ReadsFromDiskWithoutNetworkCall()
        {
            var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "fake-mp3-bytes");
            var httpClient = new HttpClient(handler);
            var repo = new AudioCacheRepository(httpClient, _cacheDir);
            const string url = "https://example.com/hello-us.mp3";

            var first = await repo.GetOrDownloadAsync(url);
            var second = await repo.GetOrDownloadAsync(url);

            Assert.AreEqual(1, handler.CallCount, "Lần tra thứ 2 phải đọc từ cache đĩa, không gọi mạng lại.");
            CollectionAssert.AreEqual(first, second);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("fake-mp3-bytes"), first);
        }

        [TestMethod]
        public async Task GetOrDownloadAsync_WritesCacheFileToDisk()
        {
            var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "fake-mp3-bytes");
            var httpClient = new HttpClient(handler);
            var repo = new AudioCacheRepository(httpClient, _cacheDir);

            await repo.GetOrDownloadAsync("https://example.com/hello-us.mp3");

            var files = Directory.GetFiles(_cacheDir, "*.mp3");
            Assert.AreEqual(1, files.Length);
        }
    }
}
