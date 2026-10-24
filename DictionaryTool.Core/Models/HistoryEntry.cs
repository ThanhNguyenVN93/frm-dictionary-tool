using System;

namespace DictionaryTool.Core.Models
{
    public class HistoryEntry
    {
        public string Query { get; set; }
        public Language Language { get; set; }
        public DateTime SearchedAtUtc { get; set; }
    }
}
