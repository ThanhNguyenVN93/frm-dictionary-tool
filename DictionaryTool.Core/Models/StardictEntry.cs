using System.Collections.Generic;

namespace DictionaryTool.Core.Models
{
    /// <summary>Kết quả tra 1 từ trong 1 bộ từ điển Stardict (EN-VI hoặc VI-EN).</summary>
    public class StardictEntry
    {
        public string Word { get; set; }
        public List<string> Definitions { get; set; } = new List<string>();
        public bool NotFound { get; set; }
    }
}
