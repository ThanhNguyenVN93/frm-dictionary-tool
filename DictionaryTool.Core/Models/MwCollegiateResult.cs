using System.Collections.Generic;

namespace DictionaryTool.Core.Models
{
    public class MwCollegiateResult
    {
        public List<MwMeaning> Meanings { get; set; } = new List<MwMeaning>();

        /// <summary>Phát âm từ MW Collegiate (hwi.prs). Dùng làm fallback khi dictionaryapi.dev không có audio.</summary>
        public List<Phonetic> Phonetics { get; set; } = new List<Phonetic>();
    }
}
