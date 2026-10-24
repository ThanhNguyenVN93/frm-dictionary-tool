using DictionaryTool.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class VietnameseNormalizerTests
    {
        [TestMethod]
        public void StripDiacritics_RemovesToneMarks()
        {
            Assert.AreEqual("khong", VietnameseNormalizer.StripDiacritics("không"));
            Assert.AreEqual("meo", VietnameseNormalizer.StripDiacritics("mèo"));
            Assert.AreEqual("duoc", VietnameseNormalizer.StripDiacritics("được"));
        }

        [TestMethod]
        public void StripDiacritics_HandlesDStrokeSeparately()
        {
            // "đ" không tách được qua NFD (không phải tổ hợp dấu) -> phải xử lý riêng.
            Assert.AreEqual("Da Nang", VietnameseNormalizer.StripDiacritics("Đà Nẵng"));
        }

        [TestMethod]
        public void StripDiacritics_PlainAsciiUnchanged()
        {
            Assert.AreEqual("hello", VietnameseNormalizer.StripDiacritics("hello"));
        }

        [TestMethod]
        public void StripDiacritics_NullOrEmpty_ReturnsSameValue()
        {
            Assert.IsNull(VietnameseNormalizer.StripDiacritics(null));
            Assert.AreEqual("", VietnameseNormalizer.StripDiacritics(""));
        }

        [TestMethod]
        public void StripLeadingClassifier_KnownClassifier_ReturnsRemainder()
        {
            Assert.AreEqual("mèo", VietnameseNormalizer.StripLeadingClassifier("con mèo"));
            Assert.AreEqual("sách", VietnameseNormalizer.StripLeadingClassifier("quyển sách"));
        }

        [TestMethod]
        public void StripLeadingClassifier_NoClassifier_ReturnsNull()
        {
            Assert.IsNull(VietnameseNormalizer.StripLeadingClassifier("mèo"));
            Assert.IsNull(VietnameseNormalizer.StripLeadingClassifier("hello"));
        }

        [TestMethod]
        public void StripLeadingClassifier_ClassifierWithNothingAfter_ReturnsNull()
        {
            Assert.IsNull(VietnameseNormalizer.StripLeadingClassifier("con"));
            Assert.IsNull(VietnameseNormalizer.StripLeadingClassifier("con "));
        }
    }
}
