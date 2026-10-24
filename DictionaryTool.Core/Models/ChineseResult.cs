using System.Collections.Generic;

namespace DictionaryTool.Core.Models
{
    public class ChineseResult
    {
        public string Traditional { get; set; }
        public string Simplified { get; set; }

        /// <summary>Pinyin có dấu thanh dạng số (vd "ni3 hao3"), đúng format gốc CC-CEDICT.</summary>
        public string Pinyin { get; set; }

        /// <summary>Danh sách nghĩa tiếng Anh (mỗi phần tử là 1 "/gloss/" trong entry CC-CEDICT).</summary>
        public List<string> Definitions { get; set; } = new List<string>();

        public bool NotFound { get; set; }
    }
}
