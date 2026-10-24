using System.Collections.Generic;
using System.Threading.Tasks;
using DictionaryTool.Core;
using DictionaryTool.Core.Models;
using DictionaryTool.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class TrilingualLookupServiceTests
    {
        private FakeDictionaryService _dictionaryService;
        private FakeCcCedictRepository _cedictRepository;
        private FakeStardictRepository _stardictRepository;
        private FakeTatoebaRepository _tatoebaRepository;
        private TrilingualLookupService _service;

        [TestInitialize]
        public void Setup()
        {
            _dictionaryService = new FakeDictionaryService();
            _cedictRepository = new FakeCcCedictRepository();
            _stardictRepository = new FakeStardictRepository();
            _tatoebaRepository = new FakeTatoebaRepository();
            _service = new TrilingualLookupService(_dictionaryService, _cedictRepository, _stardictRepository, _tatoebaRepository);
        }

        [TestMethod]
        public async Task LookupAsync_EnglishInput_DetectsEnglishAndFillsAllColumns()
        {
            _dictionaryService.Seed("cat", new WordLookupResult { Word = "cat" });
            _stardictRepository.SeedEnVi("cat", new StardictEntry { Word = "cat", Definitions = new List<string> { "con mèo" } });
            _cedictRepository.SeedByEnglish("cat", new ChineseResult { Simplified = "猫", Traditional = "貓" });
            _tatoebaRepository.Seed("cat", new List<ExampleSentence> { new ExampleSentence { English = "I have a cat." } });

            var result = await _service.LookupAsync("cat");

            Assert.AreEqual(Language.English, result.DetectedLanguage);
            Assert.AreEqual("cat", result.EnglishResult.Word);
            Assert.AreEqual("con mèo", result.VietnameseResult.Definitions[0]);
            Assert.AreEqual("猫", result.ChineseResult.Simplified);
            Assert.AreEqual(1, result.ExampleSentences.Count);
        }

        [TestMethod]
        public async Task LookupAsync_VietnameseInput_BridgesThroughEnglishToFillEnAndZh()
        {
            _stardictRepository.SeedViEn("con mèo", new StardictEntry { Word = "con mèo", Definitions = new List<string> { "cat" } });
            _dictionaryService.Seed("cat", new WordLookupResult { Word = "cat" });
            _cedictRepository.SeedByEnglish("cat", new ChineseResult { Simplified = "猫" });

            var result = await _service.LookupAsync("con mèo");

            Assert.AreEqual(Language.Vietnamese, result.DetectedLanguage);
            Assert.AreEqual("con mèo", result.VietnameseResult.Word);
            Assert.IsNotNull(result.EnglishResult);
            Assert.AreEqual("cat", result.EnglishResult.Word);
            Assert.AreEqual("猫", result.ChineseResult.Simplified);
            Assert.AreEqual(1, _dictionaryService.CallCount);
        }

        [TestMethod]
        public async Task LookupAsync_ChineseInput_BridgesThroughEnglishToFillEnAndVi()
        {
            _cedictRepository.SeedByChinese("猫", new ChineseResult { Simplified = "猫", Traditional = "貓", Definitions = new List<string> { "cat" } });
            _dictionaryService.Seed("cat", new WordLookupResult { Word = "cat" });
            _stardictRepository.SeedEnVi("cat", new StardictEntry { Word = "cat", Definitions = new List<string> { "con mèo" } });

            var result = await _service.LookupAsync("猫");

            Assert.AreEqual(Language.Chinese, result.DetectedLanguage);
            Assert.AreEqual("猫", result.ChineseResult.Simplified);
            Assert.IsNotNull(result.EnglishResult);
            Assert.AreEqual("cat", result.EnglishResult.Word);
            Assert.AreEqual("con mèo", result.VietnameseResult.Definitions[0]);
        }

        [TestMethod]
        public async Task LookupAsync_VietnameseInput_SkipsLeadingInfinitiveMarkerWhenExtractingBridgeWord()
        {
            // Định nghĩa Stardict VI-EN thường viết kiểu "To greet, to salute" — nếu lấy
            // nguyên token đầu tiên sẽ bắc cầu nhầm sang "to" (giới từ) thay vì "greet".
            _stardictRepository.SeedViEn("chào", new StardictEntry { Word = "chào", Definitions = new List<string> { "To greet, to salute" } });
            _dictionaryService.Seed("greet", new WordLookupResult { Word = "greet" });

            var result = await _service.LookupAsync("chào");

            Assert.AreEqual("greet", result.EnglishResult.Word);
        }

        [TestMethod]
        public async Task ResolveVietnamese_PrefersExactMatchOverClassifierStrip()
        {
            _stardictRepository.SeedViEn("con dao", new StardictEntry { Word = "con dao", Definitions = new List<string> { "blade" } });
            _stardictRepository.SeedViEn("dao", new StardictEntry { Word = "dao", Definitions = new List<string> { "knife" } });

            var result = await _service.LookupAsync("con dao");

            Assert.AreEqual("con dao", result.VietnameseResult.Word);
            Assert.AreEqual("blade", result.VietnameseResult.Definitions[0]);
        }

        [TestMethod]
        public async Task ResolveVietnamese_FallsBackToClassifierStripWhenExactMissing()
        {
            _stardictRepository.SeedViEn("mèo", new StardictEntry { Word = "mèo", Definitions = new List<string> { "cat" } });

            var result = await _service.LookupAsync("con mèo");

            Assert.AreEqual("mèo", result.VietnameseResult.Word);
        }

        [TestMethod]
        public async Task LookupAsync_NonDiacriticVietnameseInput_DetectsAndResolvesViaNoDiacriticsIndex()
        {
            _stardictRepository.SeedViEnNoDiacritics("khong", new StardictEntry { Word = "không", Definitions = new List<string> { "no, not" } });

            var result = await _service.LookupAsync("khong");

            Assert.AreEqual(Language.Vietnamese, result.DetectedLanguage);
            Assert.AreEqual("không", result.VietnameseResult.Word);
        }

        [TestMethod]
        public async Task LookupAsync_NonDiacriticInputWithClassifier_ResolvesViaClassifierAndNoDiacriticsCombined()
        {
            _stardictRepository.SeedViEnNoDiacritics("meo", new StardictEntry { Word = "mèo", Definitions = new List<string> { "cat" } });

            var result = await _service.LookupAsync("con meo");

            Assert.AreEqual(Language.Vietnamese, result.DetectedLanguage);
            Assert.AreEqual("mèo", result.VietnameseResult.Word);
        }

        [TestMethod]
        public async Task LookupAsync_NonDiacriticInputNotInAnyIndex_FallsBackToEnglish()
        {
            var result = await _service.LookupAsync("xyznotaword");

            Assert.AreEqual(Language.English, result.DetectedLanguage);
        }

        [TestMethod]
        public async Task LookupAsync_VietnameseWordNotInDictionary_DoesNotBridge()
        {
            // Stardict VI-EN mặc định trả NotFound cho từ chưa seed -> không có cầu nối,
            // EnglishResult/ChineseResult phải giữ nguyên null, không gọi nhầm nguồn khác.
            var result = await _service.LookupAsync("từkhôngtồntại");

            Assert.AreEqual(Language.Vietnamese, result.DetectedLanguage);
            Assert.IsTrue(result.VietnameseResult.NotFound);
            Assert.IsNull(result.EnglishResult);
            Assert.IsNull(result.ChineseResult);
            Assert.AreEqual(0, _dictionaryService.CallCount);
        }
    }
}
