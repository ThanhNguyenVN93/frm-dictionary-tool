using System.Threading.Tasks;
using DictionaryTool.Core;
using DictionaryTool.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class LanguageDetectorTests
    {
        [TestMethod]
        public void Detect_PlainAsciiWord_ReturnsEnglish()
        {
            Assert.AreEqual(Language.English, LanguageDetector.Detect("hello"));
        }

        [TestMethod]
        public void Detect_VietnameseWordWithDiacritics_ReturnsVietnamese()
        {
            Assert.AreEqual(Language.Vietnamese, LanguageDetector.Detect("không"));
        }

        [TestMethod]
        public void Detect_ChineseCharacters_ReturnsChinese()
        {
            Assert.AreEqual(Language.Chinese, LanguageDetector.Detect("你好"));
        }

        [TestMethod]
        public void Detect_EmptyOrWhitespace_ReturnsUnknown()
        {
            Assert.AreEqual(Language.Unknown, LanguageDetector.Detect(""));
            Assert.AreEqual(Language.Unknown, LanguageDetector.Detect("   "));
            Assert.AreEqual(Language.Unknown, LanguageDetector.Detect(null));
        }

        [TestMethod]
        public void Detect_NonDiacriticVietnamese_SyncVersionFallsBackToEnglish()
        {
            // Detect() (sync, chỉ nhìn Unicode) không có cách nào biết "khong" là
            // tiếng Việt — DetectAsync (xem các test bên dưới) xử lý case này bằng
            // cách tra ngược index.
            Assert.AreEqual(Language.English, LanguageDetector.Detect("khong"));
        }

        [TestMethod]
        public async Task DetectAsync_NoProbe_BehavesLikeSyncDetect()
        {
            Assert.AreEqual(Language.English, await LanguageDetector.DetectAsync("hello", null));
            Assert.AreEqual(Language.Vietnamese, await LanguageDetector.DetectAsync("không", null));
            Assert.AreEqual(Language.Chinese, await LanguageDetector.DetectAsync("你好", null));
        }

        [TestMethod]
        public async Task DetectAsync_ProbeFindsNonDiacriticWord_ReturnsVietnamese()
        {
            var result = await LanguageDetector.DetectAsync("khong", _ => Task.FromResult(true));

            Assert.AreEqual(Language.Vietnamese, result);
        }

        [TestMethod]
        public async Task DetectAsync_ProbeDoesNotFindWord_ReturnsEnglish()
        {
            var result = await LanguageDetector.DetectAsync("hello", _ => Task.FromResult(false));

            Assert.AreEqual(Language.English, result);
        }

        [TestMethod]
        public async Task DetectAsync_InputAlreadyHasDiacritics_DoesNotCallProbe()
        {
            var probeCalled = false;

            await LanguageDetector.DetectAsync("không", _ =>
            {
                probeCalled = true;
                return Task.FromResult(false);
            });

            Assert.IsFalse(probeCalled);
        }

        [TestMethod]
        public async Task DetectAsync_ChineseInput_DoesNotCallProbe()
        {
            var probeCalled = false;

            await LanguageDetector.DetectAsync("你好", _ =>
            {
                probeCalled = true;
                return Task.FromResult(false);
            });

            Assert.IsFalse(probeCalled);
        }
    }
}
