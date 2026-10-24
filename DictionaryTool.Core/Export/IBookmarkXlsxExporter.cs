using System.Collections.Generic;
using DictionaryTool.Core.Models;

namespace DictionaryTool.Core.Export
{
    public interface IBookmarkXlsxExporter
    {
        /// <summary>Tạo nội dung file .xlsx (bytes) từ danh sách bookmark, chưa ghi ra đĩa.</summary>
        byte[] Export(IEnumerable<BookmarkEntry> bookmarks);
    }
}
