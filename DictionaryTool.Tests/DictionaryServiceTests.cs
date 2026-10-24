using System.Collections.Generic;
using System.Threading.Tasks;
using DictionaryTool.Core;
using DictionaryTool.Core.Clients;
using DictionaryTool.Core.Models;
using DictionaryTool.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class DictionaryServiceTests
    {
        [TestMethod]
        public async Task LookupAsync_CacheHit_ReturnsCachedDataWithoutCallingApis()
        {
            var cache = new FakeCacheRepository();
            var cachedResult = new WordLookupResult
            {
                Word = "hello",
                Meanings = new List<DictionaryMeaning>
                {
                    new DictionaryMeaning { PartOfSpeech = "interjection" }
                }
            };
            cache.Seed("hello", cachedResult);

            var apiDevClient = FakeDictionaryApiDevClient.ReturningNotFound();
            var mwClient = FakeMerriamWebsterClient.ReturningEmpty();
            var service = new DictionaryService(apiDevClient, mwClient, cache);

            var result = await service.LookupAsync("hello");

            Assert.IsTrue(result.FromCache);
            Assert.AreEqual("hello", result.Word);
            Assert.AreEqual(1, result.Meanings.Count);
            Assert.AreEqual("interjection", result.Meanings[0].PartOfSpeech);
            Assert.AreEqual(0, apiDevClient.CallCount);
            Assert.AreEqual(0, mwClient.CallCount);
        }

        [TestMethod]
        public async Task LookupAsync_OneSourceFails_StillReturnsDataFromTheOtherSource()
        {
            var apiDevClient = FakeDictionaryApiDevClient.ThrowingException();
            var mwClient = FakeMerriamWebsterClient.Returning(
                collegiate: new List<MwMeaning> { new MwMeaning { PartOfSpeech = "noun", ShortDefinitions = new List<string> { "a greeting" }, Source = "Collegiate" } },
                learner: new List<MwMeaning>());
            var cache = new FakeCacheRepository();
            var service = new DictionaryService(apiDevClient, mwClient, cache);

            var result = await service.LookupAsync("hello");

            Assert.IsFalse(result.HasError);
            Assert.IsFalse(result.NotFound);
            Assert.AreEqual(0, result.Meanings.Count);
            Assert.AreEqual(1, result.LearnerMeanings.Count);
            Assert.AreEqual(1, cache.SaveCallCount);
        }

        [TestMethod]
        public async Task LookupAsync_BothSourcesFail_ReturnsErrorAndDoesNotCache()
        {
            var apiDevClient = FakeDictionaryApiDevClient.ThrowingException();
            var mwClient = FakeMerriamWebsterClient.ThrowingException();
            var cache = new FakeCacheRepository();
            var service = new DictionaryService(apiDevClient, mwClient, cache);

            var result = await service.LookupAsync("hello");

            Assert.IsTrue(result.HasError);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.AreEqual(0, cache.SaveCallCount);
        }

        [TestMethod]
        public async Task LookupAsync_WordDoesNotExistInEitherSource_DoesNotThrowAndSetsNotFound()
        {
            var apiDevClient = FakeDictionaryApiDevClient.ReturningNotFound();
            var mwClient = FakeMerriamWebsterClient.ReturningEmpty();
            var cache = new FakeCacheRepository();
            var service = new DictionaryService(apiDevClient, mwClient, cache);

            var result = await service.LookupAsync("asdkjhasdkjh");

            Assert.IsFalse(result.HasError);
            Assert.IsTrue(result.NotFound);
            Assert.AreEqual(0, cache.SaveCallCount);
        }

        [TestMethod]
        public async Task LookupAsync_NormalizesWord_TrimsAndLowercases()
        {
            var apiDevClient = FakeDictionaryApiDevClient.ReturningNotFound();
            var mwClient = FakeMerriamWebsterClient.ReturningEmpty();
            var cache = new FakeCacheRepository();
            var service = new DictionaryService(apiDevClient, mwClient, cache);

            var result = await service.LookupAsync("  HeLLo  ");

            Assert.AreEqual("hello", result.Word);
        }

        [TestMethod]
        public async Task LookupAsync_CollegiateHasResults_SkipsLearnerApiCall()
        {
            var apiDevClient = FakeDictionaryApiDevClient.ReturningNotFound();
            var mwClient = FakeMerriamWebsterClient.Returning(
                collegiate: new List<MwMeaning> { new MwMeaning { PartOfSpeech = "noun", ShortDefinitions = new List<string> { "a cat" }, Source = "Collegiate" } },
                learner: new List<MwMeaning> { new MwMeaning { PartOfSpeech = "noun", ShortDefinitions = new List<string> { "a small animal" }, Source = "Learner's" } });
            var cache = new FakeCacheRepository();
            var service = new DictionaryService(apiDevClient, mwClient, cache);

            var result = await service.LookupAsync("cat");

            Assert.AreEqual(1, mwClient.CollegiateCallCount);
            Assert.AreEqual(0, mwClient.LearnerCallCount);
            Assert.AreEqual(1, result.LearnerMeanings.Count);
            Assert.AreEqual("Collegiate", result.LearnerMeanings[0].Source);
        }

        [TestMethod]
        public async Task LookupAsync_CollegiateEmpty_FallsBackToLearnerApi()
        {
            var apiDevClient = FakeDictionaryApiDevClient.ReturningNotFound();
            var mwClient = FakeMerriamWebsterClient.Returning(
                collegiate: new List<MwMeaning>(),
                learner: new List<MwMeaning> { new MwMeaning { PartOfSpeech = "noun", ShortDefinitions = new List<string> { "a small animal" }, Source = "Learner's" } });
            var cache = new FakeCacheRepository();
            var service = new DictionaryService(apiDevClient, mwClient, cache);

            var result = await service.LookupAsync("cat");

            Assert.AreEqual(1, mwClient.CollegiateCallCount);
            Assert.AreEqual(1, mwClient.LearnerCallCount);
            Assert.AreEqual(1, result.LearnerMeanings.Count);
            Assert.AreEqual("Learner's", result.LearnerMeanings[0].Source);
        }
    }
}
