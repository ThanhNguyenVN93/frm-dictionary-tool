using System.Collections.Generic;
using Newtonsoft.Json;

namespace DictionaryTool.Core.Models
{
    public class WordLookupResult
    {
        public string Word { get; set; }

        [JsonIgnore]
        public bool FromCache { get; set; }

        public List<Phonetic> Phonetics { get; set; } = new List<Phonetic>();

        /// <summary>Từ dictionaryapi.dev.</summary>
        public List<DictionaryMeaning> Meanings { get; set; } = new List<DictionaryMeaning>();

        /// <summary>Từ Merriam-Webster (Collegiate + Learner's, xem MwMeaning.Source).</summary>
        public List<MwMeaning> LearnerMeanings { get; set; } = new List<MwMeaning>();

        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }

        /// <summary>
        /// True khi cả 2 nguồn đều trả lời thành công nhưng không tìm thấy từ này
        /// (khác với HasError, vốn dành cho lỗi hệ thống/mạng). UI dùng cờ này để
        /// hiện thông báo "Không tìm thấy từ này" thay vì thông báo lỗi (spec 3.3).
        /// </summary>
        [JsonIgnore]
        public bool NotFound { get; set; }
    }
}
