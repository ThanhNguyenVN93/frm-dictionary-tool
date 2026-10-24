using System;

namespace DictionaryTool.Core.Models
{
    public class BookmarkEntry
    {
        public string Query { get; set; }
        public Language Language { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
