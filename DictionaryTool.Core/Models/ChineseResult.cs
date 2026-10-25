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

        public SourceKind Source { get; set; } = SourceKind.None;

        /// <summary>
        /// Tách từng chữ Hán trong từ (vd 老师 → 老, 师) kèm pinyin + nghĩa riêng của
        /// mỗi chữ, tra từ chính CC-CEDICT. Rỗng nếu từ chỉ có 1 ký tự hoặc không tra
        /// được ký tự lẻ nào.
        /// </summary>
        public List<ChineseCharacterInfo> Characters { get; set; } = new List<ChineseCharacterInfo>();
    }
}
