using DictionaryTool.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class ClipboardWatchFilterTests
    {
        [TestMethod]
        public void ShouldTrigger_ShortNewWord_ReturnsTrue()
        {
            Assert.IsTrue(ClipboardWatchFilter.ShouldTrigger(previousText: "", newText: "hello"));
        }

        [TestMethod]
        public void ShouldTrigger_EmptyOrWhitespace_ReturnsFalse()
        {
            Assert.IsFalse(ClipboardWatchFilter.ShouldTrigger("", ""));
            Assert.IsFalse(ClipboardWatchFilter.ShouldTrigger("", "   "));
            Assert.IsFalse(ClipboardWatchFilter.ShouldTrigger("", null));
        }

        [TestMethod]
        public void ShouldTrigger_SameAsPreviousText_ReturnsFalse()
        {
            Assert.IsFalse(ClipboardWatchFilter.ShouldTrigger("hello", "hello"));
            Assert.IsFalse(ClipboardWatchFilter.ShouldTrigger("hello", "  hello  "));
        }

        [TestMethod]
        public void ShouldTrigger_TooLong_ReturnsFalse()
        {
            var longText = new string('a', ClipboardWatchFilter.MaxLength + 1);
            Assert.IsFalse(ClipboardWatchFilter.ShouldTrigger("", longText));
        }

        [TestMethod]
        public void ShouldTrigger_AtMaxLength_ReturnsTrue()
        {
            var text = new string('a', ClipboardWatchFilter.MaxLength);
            Assert.IsTrue(ClipboardWatchFilter.ShouldTrigger("", text));
        }

        [TestMethod]
        public void ShouldTrigger_MultiLineText_ReturnsFalse()
        {
            Assert.IsFalse(ClipboardWatchFilter.ShouldTrigger("", "hello\nworld"));
            Assert.IsFalse(ClipboardWatchFilter.ShouldTrigger("", "hello\r\nworld"));
        }

        [TestMethod]
        public void ShouldTrigger_TrimsWhitespaceBeforeChecking()
        {
            Assert.IsTrue(ClipboardWatchFilter.ShouldTrigger("", "  cat  "));
        }
    }
}
