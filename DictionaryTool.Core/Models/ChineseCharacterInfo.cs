using System.Collections.Generic;

namespace DictionaryTool.Core.Models
{
    /// <summary>Thông tin tra được của 1 ký tự Hán đơn lẻ (dùng cho breakdown từ ghép).</summary>
    public class ChineseCharacterInfo
    {
        public string Character { get; set; }
        public string Pinyin { get; set; }
        public List<string> Definitions { get; set; } = new List<string>();
    }
}
