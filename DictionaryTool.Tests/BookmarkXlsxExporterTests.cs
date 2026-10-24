using System;
using System.Collections.Generic;
using System.IO;
using DictionaryTool.Core.Export;
using DictionaryTool.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;

namespace DictionaryTool.Tests
{
    [TestClass]
    public class BookmarkXlsxExporterTests
    {
        [TestMethod]
        public void Export_WritesHeaderAndRows()
        {
            var exporter = new BookmarkXlsxExporter();
            var bookmarks = new List<BookmarkEntry>
            {
                new BookmarkEntry { Query = "hello", Language = Language.English, CreatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new BookmarkEntry { Query = "mèo", Language = Language.Vietnamese, CreatedAtUtc = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc) }
            };

            var bytes = exporter.Export(bookmarks);

            Assert.IsTrue(bytes.Length > 0);

            using (var ms = new MemoryStream(bytes))
            using (var package = new ExcelPackage(ms))
            {
                var sheet = package.Workbook.Worksheets[0];

                Assert.AreEqual("Từ", sheet.Cells[1, 1].Text);
                Assert.AreEqual("hello", sheet.Cells[2, 1].Text);
                Assert.AreEqual("English", sheet.Cells[2, 2].Text);
                Assert.AreEqual("mèo", sheet.Cells[3, 1].Text);
                Assert.AreEqual("Vietnamese", sheet.Cells[3, 2].Text);
            }
        }

        [TestMethod]
        public void Export_EmptyList_StillProducesValidWorkbookWithHeaderOnly()
        {
            var exporter = new BookmarkXlsxExporter();

            var bytes = exporter.Export(new List<BookmarkEntry>());

            using (var ms = new MemoryStream(bytes))
            using (var package = new ExcelPackage(ms))
            {
                var sheet = package.Workbook.Worksheets[0];
                Assert.AreEqual("Từ", sheet.Cells[1, 1].Text);
                Assert.AreEqual(string.Empty, sheet.Cells[2, 1].Text);
            }
        }
    }
}
