using System.Collections.Generic;

namespace DictionaryTool.Core.Models
{
    public class VietnameseResult
    {
        public string Word { get; set; }

        /// <summary>Các định nghĩa thô từ Stardict (mỗi phần tử là 1 khối "m" trong entry gốc).</summary>
        public List<string> Definitions { get; set; } = new List<string>();

        public bool NotFound { get; set; }

        public SourceKind Source { get; set; } = SourceKind.None;
    }
}
