using System.Collections.Generic;
using DictionaryTool.Core.Models;
using OfficeOpenXml;

namespace DictionaryTool.Core.Export
{
    /// <summary>Xuất danh sách bookmark ra file Excel (.xlsx) bằng EPPlus (spec: "Export bookmark ra file XLS").</summary>
    public class BookmarkXlsxExporter : IBookmarkXlsxExporter
    {
        public BookmarkXlsxExporter()
        {
            // EPPlus >=5 yêu cầu khai báo license context — dùng cho mục đích phi thương mại.
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public byte[] Export(IEnumerable<BookmarkEntry> bookmarks)
        {
            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Bookmarks");

                sheet.Cells[1, 1].Value = "Từ";
                sheet.Cells[1, 2].Value = "Ngôn ngữ";
                sheet.Cells[1, 3].Value = "Ngày thêm";
                sheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;

                var row = 2;
                foreach (var bookmark in bookmarks)
                {
                    sheet.Cells[row, 1].Value = bookmark.Query;
                    sheet.Cells[row, 2].Value = bookmark.Language.ToString();
                    sheet.Cells[row, 3].Value = bookmark.CreatedAtUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                    row++;
                }

                if (row > 2)
                {
                    sheet.Cells[1, 1, row - 1, 3].AutoFitColumns();
                }

                return package.GetAsByteArray();
            }
        }
    }
}
